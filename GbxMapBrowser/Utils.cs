using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using TmEssentials;

namespace GbxMapBrowser
{
    /// <summary>
    /// Provides needed converters and tools
    /// </summary>
    public static class Utils
    {
        public static BitmapImage ConvertToImageSource(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        /// <summary>
        /// Converts TimeSpan to string (in TrackMania format)
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string TimeSpanToString(TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue) return "-:--.---";
            TimeSpan time = timeSpan.GetValueOrDefault();
            return time.ToTmString();
        }

        /// <summary>
        /// Formats text containing colors, styles, etc to plain text
        /// </summary>
        /// <param name="defaultname"></param>
        /// <returns></returns>
        public static string ToReadableText(string defaultname)
        {
            if (defaultname is null)
                return null;
            string formattedName = defaultname;
            formattedName = TmEssentials.TextFormatter.Deformat(formattedName);
            return formattedName;
        }
    }
}
