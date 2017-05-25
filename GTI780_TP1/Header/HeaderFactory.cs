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
        public static AbstractHeader Create(HeaderType headerType)
        {
            switch(headerType)
            {
                case HeaderType.Stereoscopic:
                    return CreateStereoscopicHeader() as StereoscopicHeader;
                default:
                    throw new ArgumentException("HeaderFactory.Create : Unrecognized HeaderType requested");
            }
        }

        /// <summary>
        /// Create a new StereoscopicHeader with an instantiated header message
        /// </summary>
        /// <returns>A new instance of the StereoscopicHeader class.</returns>
        private static AbstractHeader CreateStereoscopicHeader()
        {
            // The calculated Header Message for a 2d Scene + Depth side by side
            var H = "F10140800000C42DD3AFF2140000000000000000000000000000000036958221";
            return new StereoscopicHeader(H);
        }
    }
}
