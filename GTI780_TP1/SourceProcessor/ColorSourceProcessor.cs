using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.Windows;

namespace GTI780_TP1.SourceProcessor
{
    public sealed class ColorSourceProcessor : AbstractSourceProcessor
    {
        public ColorFrame Source { get; set; }

        public ColorSourceProcessor(WriteableBitmap bitmap)
            : base(SourceProcessorTypes.Color)
        {
            this.Source = null;
            this.Bitmap = bitmap;
        }

        public override void Process()
        {
            bool isColorBitmapLocked = false;

            try
            { 
                FrameDescription colorDescription = this.Source.FrameDescription;
                using (KinectBuffer colorBuffer = this.Source.LockRawImageBuffer())
                {
                    // Lock the colorBitmap while we write in it.
                    this.Bitmap.Lock();
                    isColorBitmapLocked = true;

                    // Check for correct size
                    if (colorDescription.Width == this.Bitmap.Width && colorDescription.Height == this.Bitmap.Height)
                    {
                        //write the new color frame data to the display bitmap
                        this.Source.CopyConvertedFrameDataToIntPtr(this.Bitmap.BackBuffer, (uint)(colorDescription.Width * colorDescription.Height * BYTESPERPIXELS), ColorImageFormat.Bgra);

                        // Mark the entire buffer as dirty to refresh the display
                        this.Bitmap.AddDirtyRect(new Int32Rect(0, 0, colorDescription.Width, colorDescription.Height));
                    }

                    // Unlock the colorBitmap
                    this.Bitmap.Unlock();
                    isColorBitmapLocked = false;
                }
            }
            finally
            {
                if (isColorBitmapLocked)
                {
                    this.Bitmap.Unlock();
                }

                if (this.Source != null)
                {
                    this.Source.Dispose();
                }
            }
        }
    }
}
