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
        /// Map depth range to byte range
        /// </summary>
        private const int MAPDEPTHTOBYTE = 8000 / 256;

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
            bool isBitmapLocked = true;

            try
            {
                FrameDescription frameDescription = frame.FrameDescription;
                using (KinectBuffer depthBuffer = frame.LockImageBuffer())
                {
                    this.Bitmap.Lock();

                    this._depthPixels = new byte[frameDescription.Width * frameDescription.Height];

                    if (frameDescription.Width == this.Bitmap.Width && frameDescription.Height == this.Bitmap.Height)
                    {
                        // Use coordinate mapper to
                        var depthSpacePointTable = new DepthSpacePoint[colorFrameDescription.Width * colorFrameDescription.Height];
                        this._mapper.MapColorFrameToDepthSpaceUsingIntPtr(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthSpacePointTable);

                        //  Utiliser la ligne ci-dessous pour l'image de profondeur
                        Image<Gray, byte> depthImageGray = new Image<Gray, byte>(
                            (int)this.Bitmap.Width,  
                            (int)this.Bitmap.Height,  
                            (int)this.Bitmap.Width, // stride
                            depthBuffer.UnderlyingBuffer); // data

                        //-----------------------------------------------------------
                        // Traiter l'image de profondeur 
                        //-----------------------------------------------------------

                        // Une fois traitée convertir l'image en Bgra
                        Image<Bgra, byte> depthImageBgra = depthImageGray.Convert<Bgra, byte>();

                        this.ProcessDepthFrameData(
                            depthImageBgra.Ptr,
                            (uint)depthImageBgra.Data.Length,
                            frame.DepthMinReliableDistance,
                            frame.DepthMaxReliableDistance,
                            frameDescription.BytesPerPixel);

                        // Render depth pixels
                        this.Bitmap.WritePixels(
                            new Int32Rect(0, 0, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight),
                            this._depthPixels,
                            this.Bitmap.PixelWidth,
                            0);
                    }
                    
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
        /// <param name="bytesPerPixel">The number of bytes used per pixel</param>
        private unsafe void ProcessDepthFrameData(
            IntPtr depthFrameData, 
            uint depthFrameDataSize, 
            ushort minDepth, 
            ushort maxDepth, 
            uint bytesPerPixel)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / bytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this._depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MAPDEPTHTOBYTE) : 0);
            }
        }
    }
}
