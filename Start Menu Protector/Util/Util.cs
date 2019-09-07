using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Data.Text;

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
        
        public static IEnumerable<E> GetEnumValues<E>() where E : Enum 
        {
            return Enum.GetValues(typeof(E)).Cast<E>();
        }
        
        /* Code credit: https://stackoverflow.com/questions/16720496/set-apartmentstate-on-a-task */
        public static Task StartSTATask(Action action)
        {
            var task = new TaskCompletionSource<object>();
            
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    task.SetResult(null);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });
            
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            
            return task.Task;
        }

        public static String GenerateRandomString(uint length)
        {
            var randomGenerator = new Random();
            var builder = new StringBuilder();

            for (uint i = 0; i < length; i++)
            {
                char randomChar = (char) randomGenerator.Next(0, 2 ^ 31);
                builder.Append(randomChar);
            }

            return builder.ToString();
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
        
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach(T item in enumeration)
            {
                action(item);
            }
        }

        public static bool IsOfType<T>(this object @object)
        {
            return Util.BelongsToClassOrSubclass(potentialBase: typeof(T), potentialDescendant: @object.GetType());
        }
    }
}