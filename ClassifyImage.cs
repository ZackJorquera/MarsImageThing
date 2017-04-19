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
using System.Threading;


namespace MarsImageThing
{
    class ClassifyImage
    {

        Bitmap outputImage;
        Bitmap[] imageList = new Bitmap[8];
        OutPutImageData outPutData;
        public List<SpectralData> spectralDataPoints = new List<SpectralData>();
        List<float[]> spectralDataVector = new List<float[]>();
        System.Drawing.Color[] colors = { System.Drawing.Color.FromName("Red"), System.Drawing.Color.FromName("Blue"), System.Drawing.Color.FromName("Green"), System.Drawing.Color.FromName("Orange"), System.Drawing.Color.FromName("Gold"), System.Drawing.Color.FromName("Purple") };




        public OutPutImageData Classify(string[] ImageLocations, List<SpectralData> SpectralDataPoints, int cameraImagesAreFrom, Microsoft.Xna.Framework.Point ImageSize)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image File (*.png)|*.png|All files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            System.Drawing.Point _imageSize = new System.Drawing.Point(ImageSize.X, ImageSize.Y);

            spectralDataPoints = SpectralDataPoints;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((outPutData.outPutImageStream = sfd.OpenFile()) != null)
                    {
                        outputImage = new Bitmap(_imageSize.X, _imageSize.Y);


                        for (int i = 0; i < ImageLocations.Length; i++)
                        {
                            if (ImageLocations[i] != null)
                            {
                                Bitmap tempImage = new Bitmap(ImageLocations[i]);
                                if (_imageSize == (new System.Drawing.Point(0, 0)))
                                    _imageSize = new System.Drawing.Point(tempImage.Width, tempImage.Height);
                                if (tempImage.Size.Height == _imageSize.Y && tempImage.Size.Width == _imageSize.X)
                                    imageList[i] = tempImage;
                            }
                        }

                        for (int i = 0; i < spectralDataPoints.Count; i++)
                        {
                            if (spectralDataPoints[i].point == Microsoft.Xna.Framework.Point.Zero)
                                spectralDataVector.Add(spectralDataPoints[i].GetReflectanceVectorFromPlotFile());
                            else
                                spectralDataVector.Add(null);
                        }


                        ManualResetEvent[] doneEvents = new ManualResetEvent[36];
                        List<ClassifyPixles> CPList = new List<ClassifyPixles>();


                        // Configure and launch threads using ThreadPool:
                        System.Drawing.Rectangle[] calculationRectangles = new System.Drawing.Rectangle[36];
                        for (int x = 0; x < 6; x++)//make better if possable
                        {
                            for (int y = 0; y < 6; y++)
                            {
                                calculationRectangles[y + x * 6].X = x * (_imageSize.X + (6 - (_imageSize.X % 6))) / 6;
                                calculationRectangles[y + x * 6].Y = y * (_imageSize.Y + (6 - (_imageSize.Y % 6))) / 6;

                                if (x == 5)
                                    calculationRectangles[y + x * 6].Width = _imageSize.X - (5 * (_imageSize.X + (6 - (_imageSize.X % 6))) / 6);
                                else
                                    calculationRectangles[y + x * 6].Width = (_imageSize.X + (6 - (_imageSize.X % 6))) / 6;

                                if (y == 5)
                                    calculationRectangles[y + x * 6].Height = _imageSize.Y - (5 * (_imageSize.Y + (6 - (_imageSize.Y % 6))) / 6);
                                else
                                    calculationRectangles[y + x * 6].Height = (_imageSize.Y + (6 - (_imageSize.Y % 6))) / 6;
                            }
                        }
                        for (int i = 0; i < calculationRectangles.Length; i++)
                        {
                            doneEvents[i] = new ManualResetEvent(false);
                            ClassifyPixles CP = new ClassifyPixles(_imageSize, imageList, spectralDataPoints, spectralDataVector, calculationRectangles[i], doneEvents[i]);
                            CPList.Add(CP);
                            ThreadPool.QueueUserWorkItem(CP.ThreadPoolCallback, i);
                        }

                        // Wait for all threads in pool to finish
                        foreach (var e in doneEvents)
                            e.WaitOne();

                        foreach (ClassifyPixles CP in CPList)
                        {
                            for (int x = CP.Range.X; x < CP.Range.X + CP.Range.Width; x++)
                            {
                                for (int y = CP.Range.Y; y < CP.Range.Y + CP.Range.Height; y++)
                                {
                                    if (CP.Pixles[x, y] != System.Drawing.Color.Black)
                                        outputImage.SetPixel(x, y, CP.Pixles[x, y]);
                                }
                            }
                        }

                        Console.WriteLine("All calculations are complete.");


                        outputImage.Save(outPutData.outPutImageStream, System.Drawing.Imaging.ImageFormat.Png);
                        outPutData.IOErrorException = null;
                        return outPutData;
                    }
                }
                catch (Exception ex)
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
    class ClassifyPixles
    {
        System.Drawing.Color[] colors = { System.Drawing.Color.FromName("Red"), System.Drawing.Color.FromName("Blue"), System.Drawing.Color.FromName("Green"), System.Drawing.Color.FromName("Orange"), System.Drawing.Color.FromName("Gold"), System.Drawing.Color.FromName("Purple") };
        Bitmap[] _imageList;
        List<SpectralData> _spectralDataPoints;
        List<float[]> _spectralDataVector;

        public ClassifyPixles(System.Drawing.Point _imageSize, Bitmap[] imageList, List<SpectralData> spectralDataPoints, List<float[]> spectralDataVector, System.Drawing.Rectangle range, ManualResetEvent doneEvent)
        {
            _imageList = imageList;
            _spectralDataPoints = spectralDataPoints;
            _spectralDataVector = spectralDataVector;
            _range = range;
            _pixles = new System.Drawing.Color[_imageSize.X, _imageSize.Y];
            _doneEvent = doneEvent;

        }

        // Wrapper method for use with thread pool.
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

        // Recursive method that calculates the Nth Fibonacci number.
        System.Drawing.Color FindPixleColor(System.Drawing.Point thisPoint)
        {

            double[] classifyedPixleValues = new double[_spectralDataPoints.Count];

            for (int i = 0; i < _spectralDataPoints.Count; i++)
            {
                double sum = 0;
                double aMag = 0;
                double bMag = 0;

                for (int j = 0; j < _imageList.Length; j++)//dot product
                {
                    if (_imageList[j] == null)
                        continue;
                    int a = 0;
                    int b = 0;
                    lock (_imageList[j])
                    {
                        a = Convert.ToInt16(_imageList[j].GetPixel(thisPoint.X, thisPoint.Y).B.ToString(), 10);
                        b = Convert.ToInt16(_imageList[j].GetPixel(_spectralDataPoints[i].point.X, _spectralDataPoints[i].point.Y).B.ToString(), 10);
                    }
                    while ((a | b) == 0)
                    {
                        Thread.Sleep(5);
                    }
                    aMag += Math.Pow(a, 2);
                    bMag += Math.Pow((_spectralDataPoints[i].point != Microsoft.Xna.Framework.Point.Zero) ? b : _spectralDataVector[i][j] * 255, 2);
                    sum += a * ((_spectralDataPoints[i].point != Microsoft.Xna.Framework.Point.Zero) ? b : _spectralDataVector[i][j] * 255);
                }
                aMag = Math.Sqrt(aMag);
                bMag = Math.Sqrt(bMag);

                classifyedPixleValues[i] = (sum) / (aMag * bMag);
            }
            int pointClosest = -1;
            for (int i = 0; i < _spectralDataPoints.Count; i++)
            {
                if (pointClosest == -1)
                {
                    pointClosest = i;
                    continue;
                }
                else
                {
                    if (classifyedPixleValues[i] > classifyedPixleValues[pointClosest])
                    {
                        pointClosest = i;
                    }
                }
            }
            return colors[pointClosest];

        }

        public System.Drawing.Rectangle Range { get { return _range; } }
        private System.Drawing.Rectangle _range;

        public System.Drawing.Color[,] Pixles { get { return _pixles; } }
        private System.Drawing.Color[,] _pixles;

        private ManualResetEvent _doneEvent;
    }
}
