using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace StartMenuProtector.Util
{
    public static class Util
    {
        public static Uri GetResourceURI(string resourcePath)
        {
            String uri = $"pack://application:,,,{resourcePath}";
            return new Uri(uri);
        }
    }

    public static class Extensions
    {
        /// Code credit: https://stackoverflow.com/questions/37890121/fast-conversion-of-bitmap-to-imagesource
        public static BitmapImage ConvertToImageSource(this Bitmap bitmap)
        {         
            var memoryStream = new MemoryStream();
            var image = new BitmapImage();

            bitmap.Save(memoryStream, ImageFormat.Png);
            image.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);
            image.StreamSource = memoryStream;
            image.EndInit();

            return image;
        }
    }
}