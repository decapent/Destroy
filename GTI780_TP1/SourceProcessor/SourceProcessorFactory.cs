using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

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

        /// <summary>
        /// Generates a new instance of a concrete AbstractSourceProcessor base on the specified type
        /// </summary>
        /// <param name="processorType">The type of source processor wanted</param>
        /// <param name="mapper">Optional, Used by the depth source processor to match the color to depth pixels</param>
        /// <returns></returns>
        public static AbstractSourceProcessor Create(SourceProcessorTypes processorType, CoordinateMapper mapper = null)
        {
            switch(processorType)
            {
                case SourceProcessorTypes.Color:
                    return CreateColorSourceProcessor();
                case SourceProcessorTypes.Depth:
                    return CreateDepthSourceProcessor(mapper);
                default:
                    throw new ArgumentException("SourceProcessorFactory.Create: Invalid processor type supplied.");
            }
        }

        private static ColorSourceProcessor CreateColorSourceProcessor()
        {
            var bitmap = new WriteableBitmap(RAWCOLORWIDTH, RAWCOLORHEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);
            return new ColorSourceProcessor(bitmap);
        }

        private static DepthSourceProcessor CreateDepthSourceProcessor(CoordinateMapper mapper)
        {
            var bitmap = new WriteableBitmap(RAWDEPTHWIDTH, RAWDEPTHHEIGHT, 96.0, 96.0, PixelFormats.Gray8, null);
            return new DepthSourceProcessor(bitmap, mapper);
        }
    }
}
