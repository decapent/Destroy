using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GTI780_TP1.Extensions;

namespace GTI780_TP1.Header.Entities
{
    /// <summary>
    /// Abstraction of the header to be inserted in the top left corner of the Kinect stream 
    /// </summary>
    public abstract class StereoscopicHeader
    {
        /// <summary>
        /// Represent the header file name to be saved on disk
        /// </summary>
        protected const string HEADERFILENAME = "EnteteModifiee.bmp";

        /// <summary>
        /// The header image buffer size
        /// </summary>
        protected const short BUFFERSIZE = 512;

        /// <summary>
        /// The Image Header type property
        /// </summary>
        public HeaderType HeaderType { get; private set; }

        /// <summary>
        /// The 32 bytes message indicating the format of the image being displayed
        /// </summary>
        public string HeaderMessage { get; private set; }

        /// <summary>
        /// The actual bitmap file to be inserted in the top left corner of the image
        /// </summary>
        public Bitmap HeaderImage { get; set; }

        /// <summary>
        /// The header message converted to binary, each array entry is equivalent to a byte
        /// </summary>
        protected IEnumerable<string> _binaryMessage = null;

        /// <summary>
        /// Instantiate a new Header object
        /// </summary>
        /// <param name="headerType">The type of the header</param>
        /// <param name="H">The header's message to be converted to bitmap</param>
        public StereoscopicHeader(HeaderType headerType, string H)
        {
            this.HeaderType = headerType;
            this.HeaderMessage = H;

            this._binaryMessage = this.HeaderMessage.HexToBinaryBytes();
        }

        /// <summary>
        /// Writes the header on the disk as a Bitmap file
        /// </summary>
        /// <param name="filePath">The path to the directory in which will be saved the header image</param>
        public virtual void EnsureBitmap(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            if (!Directory.Exists(filePath))
            {
                throw new ExternalException("The specified directory does not exists!");
            }

            var imagePath = Path.Combine(filePath, HEADERFILENAME);

            var imageBuffer = this.BuildImageBuffer();

            this.HeaderImage = new Bitmap(
                imageBuffer.Length, // columns
                1,                  // rows
                imageBuffer.Length, // stride
                PixelFormat.Format8bppIndexed,
                Marshal.UnsafeAddrOfPinnedArrayElement(imageBuffer, 0));

            this.HeaderImage.Save(imagePath);
        }

        /// <summary>
        /// Construct the byte array containing the header data
        /// </summary>
        /// <returns>The byte representation of the header</returns>
        protected virtual byte[] BuildImageBuffer()
        {
            var imageBuffer = new byte[BUFFERSIZE];

            // Foreach byte
            for (int byteIndex = 0; byteIndex < this._binaryMessage.Count(); byteIndex++)
            {
                // Foreach bit - Starting with the lightest bit at last index
                var currentByte = this._binaryMessage.ElementAt(byteIndex);
                for (int bitIndex = currentByte.Length - 1; bitIndex >= 0; bitIndex--)
                {
                    // Determine which index needs to be written. We start from the lightest bit
                    // up to the heaviest one. The first bit to be fed to the H function is the last
                    // one but needs to be treated as Index == 0.
                    var convertedBitIndex = Math.Abs(bitIndex - (currentByte.Length - 1));
                    var bufferIndex = H(byteIndex, convertedBitIndex);

                    imageBuffer[bufferIndex] = currentByte[bitIndex].ToMinMaxByteValue();
                }
            }

            return imageBuffer;
        }

        /// <summary>
        /// The H function that calculates at which index to write the pixel
        /// of the resulting Header Image
        /// </summary>
        /// <param name="x">The current byte index</param>
        /// <param name="y">The current bit index</param>
        /// <returns></returns>
        protected virtual int H(int x, int y)
        {
            return 2 * (7 - y) + 16 * x;
        }
    }
}
