using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTI780_TP1.Contracts.Extensions
{
    /// <summary>
    /// Extension methods for the string class
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Convert an hex string to a byte array
        /// </summary>
        /// <param name="hex">The hexadecimal string to convert</param>
        /// <returns>The hex representation as bytes</returns>
        public static byte[] HexToBytes(this string hex)
        {
            if(string.IsNullOrEmpty(hex))
            {
                throw new ArgumentNullException("hex");
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
