using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GTI780_TP1.Extensions;

namespace GTI780_TP1.Test
{
    [TestClass]
    public class BitmapExtensionTests
    {
        [TestMethod]
        [TestCategory("Extensions")]
        [ExpectedException(typeof(ArgumentNullException), "bitmap")]
        public void ToImageSource_BitmapIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            Bitmap invalid = null;

            // Act
            invalid.ToImageSource();

            // Assert
        }

        [TestMethod]
        [TestCategory("Extensions")]
        public void ToImageSource_BitmapIsValid_ShouldConvertToWPFImageSource()
        {
            // Arrange
            Bitmap bitmap = new Bitmap(512, 1);

            // Act
            var converted = bitmap.ToImageSource();

            // Assert
            Assert.IsNotNull(converted);
            Assert.IsInstanceOfType(converted, typeof(BitmapImage));
        }
    }
}
