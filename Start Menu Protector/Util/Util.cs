using System;
using System.Collections;
using System.Collections.Generic;
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
        
        /* Code credit stackoverflow user Lasse Vågsæther Karlsen: https://stackoverflow.com/questions/2742276/how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object*/ 
        public static bool BelongsToClassOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            if (potentialDescendant == potentialBase)
            {
                return true;
            }
            else if (potentialDescendant.IsSubclassOf(potentialBase))
            {
                return true;
            }
            else if (potentialDescendant.IsAssignableFrom(potentialBase))
            {
                return true;
            }
            else if (((IList) potentialDescendant.GetInterfaces()).Contains(potentialBase))
            {
                return true;
            }
            else
            {
                return false;
            }
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
        
        public static void AddAll<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }
        
        /// <summary>
        /// Clear the contents of collection and replace with items
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        public static void ReplaceAll<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            collection.AddAll(items);
        }

        public static bool IsOfType<T>(this object @object)
        {
            return Util.BelongsToClassOrSubclass(potentialBase: typeof(T), potentialDescendant: @object.GetType());
        }
    }
}