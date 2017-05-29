using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GTI780_TP1.SourceProcessor
{
    public static class SourceProcessorFactory
    {
        // Size of the raw depth stream
        private const int RAWDEPTHWIDTH = 512;
        private const int RAWDEPTHHEIGHT = 424;

        // Size of the raw color stream
        private const int RAWCOLORWIDTH = 1920;
        private const int RAWCOLORHEIGHT = 1080;

        public static AbstractSourceProcessor Create(SourceProcessorTypes processorType)
        {
            switch(processorType)
            {
                case SourceProcessorTypes.Color:
                    return CreateColorSourceProcessor() as ColorSourceProcessor;
                case SourceProcessorTypes.Depth:
                    return null;
                default:
                    throw new ArgumentException("SourceProcessorFactory.Create: Invalid processor type supplied.");
            }
        }

        private static ColorSourceProcessor CreateColorSourceProcessor()
        {
            var bitmap = new WriteableBitmap(RAWCOLORWIDTH, RAWCOLORHEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);
            return new ColorSourceProcessor(bitmap);
        }
    }
}
