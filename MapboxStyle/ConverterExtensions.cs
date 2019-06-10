using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace MapboxStyle
{
    public static class ConverterExtensions
    {
        private const string HexPrefix = "#";
        private const string RgbPrefix = "rgb(";
        private const string RgbaPrefix = "rgba(";
        private const string HslPrefix = "hsl(";
        private const string HslaPrefix = "hsla(";

        public static double ParsePercent(this string value)
        {
            return double.Parse(value.Replace(NumberFormatInfo.CurrentInfo.PercentSymbol, string.Empty)) / 100;
        }

        public static Color ParseHtmlColor(this string htmlColor)
        {
            if (htmlColor.StartsWith(HexPrefix))
            {
                return ConvertFromHex(htmlColor);
            }

            if (htmlColor.StartsWith(RgbPrefix))
            {
                return ConvertFromRgb(htmlColor);
            }

            if (htmlColor.StartsWith(RgbaPrefix))
            {
                return ConvertFromRgba(htmlColor);
            }

            if (htmlColor.StartsWith(HslPrefix))
            {
                return ConvertFromHsl(htmlColor);
            }

            if (htmlColor.StartsWith(HslaPrefix))
            {
                return ConvertFromHsla(htmlColor);
            }

            return Color.FromName(htmlColor);
        }


        private static Color ConvertFromHex(string htmlColor)
        {
            if (htmlColor[0] == '#' && htmlColor.Length == 4)
            {
                char r = htmlColor[1], g = htmlColor[2], b = htmlColor[3];
                htmlColor = new string(new[] { '#', r, r, g, g, b, b });
            }

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Color));

            return (Color)converter.ConvertFromString(htmlColor);
        }

        private static Color ConvertFromRgb(string htmlColor)
        {
            var rgb = GetValues(RgbPrefix, htmlColor);

            if (rgb.Length != 3)
            {
                throw new InvalidDataException();
            }

            var r = byte.Parse(rgb[0]);
            var g = byte.Parse(rgb[1]);
            var b = byte.Parse(rgb[2]);

            return Color.FromArgb(byte.MaxValue, r, g, b);
        }

        private static string[] GetValues(string prefix, string htmlColor)
        {
            var start = htmlColor.IndexOf(prefix, StringComparison.InvariantCultureIgnoreCase) + prefix.Length;
            var end = htmlColor.IndexOf(")", StringComparison.InvariantCultureIgnoreCase);
            var length = end - start;

            var colors = htmlColor.Substring(start, length);
            var rgb = colors.Split(',');
            return rgb;
        }

        private static Color ConvertFromRgba(string htmlColor)
        {
            var rgba = GetValues(RgbaPrefix, htmlColor);

            if (rgba.Length != 4)
            {
                throw new InvalidDataException();
            }

            var r = byte.Parse(rgba[0]);
            var g = byte.Parse(rgba[1]);
            var b = byte.Parse(rgba[2]);
            var a = double.Parse(rgba[3]) * byte.MaxValue;

            return Color.FromArgb((byte) a, r, g, b);
        }

        private static Color ConvertFromHsl(string htmlColor)
        {
            var hsl = GetValues(HslPrefix, htmlColor);

            if (hsl.Length != 3)
            {
                throw new InvalidDataException();
            }

            var h = double.Parse(hsl[0]);
            var s = hsl[1].ParsePercent();
            var l = hsl[2].ParsePercent();

            var hslaColor = new HslaColor(h / 360, s, l);

            return hslaColor.ToColor();
        }

        private static Color ConvertFromHsla(string htmlColor)
        {
            var hsla = GetValues(HslaPrefix, htmlColor);

            if (hsla.Length != 4)
            {
                throw new InvalidDataException();
            }

            var h = double.Parse(hsla[0]);
            var s = hsla[1].ParsePercent();
            var l = hsla[2].ParsePercent();
            var a = double.Parse(hsla[3]);

            var hslaColor = new HslaColor(h / 360, s, l, a);

            return hslaColor.ToColor();
        }
    }
}
