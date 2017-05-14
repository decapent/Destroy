using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTI780_TP1.Contracts;
using GTI780_TP1.Contracts.Entities;
using GTI780_TP1.Contracts.Extensions;

namespace GTI780_TP1.Core
{
    public static class HeaderFactory
    {
        public static Header Create(HeaderType headerType)
        {
            switch(headerType)
            {
                case HeaderType.Stereoscopic:
                    return CreateStereoscopicHeader(headerType) as StereoscopicHeader;
                default:
                    throw new ArgumentException("HeaderFactory.Create : Unrecognized HeaderType requested");
            }
        }

        private static Header CreateStereoscopicHeader(HeaderType headerType)
        {
            // The calculated Header Message for a 2d Scene + Depth side by side
            var H = "F10140800000C42DD3AFF2140000000000000000000000000000000036958221";
            var header = new StereoscopicHeader(H);

            //header.ConvertHeaderMessageToBitmap("EnTeteModifiee");

            header.HeaderMessage.HexToBytes();

            return header;
        }
    }
}
