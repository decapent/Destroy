using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using GTI780_TP1.Extensions;

namespace GTI780_TP1.Header.Entities
{
    /// <summary>
    /// Concrete implementation of the Header class for a stereoscopic header type
    /// </summary>
    public sealed class StereoscopicHeader : AbstractHeader
    {
        /// <summary>
        /// The header image buffer size
        /// </summary>
        private const short BUFFERSIZE = 512;
        
        /// <summary>
        /// Instantiate a new StereoscopicHeader object
        /// </summary>
        /// <param name="H">The header hexadecimal message</param>
        public StereoscopicHeader(string H)
            :base(HeaderType.Stereoscopic, H)
        {
            
        }

        /// <summary>
        /// Writes the header on the disk as a Bitmap file
        /// </summary>
        /// <param name="filePath">The path to the directory in which will be saved the header image</param>
        public override void EnsureBitmap(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }
            
            if(!Directory.Exists(filePath))
            {
                throw new ExternalException("The specified directory does not exists!");
            }

            var imageBuffer = this.BuildImageBuffer();
            
            this.HeaderImage = new Bitmap(
                imageBuffer.Length, // columns
                1,                  // rows
                imageBuffer.Length, // stride
                PixelFormat.Format8bppIndexed,
                Marshal.UnsafeAddrOfPinnedArrayElement(imageBuffer, 0));

            this.HeaderImage.Save(Path.Combine(filePath, "EnteteModifiee.bmp"));             
        }

        /// <summary>
        /// Construct the byte array containing the header data
        /// </summary>
        /// <returns>The byte representation of the header</returns>
        protected override byte[] BuildImageBuffer()
        {
            var imageBuffer = new byte[BUFFERSIZE];

            // Foreach byte
            for (int byteIndex = 0; byteIndex < this._binaryMessage.Count(); byteIndex++)
            {
                // Foreach bit
                var currentByte = this._binaryMessage.ElementAt(byteIndex);
                for (int bitIndex = currentByte.Length - 1; bitIndex >= 0; bitIndex--)
                {
                    // Determine which index needs to be written and the value of the heavy bit
                    // that will caracterize the index
                    var bufferIndex = CalculateImageBufferIndex(byteIndex, bitIndex);
                    var heavyBit = currentByte.FirstOrDefault();

                    imageBuffer[bufferIndex] = heavyBit.ToMinMaxByteValue();
                }
            }

            return imageBuffer;
        }

        /// <summary>
        /// Calculates the index of the header image buffer that needs to be written.
        /// </summary>
        /// <param name="byteIndex">The byte index</param>
        /// <param name="bitIndex">The bit index</param>
        /// <returns>The index of the header image buffer.</returns>
        private static int CalculateImageBufferIndex(int byteIndex, int bitIndex)
        {
            return 2 * (7 - bitIndex) + 16 * byteIndex;
        }
    }
}
