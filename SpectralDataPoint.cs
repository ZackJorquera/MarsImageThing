using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MarsImageThing
{
    struct OutPutImageData
    {
        public Stream outPutImageStream;
        public Exception IOErrorException;
    }
    class SpectralData
    {
        public Microsoft.Xna.Framework.Point point = new Microsoft.Xna.Framework.Point();
        public Stream spectralDataPlotLocation;
        public String fileName;
        int cameraImagesAreFrom;
        int[] SpectralWaveLengthForEachFilterLeft = { 739, 753, 673, 601, 535, 482, 432, 440 }; //does not include L0
        int[] SpectralWaveLengthForEachFilterRight = { 436, 754, 803, 864, 904, 934, 1009, 880 };//or R0

        public SpectralData(Microsoft.Xna.Framework.Point Point, Stream SpectralDataPlotLocation, int CameraImagesAreFrom)
        {
            point = Point;
            spectralDataPlotLocation = SpectralDataPlotLocation;

            if (spectralDataPlotLocation != null)
                fileName = (spectralDataPlotLocation as FileStream).Name.Split('\\').Last().Split('.').First();//((FileStream)spectralDataPlotLocation) would of also work

            cameraImagesAreFrom = CameraImagesAreFrom;
        }
        public float[] GetReflectanceVectorFromPlotFile()
        {
            List<int> WaveLengths = new List<int>();
            if (cameraImagesAreFrom == -1)
                WaveLengths = SpectralWaveLengthForEachFilterLeft.ToList();
            else if (cameraImagesAreFrom == 1)
                WaveLengths = SpectralWaveLengthForEachFilterRight.ToList();
            else
            {
                WaveLengths = SpectralWaveLengthForEachFilterLeft.ToList();
                WaveLengths.Concat(SpectralWaveLengthForEachFilterRight.ToList());
            }

            float[,] vector = new float[WaveLengths.Count, 2];
            for (int i = 0; i < vector.Length / 2; i ++ )
            {
                vector[i, 1] = 100;
            }

            using (StreamReader SR = new StreamReader(spectralDataPlotLocation))
            {
                char[] fullText = SR.ReadToEnd().ToCharArray();
                int dataPointNum = 0;
                bool gettingPoint = false;
                bool startLookingForData = false;
                string[] currentDataPoint = { "", "", "" };
                for (int i = 0; i < fullText.LongLength; i++)
                {
                    if (!startLookingForData)
                    {
                        if (i + 15 >= fullText.LongLength)
                            break;
                        startLookingForData = true;
                        for (int j = 0; j < 14; j++)//looks for 14 [Spaces] or [New Line] before starting to collect data. this avoides the junk that the begining
                        {
                            if (fullText[i + j] != (char)32 && fullText[i + j] != (char)10)
                                startLookingForData = false;
                        }
                        continue;
                    }

                    if (fullText[i] == (char)32 || fullText[i] == (char)10 || i == fullText.LongLength - 1)
                    {
                        if (gettingPoint)
                        {
                            if (!(currentDataPoint[dataPointNum % 3].Length <= 2 || currentDataPoint[0] == "-1.23e34"))
                            {
                                if (dataPointNum % 3 == 2)//why 2
                                {
                                    float waveLengthFromFile = float.Parse(currentDataPoint[0]) * 1000;
                                    for (int j = 0; j < WaveLengths.Count; j++)
                                    {
                                        if (currentDataPoint[1] == "-1.23e34")
                                            continue;

                                        float waveLengthToBe = WaveLengths[j];

                                        if (Math.Abs(waveLengthFromFile - waveLengthToBe) < vector[j, 1])
                                        {
                                            vector[j, 0] = float.Parse(currentDataPoint[1]);
                                            vector[j, 1] = Math.Abs(waveLengthFromFile - waveLengthToBe);
                                        }
                                    }
                                }
                            }
                            dataPointNum++;

                        }
                        gettingPoint = false;
                    }
                    else
                    {
                        if (!gettingPoint)
                        {
                            currentDataPoint[dataPointNum % 3] = "";
                        }
                        gettingPoint = true;
                        currentDataPoint[dataPointNum % 3] += fullText[i];
                    }
                }
                SR.Close();
            }
            float[] returnVector = new float[vector.Length / 2];
            for (int i = 0; i < returnVector.Length; i++ )
            {
                returnVector[i] = vector[i, 0];
            }
                
            return returnVector;
        }
    }
}
