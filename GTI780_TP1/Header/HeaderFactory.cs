using System;
using GTI780_TP1.Header.Entities;

namespace GTI780_TP1.Header
{
    /// <summary>
    /// Produces instances of the Header class by type of header. 
    /// </summary>
    public static class HeaderFactory
    {
        /// <summary>
        /// Produces a new instance of a Header by type of header
        /// </summary>
        /// <param name="headerType">The type of header to create.</param>
        /// <returns>A new instance of the Header class</returns>
        public static StereoscopicHeader Create(HeaderType headerType)
        {
            switch(headerType)
            {
                case HeaderType.SideBySide:
                    return CreateStereoscopicHeader() as SideBySideHeader;
                case HeaderType.TopAndDown:
                    return CreateTopAndDownHeader() as TopAndDownHeader;
                default:
                    throw new ArgumentException("HeaderFactory.Create : Unrecognized HeaderType requested");
            }
        }

        /// <summary>
        /// Create a new StereoscopicHeader with an instantiated header message
        /// </summary>
        /// <returns>A new instance of the StereoscopicHeader class.</returns>
        private static StereoscopicHeader CreateStereoscopicHeader()
        {
            // The calculated Header Message for a 2d Scene + Depth side by side
            var H = "F10140800000C42DD3AFF2140000000000000000000000000000000036958221";
            return new SideBySideHeader(H);
        }

        private static StereoscopicHeader CreateTopAndDownHeader()
        {
            // The calculated Header Message for Top frame with color and
            // bottom frame processed throught the DIBR algorithm.
            var H = "F10140801000D47C48BCF22233000000000000000000000000000000AF7AB8ED";
            return new TopAndDownHeader(H);
        }
    }
}
