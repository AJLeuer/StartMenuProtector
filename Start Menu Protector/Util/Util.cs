using System.IO;
using System.Windows.Media.Imaging;

namespace StartMenuProtector.Util
{
    public static class Util
    {
        public static void Sanitize(FileSystemInfo fileSystemItem)
        {
        }
    }

    public static class Extensions
    {
        /// Code credit: https://stackoverflow.com/questions/37890121/fast-conversion-of-bitmap-to-imagesource
        public static BitmapImage ConvertToImageSource(this System.Drawing.Bitmap bitmap)
        {         
            var memoryStream = new MemoryStream();
            var image = new BitmapImage();

            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            image.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);
            image.StreamSource = memoryStream;
            image.EndInit();

            return image;
        }
    }
}