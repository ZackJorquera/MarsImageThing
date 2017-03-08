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

        public Stream Classify(string[] ImageLocations, List<Microsoft.Xna.Framework.Point> points, bool selfClassification)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image File (*.png)|*.png|All files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            Stream stream;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((stream = sfd.OpenFile()) != null)
                    {
                        Bitmap outputImage = new Bitmap(256,256);
                        System.Drawing.Color[] colors = { System.Drawing.Color.FromName("Red"), System.Drawing.Color.FromName("Blue"), System.Drawing.Color.FromName("Green"), System.Drawing.Color.FromName("Orange"), System.Drawing.Color.FromName("Gold"), System.Drawing.Color.FromName("Purple") };
                        List<Bitmap> imageList = new List<Bitmap>();

                        for(int i = 0; i < ImageLocations.Length; i++)
                        {
                            if (ImageLocations[i] != null)
                            {
                                Bitmap tempImage = new Bitmap(ImageLocations[i]);
                                if (tempImage.Size.Height == tempImage.Size.Width && tempImage.Size.Width == 256)
                                    imageList.Add(tempImage);
                            }
                        }

                        double[, ,] classifyedImage = new double[256, 256, points.Count];

                        for (int i = 0; i < points.Count; i++)
                        {
                            for (int x = 0; x < 256; x++)
                            {
                                for (int y = 0; y < 256; y++)
                                {
                                    int sum = 0;
                                    double aMag = 0;
                                    double bMag = 0;
                                    for(int j = 0; j < imageList.Count; j++)
                                    {
                                        aMag += Math.Pow(Convert.ToInt16(imageList[j].GetPixel(x, y).B.ToString(), 10), 2);
                                        bMag += Math.Pow(Convert.ToInt16(imageList[j].GetPixel(points[i].X, points[i].Y).B.ToString(), 10),2);
                                        sum += Convert.ToInt16(imageList[j].GetPixel(x, y).B.ToString(), 10) * Convert.ToInt16(imageList[j].GetPixel(points[i].X, points[i].Y).B.ToString(), 10);
                                    }
                                    aMag = Math.Sqrt(aMag);
                                    bMag = Math.Sqrt(bMag);

                                    classifyedImage[x, y, i] = ((double)sum) / (aMag * bMag);
                                }
                            }
                        }

                        for (int x = 0; x < 256; x++)
                        {
                            for (int y = 0; y < 256; y++)
                            {
                                int pointClosest = 6;
                                for(int i = 0; i < points.Count; i++)
                                {
                                    if(pointClosest == 6)
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
                        
                            outputImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            return stream;
                        
                    }
                }
                catch(Exception ex)
                {
                }
            }
        return null;
        }
    }
}
