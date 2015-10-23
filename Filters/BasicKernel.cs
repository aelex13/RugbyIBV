﻿using INFOIBV.Presentation;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace INFOIBV.Filters
{
    public abstract class BasicKernel : IApplicableKernel
    {
        protected IApplicableFilter decoratingKernel;

        protected int width;
        protected int height;
        protected float[,] weights;

        public BasicKernel(IApplicableFilter decoratingKernel, int width, int height, float[,] weights)
        {
            this.decoratingKernel = decoratingKernel;
            this.width = width;
            this.height = height;
            this.weights = weights;
        }

        public Color[,] apply(Color[,] imageToProcess, MainViewModel reportProgressTo)
        {
            if (decoratingKernel != null)
            {
                imageToProcess = decoratingKernel.apply(imageToProcess, reportProgressTo);
            }

            int xOffset = (this.width - 1) / 2;
            int yOffset = (this.height - 1) / 2;
            Color[,] imageToReturn = imageToProcess;

            for (int y = yOffset; y < imageToProcess.GetLength(0) - yOffset; y++) // GetLength(x), where x is the dimension, give you the length of the specified part of the array.
            {
                for (int x = xOffset; x < imageToProcess.GetLength(1) - xOffset; x++)
                {
                    int sum = processPixel(x, y, imageToProcess, reportProgressTo);
                    imageToReturn[x, y] = Color.FromArgb(sum, sum, sum);
                }
            }

            return imageToReturn;
        }

        public abstract int processPixel(int xCoordinate, int yCoordinate, Color[,] imageToProcess, MainViewModel reportProgressTo);

        public virtual double GetMaximumProgress(int imageWidth, int imageHeight)
        {
            if (this.decoratingKernel != null)
                return decoratingKernel.GetMaximumProgress(imageWidth, imageHeight);

            return 0.0;
        }
    }
}