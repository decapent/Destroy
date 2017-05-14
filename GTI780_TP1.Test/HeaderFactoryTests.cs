using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GTI780_TP1.Core;
using GTI780_TP1.Contracts.Entities;

namespace GTI780_TP1.Test
{
    /// <summary>
    /// Summary description for HeaderFactoryTests
    /// </summary>
    [TestClass]
    public class HeaderFactoryTests
    {
        [TestMethod]
        [TestCategory("Factory")]
        [ExpectedException(typeof(ArgumentException), "HeaderFactory.Create : Unrecognized HeaderType requested")]
        public void Create_WhenSuppliedWithInvalidHeaderType_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidHeaderType = (HeaderType)1234;

            // Act
            HeaderFactory.Create(invalidHeaderType);
        }

        [TestMethod]
        [TestCategory("Factory")]
        public void Create_WhenSuppliedWithStereoscopicHeaderType_ShouldCreateNewHeaderWithMessage()
        {
            // Arrange

            // Act
            var header = HeaderFactory.Create(HeaderType.Stereoscopic) as StereoscopicHeader;

            // Assert
            Assert.IsNotNull(header);
            Assert.IsInstanceOfType(header, typeof(StereoscopicHeader));
            Assert.IsFalse(string.IsNullOrEmpty(header.HeaderMessage));
        }
    }
}
