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
        public ColorSourceProcessor(WriteableBitmap bitmap)
            : base(SourceProcessorTypes.Color)
        {
            this.Bitmap = bitmap;
        }

        public void Process(ColorFrame frame)
        {
            bool isBitmapLocked = true;

            try
            { 
                FrameDescription frameDescription = frame.FrameDescription;
                using (KinectBuffer colorBuffer = frame.LockRawImageBuffer())
                {
                    this.Bitmap.Lock();

                    // Check for correct size
                    if (frameDescription.Width == this.Bitmap.Width && frameDescription.Height == this.Bitmap.Height)
                    {
                        //write the new color frame data to the display bitmap
                        frame.CopyConvertedFrameDataToIntPtr(this.Bitmap.BackBuffer, (uint)(frameDescription.Width * frameDescription.Height * BYTESPERPIXELS), ColorImageFormat.Bgra);

                        // Mark the entire buffer as dirty to refresh the display
                        this.Bitmap.AddDirtyRect(new Int32Rect(0, 0, frameDescription.Width, frameDescription.Height));
                    }

                    // Unlock the Bitmap
                    this.Bitmap.Unlock();
                    isBitmapLocked = false;
                }
            }
            finally
            {
                if (isBitmapLocked)
                {
                    this.Bitmap.Unlock();
                }
            }
        }
    }
}
