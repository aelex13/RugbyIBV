using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.Filters
{
    public static class FilterTypes
    {
        private static readonly List<FilterType> Filters = new List<FilterType>()
        {
            // Some Dummy filtertypes
             new FilterType(FilterType.Type.BasicKernel, "3x3 Kernel"),
             new FilterType(FilterType.Type.GrayscaleFilter, "Grayscale"),
             new FilterType(FilterType.Type.HighPassFilter32, "High-filter pass 32"),
             new FilterType(FilterType.Type.HighPassFilter64, "High-filter pass 64"),
             new FilterType(FilterType.Type.HighPassFilter96, "High-filter pass 96"),
             new FilterType(FilterType.Type.HighPassFilter128, "High-filter pass 128"),
             new FilterType(FilterType.Type.HighPassFilter160, "High-filter pass 160"),
             new FilterType(FilterType.Type.HighPassFilter192, "High-filter pass 192"),
             new FilterType(FilterType.Type.HighPassFilter224, "High-filter pass 224"),
             new FilterType(FilterType.Type.HighPassFilter256, "High-filter pass 256"),
             new FilterType(FilterType.Type.LowPassFilter32, "Low-filter pass 32"),
             new FilterType(FilterType.Type.LowPassFilter64, "Low-filter pass 64"),
             new FilterType(FilterType.Type.LowPassFilter96, "Low-filter pass 96"),
             new FilterType(FilterType.Type.LowPassFilter128, "Low-filter pass 128"),
             new FilterType(FilterType.Type.LowPassFilter160, "Low-filter pass 160"),
             new FilterType(FilterType.Type.LowPassFilter192, "Low-filter pass 192"),
             new FilterType(FilterType.Type.LowPassFilter224, "Low-filter pass 224"),
             new FilterType(FilterType.Type.LowPassFilter256, "Low-filter pass 256"),
             new FilterType(FilterType.Type.ThresholdFilter32, "Threshold 32"),
             new FilterType(FilterType.Type.ThresholdFilter64, "Threshold 64"),
             new FilterType(FilterType.Type.ThresholdFilter96, "Threshold 96"),
             new FilterType(FilterType.Type.ThresholdFilter128, "Threshold 128"),
             new FilterType(FilterType.Type.ThresholdFilter160, "Threshold 160"),
             new FilterType(FilterType.Type.ThresholdFilter192, "Threshold 192"),
             new FilterType(FilterType.Type.ThresholdFilter224, "Threshold 224"),
             new FilterType(FilterType.Type.ThresholdFilter256, "Threshold 256")
        };

        public static List<FilterType> GetAllFilters()
        {
            return Filters;
        }
    }

    public sealed class FilterType
    {
        public enum Type
        {
            BasicKernel,
            GrayscaleFilter,
            HighPassFilter32,
            HighPassFilter64,
            HighPassFilter96,
            HighPassFilter128,
            HighPassFilter160,
            HighPassFilter192,
            HighPassFilter224,
            HighPassFilter256,
            LowPassFilter32,
            LowPassFilter64,
            LowPassFilter96,
            LowPassFilter128,
            LowPassFilter160,
            LowPassFilter192,
            LowPassFilter224,
            LowPassFilter256,
            ThresholdFilter32,
            ThresholdFilter64,
            ThresholdFilter96,
            ThresholdFilter128,
            ThresholdFilter160,
            ThresholdFilter192,
            ThresholdFilter224,
            ThresholdFilter256
        }

        public Type fType { get; private set; }
        public string Value { get; private set; }

        public FilterType(Type fType, string Value)
        {
            this.fType = fType;
            this.Value = Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}