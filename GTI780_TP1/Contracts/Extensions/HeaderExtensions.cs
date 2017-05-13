using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing.Imaging;
using GTI780_TP1.Contracts.Entities;


namespace GTI780_TP1.Contracts.Extensions
{
    public static class HeaderExtensions
    {
        public static void ConvertHeaderMessageToBitmap(this Header header, string fileName)
        {
            header.HeaderImage.Save(fileName, ImageFormat.Bmp);
        }
    }
}
