# MarsImageClassifyer about
This program was made for the martion club(link coming soon) that was started in my school([CHS](http://ceh.bvsd.org/Pages/default.aspx)). 
This program takes in 2-6 black and white images of size 256X256. the program will get the dot product of each pixle using the images intensity as the point where the vector is facing in each demintion and comparing that to points on the image that can be selected. 

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
In this example the file is never used, but the file location is saved to the variable stream when stream = openFileDialog2.OpenFile() was used.

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
This is done by taking the dot puduct of two vecotrs The dot product can be expressed by the equation ![alt text](https://wikimedia.org/api/rest_v1/media/math/render/svg/f578afaa0ed0f3728d4a6546d11b95456ec84647, "Look it up")

# Usage
The application file can be ran from: MarsImageThing/Release/MarsImageThing.exe
The images must be in .jpg format and the example images can be found in: MarsImageThing/MartianProjectImages/
To add spectral data to classify with, use a .asc file, examples can be found in: (coming soon) or at https://speclab.cr.usgs.gov/spectral.lib06/


# Info
Last edit: 3/8/2017

Made By: Zack Jorquera

All right reserved
