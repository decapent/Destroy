using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTI780_TP1.Contracts.Extensions
{
    public static class StringExtensions
    {
        public static byte[] HexToBytes(this string hex)
        {
            if(string.IsNullOrEmpty(hex))
            {
                throw new ArgumentNullException("hex");
            }

            if(hex.Length % 2 != 0)
            {
                throw new ArgumentException(string.Format("Odd Hex string length, should be even --> {0}", hex.Length));
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}
