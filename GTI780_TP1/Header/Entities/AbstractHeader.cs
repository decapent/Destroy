using System.Collections.Generic;
using System.Drawing;

using GTI780_TP1.Extensions;

namespace GTI780_TP1.Header.Entities
{
    /// <summary>
    /// Abstraction of the header to be inserted in the top left corner of the Kinect stream 
    /// </summary>
    public abstract class AbstractHeader
    {
        /// <summary>
        /// Represent the header file name to be saved on disk
        /// </summary>
        protected const string HEADERFILENAME = "EnteteModifiee.bmp";

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
        public AbstractHeader(HeaderType headerType, string H)
        {
            this.HeaderType = headerType;
            this.HeaderMessage = H;

            this._binaryMessage = this.HeaderMessage.HexToBinaryBytes();
        }

        /// <summary>
        /// Writes the header on the disk as a Bitmap file
        /// </summary>
        /// <param name="filePath">The path to the directory in which will be saved the header image</param>
        public abstract void EnsureBitmap(string filePath);

        /// <summary>
        /// Construct the byte array containing the header data
        /// </summary>
        /// <returns>The byte representation of the header</returns>
        protected abstract byte[] BuildImageBuffer();
    }
}
