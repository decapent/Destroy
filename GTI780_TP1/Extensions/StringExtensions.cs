using System;
using System.Collections.Generic;
using System.Linq;

namespace GTI780_TP1.Extensions
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
        public static IEnumerable<string> HexToBinaryBytes(this string hex)
        {
            // Sanity check for instanciated hexadecimal string
            if(string.IsNullOrEmpty(hex))
            {
                throw new ArgumentNullException("hex");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            // Convert bytes representation to binary
            return bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).ToList();
        }

        /// <summary>
        /// Converts the char representation of a bit value into a byte data type.
        /// </summary>
        /// <param name="value">The binary char value</param>
        /// <returns>The value 255 if the bit is 1, 0 otherwise.</returns>
        public static byte ToByte(this char bit)
        {
            if(bit != '0' && bit != '1')
            {
                throw new ArgumentException("Please provide a valid binary value");
            }

            return bit == '1' ? byte.MaxValue : byte.MinValue;
        }
    }
}
