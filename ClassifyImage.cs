using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Xna.Framework;

namespace MarsImageThing
{
    class ClassifyImage
    {

        public OutPutImageData Classify(string[] ImageLocations, List<SpectralData> spectralDataPoints, int cameraImagesAreFrom, Microsoft.Xna.Framework.Point ImageSize)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image File (*.png)|*.png|All files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            OutPutImageData outPutData;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((outPutData.outPutImageStream = sfd.OpenFile()) != null)
                    {
                        Bitmap outputImage = new Bitmap(ImageSize.X, ImageSize.Y);
                        System.Drawing.Color[] colors = { System.Drawing.Color.FromName("Red"), System.Drawing.Color.FromName("Blue"), System.Drawing.Color.FromName("Green"), System.Drawing.Color.FromName("Orange"), System.Drawing.Color.FromName("Gold"), System.Drawing.Color.FromName("Purple") };
                        Bitmap[] imageList = new Bitmap[8];
                        List<float[]> spectralDataVector = new List<float[]>();


                        for(int i = 0; i < ImageLocations.Length; i++)
                        {
                            if (ImageLocations[i] != null)
                            {
                                Bitmap tempImage = new Bitmap(ImageLocations[i]);
                                if (ImageSize == Microsoft.Xna.Framework.Point.Zero)
                                    ImageSize = new Microsoft.Xna.Framework.Point(tempImage.Width, tempImage.Height);
                                if (tempImage.Size.Height == ImageSize.Y && tempImage.Size.Width == ImageSize.X)
                                    imageList[i] = tempImage;
                            }
                        }

                        for(int i = 0; i < spectralDataPoints.Count; i ++)
                        {
                            if (spectralDataPoints[i].point == Microsoft.Xna.Framework.Point.Zero)
                                spectralDataVector.Add(spectralDataPoints[i].GetReflectanceVectorFromPlotFile());
                            else
                                spectralDataVector.Add(null);
                        }

                        double[, ,] classifyedImage = new double[ImageSize.X, ImageSize.Y, spectralDataPoints.Count];

                        for (int i = 0; i < spectralDataPoints.Count; i++)
                        {
                            for (int x = 0; x < ImageSize.X; x++)
                            {
                                for (int y = 0; y < ImageSize.Y; y++)
                                {
                                    double sum = 0;
                                    double aMag = 0;
                                    double bMag = 0;
                                    
                                    for(int j = 0; j < imageList.Length; j++)//dot product
                                    {
                                        if (imageList[j] == null)
                                            continue;
                                        aMag += Math.Pow(Convert.ToInt16(imageList[j].GetPixel(x, y).B.ToString(), 10), 2);
                                        bMag += Math.Pow((spectralDataPoints[i].point != Microsoft.Xna.Framework.Point.Zero) ? Convert.ToInt16(imageList[j].GetPixel(spectralDataPoints[i].point.X, spectralDataPoints[i].point.Y).B.ToString(), 10) : spectralDataVector[i][j] * 255, 2);
                                        sum += Convert.ToInt16(imageList[j].GetPixel(x, y).B.ToString(), 10) * ((spectralDataPoints[i].point != Microsoft.Xna.Framework.Point.Zero) ? Convert.ToInt16(imageList[j].GetPixel(spectralDataPoints[i].point.X, spectralDataPoints[i].point.Y).B.ToString(), 10) : spectralDataVector[i][j] * 255);
                                    }
                                    aMag = Math.Sqrt(aMag);
                                    bMag = Math.Sqrt(bMag);

                                    classifyedImage[x, y, i] = (sum) / (aMag * bMag);
                                }
                            }
                        }

                        for (int x = 0; x < ImageSize.X; x++)
                        {
                            for (int y = 0; y < ImageSize.Y; y++)
                            {
                                int pointClosest = -1;
                                for (int i = 0; i < spectralDataPoints.Count; i++)
                                {
                                    if(pointClosest == -1)
                                    {
                                        pointClosest = i;
                                        continue;
                                    }
                                    else
                                    {
                                        if(classifyedImage[x,y,i] > classifyedImage[x,y,pointClosest])
                                        {
                                             pointClosest = i;
                                        }
                                    }
                                }
                                outputImage.SetPixel(x, y, colors[pointClosest]);
                            }
                        }
                        outputImage.Save(outPutData.outPutImageStream, System.Drawing.Imaging.ImageFormat.Png);
                        outPutData.IOErrorException = null;
                        return outPutData;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    outPutData.IOErrorException = ex;
                    outPutData.outPutImageStream = null;
                    return outPutData;

                }
            }
            return new OutPutImageData();
        }
    }
}
