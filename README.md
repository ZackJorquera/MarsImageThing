# MarsImageClassifyer about
This program was made for the martion club(link coming soon) that was started at my school, [Centaurus High School](http://ceh.bvsd.org/Pages/default.aspx).

# Code
This code uses the [MonoGame XNA Framework](http://www.monogame.net/) with visual studios.
All code for the UI can be found in: MarsImageThing/Game1.cs
The classification algorithum can be found in: MarsImageThing/ClassifyImage.cs

## How the code works to classify the images
### Intro
in everyday life, everything absorbs and reflects light, this in turn gives it its color. while we humans can only see the visable spectrum (400nm - 750nm) there are still more wavelengths that are also effected be objects in the same way.

![alt text](https://imagine.gsfc.nasa.gov/Images/science/EM_spectrum_compare_level1_lg.jpg "Electromagnetic Spectrum")

### Part 1: Files
In windows, opening a file using their built in using System.Windows.Forms.OpenFileDiolog can be done like this (found in Game1.cs):
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
In this example the file is never used, but the file location is saved to the variable stream when stream = openFileDialog2.OpenFile() was used. Also for Part 4 this same methed can be used to save the file, but use System.Windows.Forms.SaveFileDiolog.

### Part 2: Declaring Variables
Here the file paths are converted in to a System.Drawing.BitMap where each pixel can be accesed sepretly:
```c#
for(int i = 0; i < ImageLocations.Length; i++)
{
    if (ImageLocations[i] != null)
    {
        Bitmap tempImage = new Bitmap(ImageLocations[i]);
        if (tempImage.Size.Height == tempImage.Size.Width && tempImage.Size.Width == 256)
            imageList[i] = tempImage;
    }
}
```
Along side this not much else is really needed  exept for the colors list.

### Part 3: Comparing images
This is done by taking the dot puduct of two vecotrs The dot product can be expressed by the equation ![alt text](https://wikimedia.org/api/rest_v1/media/math/render/svg/f578afaa0ed0f3728d4a6546d11b95456ec84647 "Look it up") where a and b are both vectors of n dementions. In the promram will will salve for Cos(θ).
```c#
for (int i = 0; i < Points.Count; i++)//both Variation 1 and 2
{
    for (int x = 0; x < 256; x++)
    {
        for (int y = 0; y < 256; y++)
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
#### Variation 1
vector a can be set to be at the brightness of each pixle which is repusented as the R, B or G value. This is done for bith a and b where a is the current pixle and b is the the pixel at the clicked point. (image coming soon)
#### Variation 2
vector a is set in the same way as in Variation 1, but vector b is set using spectral data (this is where spectroscapy comes in). To do this a Reflection vs. wavelength graph must be used. so F(x) returns the % reflection where x is the wavelength, and knowing the wavelenght of each filter for each camera and get the a vector to compair to.
(Image coming soon)

### Part 4: Classification
This is where it iterate over each pixel of the classiﬁedImage and select the maximum value for the Cos(θ) respective to that pixel and that of vector b's pixel.
```c#
for (int x = 0; x < 256; x++)
{
    for (int y = 0; y < 256; y++)
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

# Usage
The application file can be ran from: MarsImageThing/Release/MarsImageThing.exe
The images must be in .jpg format and the example images can be found in: MarsImageThing/MartianProjectImages/
To add spectral data to classify with, use a .asc file, examples can be found in: (coming soon) or at [https://speclab.cr.usgs.gov/spectral.lib06/](https://speclab.cr.usgs.gov/spectral.lib06/)


# Info
Last edit: 3/8/2017

Made By: Zack Jorquera

All right reserved
