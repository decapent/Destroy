using System;
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
            var invalidDirectoryPath = string.Format("C:\\{0}", new Guid().ToString());

            // Act
            header.EnsureBitmap(invalidDirectoryPath);

            // Assert
        }

        /// <summary>
        /// Tests that an bitmap image can be saved to disk given an existing path
        /// </summary>
        [TestMethod]
        [TestCategory("Header")]
        public void EnsureBitmap_SuppliedWithExistingFilePath_ShouldNotCreateNewImageOnDisk()
        {
            // Arrange
            var applicationPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            var header = HeaderFactory.Create(HeaderType.Stereoscopic);

            // Act
            header.EnsureBitmap(applicationPath);

            // Assert
            Assert.IsNull(header.HeaderImage);
        }
    }
}
