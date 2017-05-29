using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GTI780_TP1.SourceProcessor
{
    public abstract class AbstractSourceProcessor
    {
        // Number of bytes per pixel for the format used in this project
        protected int BYTESPERPIXELS = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        public SourceProcessorTypes ProcessorType { get; private set; }

        public WriteableBitmap Bitmap { get; set; }

        public AbstractSourceProcessor(SourceProcessorTypes processorType)
        {
            this.ProcessorType = processorType;
        }

        public abstract void Process();
    }
}
