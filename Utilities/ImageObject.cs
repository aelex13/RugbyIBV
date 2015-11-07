﻿using INFOIBV.Utilities.Enums;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.Utilities
{
    public class ImageObject
    {
        protected int[,] pixels;
        protected int[,] perimeterPixels;
        protected List<ListPixel> perimeterListPixels;
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }

        public ImageObject(List<ListPixel> pixelsToProcess)
        {
            Area = 0;
            Perimeter = 0;
            Compactness = 0.0;
            Roundness = 0.0;
            LongestChord = null;
            LongestPerpendicularChord = null;
            BoundingBoxArea = 0;
            Rectangularity = 0.0;
            Elongation = 0.0;
            Elongation2 = 0.0;

            OffsetX = int.MaxValue;
            OffsetY = int.MaxValue; // To make sure it changes on the first time
            int offsetXMax = -1;
            int offsetYMax = -1;

            for (int i = 0; i < pixelsToProcess.Count; i++)
            {
                if (pixelsToProcess[i].X < OffsetX)
                    OffsetX = pixelsToProcess[i].X;
                if (pixelsToProcess[i].Y < OffsetY)
                    OffsetY = pixelsToProcess[i].Y;
                if (pixelsToProcess[i].X > offsetXMax)
                    offsetXMax = pixelsToProcess[i].X;
                if (pixelsToProcess[i].Y > offsetYMax)
                    offsetYMax = pixelsToProcess[i].Y;
            }

            int sizeX = offsetXMax - (OffsetX - 1);
            int sizeY = offsetYMax - (OffsetY - 1);

            pixels = new int[sizeX, sizeY];
            foreach (var pixel in pixelsToProcess)
                pixels[pixel.X - OffsetX, pixel.Y - OffsetY] = 1;

            for (int i = 0; i < pixels.GetLength(0); i++)
                for (int j = 0; j < pixels.GetLength(1); j++)
                    if (pixels[i, j] == 1)
                        continue;
                    else
                        pixels[i, j] = 0;

            Console.WriteLine("Succesfully constructed an image object");
            Console.WriteLine("The image object has the following properties:");
            Console.WriteLine("OffsetX: {0}, OffsetY: {1}", OffsetX, OffsetY);
            Console.WriteLine("SizeX: {0}, SizeY: {1}", sizeX, sizeY);
            Console.WriteLine("Area: {0}", Area);
            Console.WriteLine("Perimeter: {0}", Math.Round(Perimeter, 2)); // Nicely round off
            Console.WriteLine("Compactness: {0}", Math.Round(Compactness, 2)); // Nicely round off
            Console.WriteLine("Roundness: {0}", Math.Round(Roundness, 2)); // Nicely round off
            Console.WriteLine("Longest Chord Info: 1st pixel.x={0}, 1st pixel.y={1}, 2nd pixel.x={2}, 2nd pixel.y={3}, Distance between points={4}, Longest Chord Orientation={5}", LongestChord.firstPixel.X, LongestChord.firstPixel.Y, LongestChord.secondPixel.X, LongestChord.secondPixel.Y, Math.Round(LongestChord.distance, 2), Math.Round(LongestChord.orientation, 2));
            Console.WriteLine("Longest Perpendicular Chord Info: 1st pixel.x={0}, 1st pixel.y={1}, 2nd pixel.x={2}, 2nd pixel.y={3}, Distance between points={4}, Longest Perpendicular Chord Orientation={5}", LongestPerpendicularChord.firstPixel.X, LongestPerpendicularChord.firstPixel.Y, LongestPerpendicularChord.secondPixel.X, LongestPerpendicularChord.secondPixel.Y, Math.Round(LongestPerpendicularChord.distance, 2), Math.Round(LongestPerpendicularChord.orientation, 2));
            Console.WriteLine("Eccentricity: {0}", Math.Round(Eccentricity, 2)); // Nicely round off
            Console.WriteLine("BoundingBoxArea: {0}", BoundingBoxArea); // Nicely round off
            Console.WriteLine("Rectangularity: {0}", Math.Round(Rectangularity, 2)); // Nicely round off
            Console.WriteLine("Elongation: {0}", Math.Round(Elongation, 2)); // Nicely round off
            Console.WriteLine("Elongation2: {0}", Math.Round(Elongation2, 2)); // Nicely round off


            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("");
        }

        protected List<ListPixel> ConvertPerimeterPixelsToList()
        {
            if (perimeterPixels == null)
                EstablishPerimeterPixels();

            List<ListPixel> list = new List<ListPixel>();

            for (int i = 0; i < perimeterPixels.GetLength(0); i++)
                for (int j = 0; j < perimeterPixels.GetLength(1); j++)
                    if (perimeterPixels[i, j] == 1)
                        list.Add(new ListPixel(i, j, null));

            return list;
        }

        protected void EstablishPerimeterPixels()
        {
            int fX = -1; // First X
            int fY = -1; // First Y
            for (int i = 0; i < pixels.GetLength(0); i++) // Would only loop once, if things were done right
            {
                for (int j = 0; j < pixels.GetLength(1); j++)
                {
                    if (pixels[i, j] == 1)
                    {
                        fX = i;
                        fY = j;
                        break;
                    }
                }

                if (fX != -1 && fY != -1)
                    break;
            }

            if (fX == -1 && fY == -1)
                return; // Error, should not come to this. Could not find a pixel of the object.

            int nX = fX; // Next X
            int nY = fY; // Next Y
            Direction lookingDirection = Direction.North; // Begin looking North

            perimeterPixels = new int[pixels.GetLength(0), pixels.GetLength(1)];
            for (int i = 0; i < perimeterPixels.GetLength(0); i++)
                for (int j = 0; j < perimeterPixels.GetLength(1); j++)
                    perimeterPixels[i, j] = 0;

            double perimeterCounter = 0.0;

            do
            {
                Direction newDirection = lookingDirection;
                bool hasADirection = false;
                // 1 loop to rule them all
                for (int i = -2; i < 3; i++)
                {
                    if (CanTurn(nX, nY, newDirection, i))
                    { // Found a direction to go to
                        hasADirection = true;
                        if (i != 0) // No turning needed
                            newDirection = Turn(nX, nY, newDirection, i);

                        break;
                    }
                }

                if (!hasADirection) // If you have no direction
                    newDirection = Turn(nX, nY, newDirection, 4); // GO BACK (to the choppa)!

                switch (newDirection)
                {
                    case Direction.NorthEast:
                    case Direction.SouthEast:
                    case Direction.SouthWest:
                    case Direction.NorthWest:
                        perimeterCounter += Math.Sqrt(2);
                        break;
                    case Direction.North:
                    case Direction.East:
                    case Direction.South:
                    case Direction.West:
                        perimeterCounter += 1.0;
                        break;
                }

                Traverse(ref nX, ref nY, newDirection); // Sets new nX and nY
                lookingDirection = newDirection;
                perimeterPixels[nX, nY] = 1;

            } while (nX != fX || nY != fY);

            Perimeter = perimeterCounter;
        }

        private bool CanTurn(int x, int y, Direction direction, int turnDirection)
        {
            int numDirection = -1;
            switch (direction) // Look in new direction
            {
                case Direction.North:
                    numDirection = 0;
                    break;
                case Direction.NorthEast:
                    numDirection = 1;
                    break;
                case Direction.East:
                    numDirection = 2;
                    break;
                case Direction.SouthEast:
                    numDirection = 3;
                    break;
                case Direction.South:
                    numDirection = 4;
                    break;
                case Direction.SouthWest:
                    numDirection = 5;
                    break;
                case Direction.West:
                    numDirection = 6;
                    break;
                case Direction.NorthWest:
                    numDirection = 7;
                    break;
                default:
                    return false;
            }

            int newNumDirection = -1;
            if (numDirection + turnDirection < 0)
                newNumDirection = 8 + (numDirection + turnDirection);
            else
                newNumDirection = (numDirection + turnDirection) % 8;

            switch (newNumDirection)
            {
                case 0: // North
                    if (y - 1 >= 0)
                        return pixels[x, y - 1] == 1;
                    break;
                case 1: // NorthEast
                    if (x + 1 < pixels.GetLength(0) && y - 1 >= 0)
                        return pixels[x + 1, y - 1] == 1;
                    break;
                case 2: // East
                    if (x + 1 < pixels.GetLength(0))
                        return pixels[x + 1, y] == 1;
                    break;
                case 3: // SouthEast
                    if (x + 1 < pixels.GetLength(0) && y + 1 < pixels.GetLength(1))
                        return pixels[x + 1, y + 1] == 1;
                    break;
                case 4: // South
                    if (y + 1 < pixels.GetLength(1))
                        return pixels[x, y + 1] == 1;
                    break;
                case 5: // SouthWest
                    if (x - 1 >= 0 && y + 1 < pixels.GetLength(1))
                        return pixels[x - 1, y + 1] == 1;
                    break;
                case 6: // West
                    if (x - 1 >= 0)
                        return pixels[x - 1, y] == 1;
                    break;
                case 7: // NorthWest
                    if (x - 1 >= 0 && y - 1 >= 0)
                        return pixels[x - 1, y - 1] == 1;
                    break;
                default:
                    return false;
            }

            return false;
        }

        private Direction Turn(int x, int y, Direction direction, int turnDirection)
        {
            int numDirection = -1;
            switch (direction) // Look in new direction
            {
                case Direction.North:
                    numDirection = 0;
                    break;
                case Direction.NorthEast:
                    numDirection = 1;
                    break;
                case Direction.East:
                    numDirection = 2;
                    break;
                case Direction.SouthEast:
                    numDirection = 3;
                    break;
                case Direction.South:
                    numDirection = 4;
                    break;
                case Direction.SouthWest:
                    numDirection = 5;
                    break;
                case Direction.West:
                    numDirection = 6;
                    break;
                case Direction.NorthWest:
                    numDirection = 7;
                    break;
                default:
                    return direction;
            }

            int newNumDirection = -1;
            if (numDirection + turnDirection < 0)
                newNumDirection = 8 + (numDirection + turnDirection);
            else
                newNumDirection = (numDirection + turnDirection) % 8;

            switch (newNumDirection)
            {
                case 0: // North
                    if (y - 1 >= 0)
                        return pixels[x, y - 1] == 1 ? Direction.North : direction;
                    break;
                case 1: // NorthEast
                    if (x + 1 < pixels.GetLength(0) && y - 1 >= 0)
                        return pixels[x + 1, y - 1] == 1 ? Direction.NorthEast : direction;
                    break;
                case 2: // East
                    if (x + 1 < pixels.GetLength(0))
                        return pixels[x + 1, y] == 1 ? Direction.East : direction;
                    break;
                case 3: // SouthEast
                    if (x + 1 < pixels.GetLength(0) && y + 1 < pixels.GetLength(1))
                        return pixels[x + 1, y + 1] == 1 ? Direction.SouthEast : direction;
                    break;
                case 4: // South
                    if (y + 1 < pixels.GetLength(1))
                        return pixels[x, y + 1] == 1 ? Direction.South : direction;
                    break;
                case 5: // SouthWest
                    if (x - 1 >= 0 && y + 1 < pixels.GetLength(1))
                        return pixels[x - 1, y + 1] == 1 ? Direction.SouthWest : direction;
                    break;
                case 6: // West
                    if (x - 1 >= 0)
                        return pixels[x - 1, y] == 1 ? Direction.West : direction;
                    break;
                case 7: // NorthWest
                    if (x - 1 >= 0 && y - 1 >= 0)
                        return pixels[x - 1, y - 1] == 1 ? Direction.NorthWest : direction;
                    break;
                default:
                    return direction;
            }

            return direction;
        }

        private void Traverse(ref int x, ref int y, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    y--;
                    return;
                case Direction.NorthEast:
                    x++;
                    y--;
                    return;
                case Direction.East:
                    x++;
                    return;
                case Direction.SouthEast:
                    x++;
                    y++;
                    return;
                case Direction.South:
                    y++;
                    return;
                case Direction.SouthWest:
                    x--;
                    y++;
                    return;
                case Direction.West:
                    x--;
                    return;
                case Direction.NorthWest:
                    x--;
                    y--;
                    return;
                default: // Shouldn't come to this. Unauthorized Direction used.
                    x = -1;
                    y = -1;
                    return;
            }
        }

        private int _area;
        public int Area
        {
            get
            {
                if (_area == 0)
                    foreach (var pixel in pixels)
                        if (pixel == 1)
                            _area++;

                return _area;
            }
            set { _area = value; }
        }

        private double _perimeter;
        public double Perimeter
        {
            get
            {
                if (_perimeter == 0)
                    EstablishPerimeterPixels(); // If needed, redo the perimeterPixels, but count this time

                return _perimeter;
            }
            set { _perimeter = value; } // Can be 1's but also Sqrt(2)'s
        }

        private double _compactness;
        public double Compactness
        {
            get
            {
                if (_compactness == 0)
                {
                    _compactness = Math.Pow(Perimeter, 2.0) / (((double)Area * 4.0) * Math.PI);
                }

                return _compactness;
            }
            set { _compactness = value; }
        }

        private double _roundness;
        public double Roundness
        {
            get
            {
                if (_roundness == 0)
                    _roundness = 1.0 / Compactness;

                return _roundness;
            }
            set { _roundness = value; }
        }

        private Chord _longestChord;
        public Chord LongestChord
        {
            get
            {
                if (_longestChord == null)
                {
                    if (this.perimeterListPixels == null)
                    {
                        this.perimeterListPixels = ConvertPerimeterPixelsToList();
                    }

                    double longestDistance = Double.NegativeInfinity;
                    double distance = 0;
                    ListPixel firstPoint = new ListPixel(0, 0, new bool[0, 0]);
                    ListPixel secondPoint = new ListPixel(0, 0, new bool[0, 0]);

                    for (int i = 0; i < this.perimeterListPixels.Count; i++)
                    {
                        if (i + 1 >= this.perimeterListPixels.Count)
                        {
                            continue; // Skip the last loop
                        }
                        ListPixel toCalcFrom = this.perimeterListPixels[i];

                        for (int j = i + 1; j < this.perimeterListPixels.Count; j++)
                        {
                            ListPixel toCalcTo = this.perimeterListPixels[j];
                            distance = Chord.calcDistance(toCalcFrom, toCalcTo); ;
                            if (distance > longestDistance)
                            {
                                longestDistance = distance;
                                firstPoint = toCalcFrom;
                                secondPoint = toCalcTo;
                            }
                        }
                    }
                    Chord toReturn = new Chord(firstPoint, secondPoint, longestDistance);
                    _longestChord = toReturn;
                }
                return _longestChord;
            }
            set { _longestChord = value; }
        }

        private Chord _longestPerpendicularChord;
        public Chord LongestPerpendicularChord
        {
            get
            {
                if (_longestPerpendicularChord == null)
                {
                    Chord longestchord = this.LongestChord;
                    double angle = longestchord.orientation;

                    Vector<double> unitVector1 = new DenseVector(new double[] { Math.Cos(angle), Math.Sin(angle) });
                    Vector<double> unitVector2 = new DenseVector(new double[] { Math.Cos(angle + 90), Math.Sin(angle + 90) });

                    Matrix<double> transformationMatrix = DenseMatrix.OfColumnVectors(unitVector1, unitVector2);

                    List<Vector<double>> transformedListPixels = new List<Vector<double>>();
                    Dictionary<Vector<double>, ListPixel> referenceMap = new Dictionary<Vector<double>, ListPixel>();

                    foreach (ListPixel pixel in this.perimeterListPixels)
                    {
                        Vector<double> convertedPixel = new DenseVector(new double[] { pixel.X, pixel.Y });
                        transformedListPixels.Add(transformationMatrix.Multiply(convertedPixel));
                        referenceMap.Add(convertedPixel, pixel);
                    }

                    Vector<double> point1 = null;
                    Vector<double> point2 = null;
                    double longestDistance = double.MinValue;
                    double distance = double.MinValue;

                    for (int i = 0; i < transformedListPixels.Count; i++)
                    {
                        if (i + 1 >= transformedListPixels.Count)
                        {
                            continue; // Skip the last loop
                        }
                        Vector<double> toCalcFrom = transformedListPixels[i];

                        for (int j = i + 1; j < transformedListPixels.Count; j++)
                        {
                            Vector<double> toCalcTo = transformedListPixels[j];

                            if ((double)toCalcFrom.ToArray()[0] != (double)toCalcTo.ToArray()[0])
                            {
                                continue;
                            }

                            distance = Math.Sqrt(Math.Pow((double)toCalcTo.ToArray()[1] - (double)toCalcFrom.ToArray()[1], 2));
                            if (distance > longestDistance)
                            {
                                longestDistance = distance;
                                point1 = toCalcFrom;
                                point2 = toCalcTo;
                            }
                        }
                    }
                    ListPixel value1;
                    referenceMap.TryGetValue(point1, out value1);

                    ListPixel value2;
                    referenceMap.TryGetValue(point2, out value2);

                    Chord toReturn = new Chord(value1, value2);
                }

                return _longestPerpendicularChord;
            }
            set { _longestPerpendicularChord = value; }
        }

        private double _eccentricity;
        public double Eccentricity
        {
            get
            {
                if (_eccentricity == 0)
                    _eccentricity = LongestChord.distance / LongestPerpendicularChord.distance;

                return _eccentricity;
            }
            set { _eccentricity = value; }
        }

        private int _boundingBoxArea;
        public int BoundingBoxArea
        {
            get
            {
                if (_boundingBoxArea == 0)
                    _boundingBoxArea = pixels.Length;

                return _boundingBoxArea;
            }
            set { _boundingBoxArea = value; }
        }

        private double _rectangularity;
        public double Rectangularity
        {
            get
            {
                if (_rectangularity == 0)
                    _rectangularity = ((double)Area) / ((double)BoundingBoxArea);

                return _rectangularity;
            }
            set { _rectangularity = value; }
        }

        private double _elongation;
        public double Elongation
        {
            get
            {
                if (_elongation == 0) // Done wrong if BoundingBoxArea is changed (/corrected)
                {
                    double longestBBSide = pixels.GetLength(0) > pixels.GetLength(1) ? ((double)pixels.GetLength(0)) : ((double)pixels.GetLength(1));
                    double shortestBBSide = pixels.GetLength(0) < pixels.GetLength(1) ? ((double)pixels.GetLength(0)) : ((double)pixels.GetLength(1));

                    _elongation = longestBBSide / shortestBBSide;
                }

                return _elongation;
            }
            set { _elongation = value; }
        }

        private double _elongation2;
        public double Elongation2
        {
            get
            {
                if (_elongation2 == 0) // Probably done wrong
                {
                    double shortestBBSide = pixels.GetLength(0) < pixels.GetLength(1) ? ((double)pixels.GetLength(0)) : ((double)pixels.GetLength(1));
                    _elongation2 = ((double)Area) / Math.Pow(shortestBBSide / 2.0, 2);
                }

                return _elongation2;
            }
            set { _elongation2 = value; }
        }
    }
}