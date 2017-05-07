# MarsImageClassifier
This program was made for the martian club(link coming soon) that was started at my school, [Centaurus High School](http://ceh.bvsd.org/Pages/default.aspx).

# Code
This code is made in c# using the [MonoGame XNA Framework](http://www.monogame.net/) with Visual Studios.
All code for the UI can be found in: MarsImageThing/Game1.cs
The classification algorithum can be found in: MarsImageThing/ClassifyImage.cs and MarsImageThing/SpectralDataPoint.cs

## How The Code Works to Classify The Images
### Key Concepts
In everyday life everything absorbs and reflects light; This in turn, gives it its color. While we humans can only see the visible spectrum (400nm - 700nm) there are still more wavelengths that are also affected by objects in the same way. Although for opportunity only near-UV to near-IR wavelengths are used as filters for the cameras.

![alt text](https://imagine.gsfc.nasa.gov/Images/science/EM_spectrum_compare_level1_lg.jpg "Electromagnetic Spectrum")

### Part 1: Files
In windows, opening a file using their built in using System.Windows.Forms.OpenFileDiolog can be done like this:
```c#
OpenFileDialog openFileDialog = new OpenFileDialog();
openFileDialog.Filter = "Image File (*.jpg)|*.jpg|All files (*.*)|*.*";
openFileDialog.FilterIndex = 1;
openFileDialog.RestoreDirectory = true;
Stream stream;
if (openFileDialog.ShowDialog() == DialogResult.OK)
{
    if ((stream = openFileDialog.OpenFile()) != null)
    {
        Console.WriteLine("Done")
        //Code that uses the stream goes here
    }
}
```
In this example the file is never used, but the file location is saved to the variable stream when ```stream = openFileDialog.OpenFile()``` was used. Also for Part 4 this same methed can be used to save the file, use System.Windows.Forms.SaveFileDiolog.

### Part 2: Declaring Variables
Here, the file paths are converted in to a System.Drawing.BitMap where each pixel can be used independently:
```c#
for(int i = 0; i < ImageLocations.Length; i++)
{
    if (ImageLocations[i] != null)
    {
        Bitmap tempImage = new Bitmap(ImageLocations[i]);
        
        if (tempImage.Size.Height == firstImageSize.Y && tempImage.Size.Width == firstImageSize.X)
            imageList[i] = tempImage;
    }
}
```
The colors list I used for all six points:
```c#
System.Drawing.Color[] colors = { System.Drawing.Color.FromName("Red"), System.Drawing.Color.FromName("Blue"), System.Drawing.Color.FromName("Green"), System.Drawing.Color.FromName("Orange"), System.Drawing.Color.FromName("Gold"), System.Drawing.Color.FromName("Purple") };
```

### Part 3: Comparing Images
This is done by taking the dot product of two vectors, this can be expressed by: ![alt text](https://wikimedia.org/api/rest_v1/media/math/render/svg/f578afaa0ed0f3728d4a6546d11b95456ec84647 "Look it up") Where a and b are both vectors of n dimensions. In this program, we solve for Cos(θ).
```c#
for (int i = 0; i < Points.Count; i++)//both Variation 1 and 2
{
    for (int x = 0; x < ImageSize.X; x++)
    {
        for (int y = 0; y < ImageSize.Y; y++)
        {
            double sum = 0;//could also be an int
            double aMag = 0;
            double bMag = 0;
            for(int j = 0; j < imageList.Length; j++)//dot product
            {
                if (imageList[j] == null)
                    continue;
                aMag += Math.Pow(imageList[j].GetPixel(x, y).B), 2);
                bMag += Math.Pow(imageList[j].GetPixel(Points[i].X, Points[i].Y).B, 2);
                sum += imageList[j].GetPixel(x, y).B, 10) * imageList[j].GetPixel(Points[i].X, Points[i].Y).B;
            }
            aMag = Math.Sqrt(aMag);
            bMag = Math.Sqrt(bMag);

            classiﬁedImage[x, y, i] = (sum) / (aMag * bMag);
        }
    }
}
```
An important thing to note is that when using both the right and left cameras, there must be a smaller x dimension to compensate for the offset of the cameras; Meaning that the value of ```imageSize.X``` will be smaller than the original images. All of this data can be found with the images.

#### Variation 1
Vector values can be set to that same value as the brightness of each pixel, represented by the Red, Green or Blue values (I used the Blue value). This is done for both vectors a and b, where vector a is the current pixel and vector b is the location of the clicked point. 
![alt text](https://raw.githubusercontent.com/ZackJorquera/MarsImageThing/master/README.md%20Images/type%201%20classification.gif)

#### Variation 2
Vector a is set in the same way as in Variation 1, while vector b is set using spectral data from a list of data points. To do this a Reflection vs. wavelength graph must be used as F(x), it should return the % reflection when x is equal to the wavelength. This can be done using the known wavelength of each filter on each camera.
![alt text](https://raw.githubusercontent.com/ZackJorquera/MarsImageThing/master/README.md%20Images/detailsub.png)
To get the wavelengths for each filter use this chart:
![alt text](https://raw.githubusercontent.com/ZackJorquera/MarsImageThing/master/README.md%20Images/CameraFilterCharacteristics.png)

### Part 4: Classification
This is where it iterates over each pixel of the classiﬁedImage and selects the maximum value for the Cos(θ) respective to that pixel and that of vector b.
```c#
for (int x = 0; x < ImageSize.X; x++)
{
    for (int y = 0; y < ImageSize.Y; y++)
    {
        int pointClosest = 6;
        for (int i = 0; i < Points.Count; i++)
        {
            if(pointClosest == 6)
                pointClosest = i;
            else
                if(classifyedImage[x,y,i] > classifyedImage[x,y,pointClosest])
                    pointClosest = i;
        }
        outputImage.SetPixel(x, y, colors[pointClosest]);
    }
}
```

### Part 5: Parallel Computing
To make the previous two steps work faster Parallel Computing was used. This is where the image is being processed at the same time by different threads. The reason why this is a possibility is because each pixel is computed independently of each other pixel.
To acomplish this I used this code:
```c#
ManualResetEvent[] doneEvents = new ManualResetEvent[36];
List<ClassifyPixles> CPList = new List<ClassifyPixles>();
for (int i = 0; i < calculationRectangles.Length; i++)
{
    doneEvents[i] = new ManualResetEvent(false);
    ClassifyPixles CP = new ClassifyPixles(_imageSize, imageList, spectralDataPoints, spectralDataVector, calculationRectangles[i], doneEvents[i]);
    CPList.Add(CP);
    ThreadPool.QueueUserWorkItem(CP.ThreadPoolCallback, i);
}
foreach (var thread in doneEvents)//waits for all of the threads to finish.
    thread.WaitOne();

```
Where ```ClassifyPixles``` is a class that is used to run the thread given by the ```ThreadPool.QueueUserWorkItem(CP.ThreadPoolCallback, i);``` line. The reason why the variable type ManualResetEvent is used is because it allows for the ability to wait for the threads to finish when the time it takes for the thread to calculate is unknown. In this example, the image was split into 36 different parts. These sections are stored in the array ```calculationRectangles``` as a System.Drawing.Rectangle.

The ```ClassifyPixles``` class then computes the pixels in a given range from the ```ThreadPoolCallback``` function. 
```c#
public void ThreadPoolCallback(Object threadContext)
{
    int threadIndex = (int)threadContext;
    Console.WriteLine("thread {0} started...", threadIndex);
    for (int x = _range.X; x < _range.X + _range.Width; x++)
    {
        for (int y = _range.Y; y < _range.Y + _range.Height; y++)
        {
            _pixles[x, y] = FindPixleColor(new System.Drawing.Point(x, y));
        }
    }
    Console.WriteLine("thread {0} result calculated...", threadIndex);
    _doneEvent.Set();
}
```
The threadIndex variable is just for debug purposes, it is not used for any computational purposes.

### Output
Something like: 
![alt text](https://raw.githubusercontent.com/ZackJorquera/MarsImageThing/master/README.md%20Images/OutPut.png "Wow,there is magnetite in rocks on mars!!! The more you know.")
The first point is set to the dirt and dust around the rocks and the second point is set to the spectral data file for magnetite, MarsImageThing/SpectralData/magnetite_hs78.13233.asc, which is commonly found in the rocks on mars.

# Usage
The application file can be ran from: MarsImageThing/Release/MarsImageThing.exe
The images must be in .jpg format and the example images can be found in: MarsImageThing/MartianProjectImages/  
All of the images are from [http://anserver1.eprsl.wustl.edu/](http://anserver1.eprsl.wustl.edu/) from oppritunity.
To add spectral data to classify with, use a .asc file, examples can be found in: MarsImageThing/SpectralData/ or at [https://speclab.cr.usgs.gov/spectral.lib06/](https://speclab.cr.usgs.gov/spectral.lib06/)


# Info
Last edit: 5/7/2017

Made By: Zack Jorquera

Thanks to Samuel Estrella for the original concept for the classification allgorithm.
