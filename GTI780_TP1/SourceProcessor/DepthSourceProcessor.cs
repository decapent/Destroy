using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Kinect;

namespace GTI780_TP1.SourceProcessor
{
    public sealed class DepthSourceProcessor : AbstractSourceProcessor
    {
        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private byte[] _depthPixels = null;

        private CoordinateMapper _mapper = null;

        public DepthSourceProcessor(WriteableBitmap bitmap, CoordinateMapper mapper)
            : base(SourceProcessorTypes.Depth)
        {
            this.Bitmap = bitmap;
            this._mapper = mapper;
        }

        public void Process(DepthFrame frame, FrameDescription colorFrameDescription)
        {
            this.IsBitmapLocked = true;

            try
            {
                FrameDescription frameDescription = frame.FrameDescription;
                using (KinectBuffer depthBuffer = frame.LockImageBuffer())
                {
                    this.Bitmap.Lock();

                    this._depthPixels = new byte[frameDescription.Width * frameDescription.Height];

                    if (frameDescription.Width == this.Bitmap.Width && frameDescription.Height == this.Bitmap.Height)
                    {
                        var depthSpacePointTable = new DepthSpacePoint[colorFrameDescription.Width * colorFrameDescription.Height];
                        this._mapper.MapColorFrameToDepthSpaceUsingIntPtr(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthSpacePointTable);

                        this.ProcessDepthFrameData(
                            depthBuffer.UnderlyingBuffer, 
                            depthBuffer.Size, 
                            frame.DepthMinReliableDistance, 
                            frame.DepthMaxReliableDistance, 
                            frameDescription.BytesPerPixel);

                      
                    }
                    
                    this.Bitmap.Unlock();
                    this.IsBitmapLocked = false;
                }
            }
            finally
            {
                if (this.IsBitmapLocked)
                {
                    this.Bitmap.Unlock();
                }
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        /// /// <param name="bytesPerPixel">The number of bytes used per pixel</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth, uint bytesPerPixel)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            int MapDepthToByte = -1 * (maxDepth / 256);

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / bytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this._depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }

            //  Utiliser la ligne ci-dessous pour l'image de profondeur
            Image<Gray, byte> depthImageGray = new Image<Gray, byte>((int)this.Bitmap.Width, (int)this.Bitmap.Height);

            //-----------------------------------------------------------
            // Traiter l'image de profondeur 
            //-----------------------------------------------------------
            depthImageGray.Bytes = this._depthPixels;

            // Une fois traitée convertir l'image en Bgra
            var depthImageBgra = depthImageGray.Convert<Bgra, byte>();           
            depthImageBgra = depthImageBgra.SmoothMedian(7);

            // Render depth pixels
            this.Bitmap.WritePixels(
                new Int32Rect(0, 0, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight),
                depthImageBgra.Bytes,
                this.Bitmap.PixelWidth * 4,
                0);
        }
    }
}
