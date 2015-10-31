﻿using INFOIBV.Presentation;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace INFOIBV.Filters
{
    public class KirschKernel : BasicKernel
    {
        public KirschKernel(IApplicableFilter decoratingKernel, Direction compass)
            : base(decoratingKernel, 3, 3, KirschKernel.constructWeights(3, 3, compass))
        {
            // What's the heading capt'n?
        }

        public override int processPixel(int xCoordinate, int yCoordinate, Color[,] imageToProcess, MainViewModel reportProgressTo)
        {
            float sum = 0;

            // Loop over Weights
            for (int x = 0; x < this.width; x++)
            {
                int xOffset = x - 1;
                for (int y = 0; y < this.height; y++)
                {
                    int yOffset = y - 1;
                    sum += imageToProcess[xCoordinate + xOffset, yCoordinate + yOffset].R * weights[x, y];
                    reportProgressTo.Progress++;
                }
            }

            return (int)Math.Floor(sum / 30) + 128;
        }

        private static float[,] constructWeights(int width, int height, Direction compass)
        {
            float[,] toReturn = new float[height, width];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float value = 0.0f;
                    if (x != 1 || y != 1)
                    {
                        switch (compass)
                        {
                            case Direction.North:
                                if (y == 0)
                                    value = 5.0f;
                                else
                                    value = -3.0f;
                                break;
                            case Direction.NorthEast:
                                if (x > y)
                                    value = 5.0f;
                                else
                                    value = -3.0f;
                                break;
                            case Direction.East:
                                if (x == 2)
                                    value = 5.0f;
                                else
                                    value = -3.0f;
                                break;
                            case Direction.SouthEast:
                                if (2 - x < y)
                                    value = 5.0f;
                                else
                                    value = -3.0f;
                                break;
                            case Direction.South:
                                if (y == 2)
                                    value = 5.0f;
                                else
                                    value = -3.0f;
                                break;
                            case Direction.SouthWest:
                                if (x < y)
                                    value = 5.0f;
                                else if (x > y)
                                    value = -3.0f;
                                break;
                            case Direction.West:
                                if (x == 0)
                                    value = 5.0f;
                                else if (x == 2)
                                    value = -3.0f;
                                break;
                            case Direction.NorthWest:
                                if (2 - x > y)
                                    value = 5.0f;
                                else
                                    value = -3.0f;
                                break;
                        }
                    }

                    toReturn[x, y] = value;
                }
            }

            return toReturn;
        }

        public override double GetMaximumProgress(int imageWidth, int imageHeight) // Needs to be implemented by every class.
        {
            return base.GetMaximumProgress(imageWidth, imageHeight) + (((imageWidth - 2) * (imageHeight - 2)) * 9);
        }
    }
}