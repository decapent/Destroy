﻿using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

using GTI780_TP1.Header;
using GTI780_TP1.Header.Entities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTI780_TP1.Test
{
    /// <summary>
    /// The header class tests suite
    /// </summary>
    [TestClass]
    public class HeaderTests
    {
        /// <summary>
        /// Test that a bitmap image cannot be saved to an empty path
        /// </summary>
        [TestMethod]
        [TestCategory("Header")]
        [ExpectedException(typeof(ArgumentNullException), "filePath")]
        public void EnsureBitmap_SuppliedWithEmptyFilePath_ShouldThrowArgumentNullException()
        {
            // Arrange
            var header = HeaderFactory.Create(HeaderType.Stereoscopic);

            // Act
            header.EnsureBitmap(string.Empty);

            // Assert
        }

        [TestMethod]
        [TestCategory("Header")]
        [ExpectedException(typeof(ExternalException), "The specified directory does not exists!")]
        public void EnsureBitmap_SuppliedWithInexistantDirectory_ShouldThrowExternalException()
        {
            // Arrange
            var header = HeaderFactory.Create(HeaderType.Stereoscopic);

            // Act
            header.EnsureBitmap("C:\\InexistantDirectoryPath");

            // Assert
        }

        /// <summary>
        /// Tests that an bitmap image can be saved to disk given an existing path
        /// </summary>
        [TestMethod]
        [TestCategory("Header")]
        public void EnsureBitmap_SuppliedWithExistingPath_ShouldCreateNewImageOnDisk()
        {
            // Arrange
            var applicationPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            var header = HeaderFactory.Create(HeaderType.Stereoscopic);

            // Act
            header.EnsureBitmap(applicationPath);

            // Assert
            Assert.IsNotNull(header.HeaderImage);
            Assert.AreEqual(512, header.HeaderImage.Width);
            Assert.AreEqual(1, header.HeaderImage.Height);
            Assert.AreEqual(PixelFormat.Format8bppIndexed, header.HeaderImage.PixelFormat);
        }
    }
}
