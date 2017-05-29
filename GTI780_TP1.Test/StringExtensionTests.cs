using System;
using System.Linq;
using GTI780_TP1.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTI780_TP1.Test
{
    /// <summary>
    /// String Extension methods tests suite
    /// </summary>
    [TestClass]
    public class StringExtensionTests
    {
        /// <summary>
        /// Tests that attempting to convert an empty string throws an exception
        /// </summary>
        [TestMethod]
        [TestCategory("Extensions")]
        [ExpectedException(typeof(ArgumentNullException), "hex")]
        public void HexToByte_StringIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var emptyHex = string.Empty;

            // Act
            emptyHex.HexToBinaryBytes();

            // Assert
        }

        /// <summary>
        /// Tests that the conversion of the string is successfull
        /// </summary>
        [TestMethod]
        [TestCategory("Extensions")]
        public void HexToByte_StringIsValid_ShouldReturnBytesConversion()
        {
            // Arrange
            var hex = "F10140800000C42DD3AFF2140000000000000000000000000000000036958221";

            // Act
            var binaryBytes = hex.HexToBinaryBytes();

            // Assert
            Assert.IsNotNull(binaryBytes);
            Assert.AreEqual(hex.Length / 2, binaryBytes.Count());
        }

        [TestMethod]
        [TestCategory("Extensions")]
        [ExpectedException(typeof(ArgumentException), "Please provide a valid binary value")]
        public void ToByte_BitIsNotZeroOrOne_ShouldThrowArgumentException()
        {
            // Arrange
            char invalid = '2';

            // Act
            var convertedBit = invalid.ToMinMaxByteValue();

            // Assert
        }

        [TestMethod]
        [TestCategory("Extensions")]
        public void ToByte_BitIsOne_ShouldConvertToByteAndBe255()
        {
            // Arrange
            char bit = '1';

            // Act
            var convertedBit = bit.ToMinMaxByteValue();

            // Assert
            Assert.AreEqual(byte.MaxValue, convertedBit);
        }

        [TestMethod]
        [TestCategory("Extensions")]
        public void ToByte_BitIsZero_ShouldConvertToByteAndBe0()
        {
            // Arrange
            char bit = '0';

            // Act
            var convertedBit = bit.ToMinMaxByteValue();

            // Assert
            Assert.AreEqual(byte.MinValue, convertedBit);
        }
    }
}
