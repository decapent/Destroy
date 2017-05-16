using System;

using GTI780_TP1.Header;
using GTI780_TP1.Header.Entities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTI780_TP1.Test
{
    /// <summary>
    /// The HeaderFactory test suite
    /// </summary>
    [TestClass]
    public class HeaderFactoryTests
    {
        /// <summary>
        /// Tests that an invalid Header Type should throw an exception
        /// </summary>
        [TestMethod]
        [TestCategory("Factory")]
        [ExpectedException(typeof(ArgumentException), "HeaderFactory.Create : Unrecognized HeaderType requested")]
        public void Create_SuppliedWithInvalidHeaderType_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidHeaderType = (HeaderType)999999999;

            // Act
            HeaderFactory.Create(invalidHeaderType);
        }

        /// <summary>
        /// Tests that a StereoScopic Header type instantiate a new Header with it's defaulted header message
        /// </summary>
        [TestMethod]
        [TestCategory("Factory")]
        public void Create_SuppliedWithStereoscopicHeaderType_ShouldCreateNewHeaderWithMessage()
        {
            // Arrange

            // Act
            var header = HeaderFactory.Create(HeaderType.Stereoscopic);

            // Assert
            Assert.IsNotNull(header);
            Assert.IsInstanceOfType(header, typeof(StereoscopicHeader));
            Assert.IsFalse(string.IsNullOrEmpty(header.HeaderMessage));
            Assert.AreEqual(64, header.HeaderMessage.Length);
        }
    }
}
