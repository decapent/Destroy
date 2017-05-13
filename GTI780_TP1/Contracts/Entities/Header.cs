using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

using GTI780_TP1.Contracts.Extensions;

namespace GTI780_TP1.Contracts.Entities
{
    /// <summary>
    /// Abstraction of the header to be inserted in the top left corner of the Kinect stream 
    /// </summary>
    public abstract class Header
    {
        /// <summary>
        /// The Image Header type property
        /// </summary>
        public HeaderType HeaderType { get; private set; }

        /// <summary>
        /// The actual bitmap file to be inserted in the top left corner of the image
        /// </summary>
        public Image HeaderImage { get; private set; }

        /// <summary>
        /// The 32 bit message indicating the format of the image being displayed
        /// </summary>
        public string HeaderMessage { get; private set; }

        /// <summary>
        /// Instantiate a new Header object
        /// </summary>
        /// <param name="headerType">The type of the header</param>
        /// <param name="H">The header's message to be converted to bitmap</param>
        public Header(HeaderType headerType, string H)
        {
            this.HeaderType = headerType;
            this.HeaderMessage = H;
        }

        public abstract void SaveToBitMap();


    }
}
