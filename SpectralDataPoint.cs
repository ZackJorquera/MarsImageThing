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
    class SpectralData
    {
        public Microsoft.Xna.Framework.Point point = new Microsoft.Xna.Framework.Point();
        public float[] vector = new float[8];
        public int[] SpectralWaveLengthForEachFilterLeft = { 739,753,673,601,535,482,432,440}; //4.00-1.000 microMeters
        public int[] SpectralWaveLengthForEachFilterRight = { 739, 753, 673, 601, 535, 482, 432, 440 }; //4.00-1.000 microMeters

        public SpectralData(Microsoft.Xna.Framework.Point Point , Stream SpectralDataPlotLocation)
        {
            if(SpectralDataPlotLocation != null)
                vector = GetReflectanceVectorFromPlotFile(SpectralWaveLengthForEachFilterLeft, SpectralDataPlotLocation);
            point = Point;
            
        }
        float[] GetReflectanceVectorFromPlotFile(int[] WaveLengths, Stream SpectralDataPlotLocation)
        {
            float[] vector = new float[8];

            using(StreamReader SR = new StreamReader(SpectralDataPlotLocation))
            {
                char[] fullText = SR.ReadToEnd().ToCharArray();
                int dataPointNum = 0;
                bool gettingPoint = false;
                bool startLookingForData = false;
                string[] currentDataPoint = {"","",""};
                for(int i = 0; i < fullText.LongLength; i++)
                {
                    if (!startLookingForData)
                    {
                        if(i + 15 == fullText.LongLength)
                            break;
                        startLookingForData = true;
                        for (int j = 0; j < 14; j++ )//looks for 14 [spaces] before starting to collect data. this avoides the junk that the begining
                        {
                            if (fullText[i + j] != ' ')
                                startLookingForData = false;
                        }
                        continue;
                    }

                    if (fullText[i] == ' ' || i == fullText.LongLength-1)
                    {
                        if(gettingPoint)
                        {
                            if (currentDataPoint[dataPointNum % 3].Length <= 2 || currentDataPoint[0] == "-1.23e34")
                                continue;
                            if (dataPointNum % 3 == 2)
                            {

                                float waveLength = float.Parse(currentDataPoint[0]) * 1000;
                                for(int j = 0; j < WaveLengths.Length;j++)
                                {
                                    float waveLengthToGet = (float)WaveLengths[j];

                                    if(waveLength == waveLengthToGet)
                                    {
                                        vector[j] = float.Parse(currentDataPoint[1]);
                                    }
                                }
                            }
                            dataPointNum++;
                            
                        }
                        gettingPoint = false;
                    }
                    else
                    {
                        if(!gettingPoint)
                        {
                            currentDataPoint[dataPointNum % 3] = "";
                        }
                        gettingPoint = true;
                        currentDataPoint[dataPointNum % 3] += fullText[i];
                    }
                }
            }
            return vector;
        }
    }
}
