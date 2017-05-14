using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GTI780_TP1.Contracts.Extensions;

namespace GTI780_TP1.Test
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        [TestCategory("Extensions")]
        [ExpectedException(typeof(ArgumentNullException), "hex")]
        public void HexToByte_WhenStringIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var emptyHex = string.Empty;

            // Act
            emptyHex.HexToBytes();
        }

        [TestMethod]
        [TestCategory("Extensions")]
        public void HexToByte_WhenStringIsValid_ShouldReturnBytesConversion()
        {
            // Arrange
            var hex = "F10140800000C42DD3AFF2140000000000000000000000000000000036958221";

            // Act
            var bytes = hex.HexToBytes();

            // Assert
            Assert.IsNotNull(bytes);
            Assert.AreEqual(hex.Length / 2, bytes.Length);
        }
    }
}
