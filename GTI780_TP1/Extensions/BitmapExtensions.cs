using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace GTI780_TP1.Extensions
{
    public static class BitmapExtensions
    {
        public static BitmapImage ToImageSource(this Bitmap bitmap)
        {
            if(bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }

            BitmapImage converted = new BitmapImage();

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                
                converted.BeginInit();
                converted.StreamSource = memory;
                converted.CacheOption = BitmapCacheOption.OnLoad;
                converted.EndInit();
            }

            return converted;
        }
    }
}
