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
    struct OutPutImageData//This allows for the classify function to output any errors that occured and the image
    {
        public Stream outPutImageStream;
        public Exception IOErrorException;
    }

    class SpectralData
    {
        int _cameraImagesAreFrom;
        int[] SpectralWaveLengthForEachFilterLeft = new int[8];// = { 739, 753, 673, 601, 535, 482, 432, 440 }; //filter wavelenght for filters L12345678
        int[] SpectralWaveLengthForEachFilterRight = new int[8];// = { 436, 754, 803, 864, 904, 934, 1009, 880 };//filter wavelenght for filters R12345678
        //red green blue filters are L2 L5 L7 or L3 L5 L6


        public SpectralData(Microsoft.Xna.Framework.Point Point, Stream SpectralDataPlotLocation, int CameraImagesAreFrom)
        {
            _point = Point;
            _spectralDataPlotLocation = SpectralDataPlotLocation;

            if (_spectralDataPlotLocation != null)
                _fileName = (_spectralDataPlotLocation as FileStream).Name.Split('\\').Last().Split('.').First();//((FileStream)spectralDataPlotLocation) would of also work

            _cameraImagesAreFrom = CameraImagesAreFrom;
        }
        public float[] GetReflectanceVectorFromPlotFile()
        {
            string TempFile = Path.GetTempPath() + "SpectralData.CHSMars";

            using (StreamReader SRT = new StreamReader(TempFile))
            {
                char[] SpectralWaveLengthForEachFilter = SRT.ReadToEnd().ToCharArray();
                bool collectingForRight = false;
                bool collectingData = false;
                string thisString = "";
                int filterOn = 0;

                for(int i = 0; i < TempFile.Length; i ++)
                {
                    if (SpectralWaveLengthForEachFilter[i] == '\n')
                    {
                        collectingForRight = true;
                        collectingData = false;
                        filterOn = 0;
                    }

                    if (collectingData && SpectralWaveLengthForEachFilter[i] != '^')
                        thisString += SpectralWaveLengthForEachFilter[i];

                    if (SpectralWaveLengthForEachFilter[i] == '^')
                    {
                        if (collectingData)
                        {
                            if (collectingForRight)
                                SpectralWaveLengthForEachFilterRight[filterOn] = (int)float.Parse(thisString);
                            else
                                SpectralWaveLengthForEachFilterLeft[filterOn] = (int)float.Parse(thisString);
                            thisString = null;
                            filterOn++;
                        }
                        else
                        {
                            collectingData = true;
                            
                        }
                    }
                }
                SRT.Close();
            }


            List<int> WaveLengths = new List<int>();
            if (_cameraImagesAreFrom == -1)//gets the used data wavelenghts to get reflectance data from
                WaveLengths = SpectralWaveLengthForEachFilterLeft.ToList();
            else if (_cameraImagesAreFrom == 1)
                WaveLengths = SpectralWaveLengthForEachFilterRight.ToList();
            else
            {
                WaveLengths = SpectralWaveLengthForEachFilterLeft.ToList();
                WaveLengths.Concat(SpectralWaveLengthForEachFilterRight.ToList());
            }

            float[,] vector = new float[WaveLengths.Count, 2];
            for (int i = 0; i < vector.Length / 2; i++)
            {
                vector[i, 1] = 100;
            }

            using (StreamReader SR = new StreamReader(_spectralDataPlotLocation))
            {
                char[] fullText = SR.ReadToEnd().ToCharArray();
                int dataPointNum = 0;
                bool gettingPoint = false;//know when it is reading junk or the values needed
                bool startLookingForData = false;//means that the next values have data point in them
                string[] currentDataPoint = { "", "", "" };//wavelength    reflectance    standard deviation
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
                                if (dataPointNum % 3 == 2)
                                {
                                    float waveLengthFromFile = float.Parse(currentDataPoint[0]) * 1000;
                                    for (int j = 0; j < WaveLengths.Count; j++)
                                    {
                                        if (float.Parse(currentDataPoint[1]) == -1.23e34f)
                                            continue;

                                        float waveLengthToBe = WaveLengths[j];

                                        if (Math.Abs(waveLengthFromFile - waveLengthToBe) < vector[j, 1])
                                        {
                                            vector[j, 0] = float.Parse(currentDataPoint[1]);
                                            vector[j, 1] = Math.Abs(waveLengthFromFile - waveLengthToBe);//uses this to find the closest value
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
            float[] returnVector = new float[WaveLengths.Count];
            for (int i = 0; i < returnVector.Length; i++)
            {
                returnVector[i] = vector[i, 0];
            }

            return returnVector;
        }

        public Microsoft.Xna.Framework.Point Point { get { return _point; } }
        private Microsoft.Xna.Framework.Point _point;

        public Stream SpectralDataPlotLocation { get { return _spectralDataPlotLocation; } }
        private Stream _spectralDataPlotLocation;

        public String FileName { get { return _fileName; } }
        private String _fileName;

    }
}