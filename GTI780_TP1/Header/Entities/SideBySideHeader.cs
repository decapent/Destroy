namespace GTI780_TP1.Header.Entities
{
    /// <summary>
    /// Concrete implementation of the Header class for a stereoscopic header type
    /// </summary>
    public sealed class SideBySideHeader : StereoscopicHeader
    {        
        /// <summary>
        /// Instantiate a new StereoscopicHeader object
        /// </summary>
        /// <param name="H">The header hexadecimal message</param>
        public SideBySideHeader(string H)
            :base(HeaderType.SideBySide, H)
        {
            
        }
    }
}
