using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using GTI780_TP1.Extensions;
using GTI780_TP1.Header;
using GTI780_TP1.Header.Entities;
using Microsoft.Kinect;

namespace GTI780_TP1
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Size of the raw color stream
        private const int RAWCOLORWIDTH = 1920;
        private const int RAWCOLORHEIGHT = 1080;

        // Number of bytes per pixel for the format used in this project
        private int BYTESPERPIXELS = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        // Size of the target display screen
        private double screenWidth;
        private double screenHeight;

        // Bitmaps to display
        private WriteableBitmap colorBitmap = null;
        private WriteableBitmap depthBitmap = null;

        // The kinect sensor
        private KinectSensor kinectSensor = null;

        // The kinect frame reader
        private MultiSourceFrameReader frameReader = null;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private byte[] depthPixels = null;

        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private Image<Bgra, byte> depthImageBgra = null;

        // DIBR algorithm parameters
        // Measurement from the Dimenco TV
        private double T = 1651; //Should be in mm. Pour 65 pouces
        private double W = 0.0;
        private double H = 0.0;
        private int TV_Ratio_Height = 9;
        private int TV_Ratio_Width = 16;

        private const int NUMBER_PIXELS_WIDTH = 3840;

        // User distance from Dimenco TV
        private double D = 0.0;
        private double tc = 65; // milimeters 
        private double knear = 0.1;
        private double kfar = 0.2;

        public MainWindow()
        {
            InitializeComponent();

            // Sets the correct size to the display components
            InitializeComponentsSize();
            SetTelevisionDimension();

            // Set Header image for 2d color + depth side by side format
            InitializeHeader();

            // Instanciate the WriteableBitmaps used to display the kinect frames
            this.colorBitmap = new WriteableBitmap(RAWCOLORWIDTH, RAWCOLORHEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.depthBitmap = new WriteableBitmap(RAWCOLORWIDTH, RAWCOLORHEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);

            // Connect to the Kinect Sensor
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the color frames
            this.frameReader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

            // wire handler for frame arrival
            this.frameReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;

            // Open the kinect sensor
            this.kinectSensor.Open();

            // Sets the context for the data binding
            this.DataContext = this;
        }

        /// <summary>
        /// This event will be called whenever the Multi source Frame Reader receives a new frame
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="frameArrivedEvent">args of the event</param>
        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs frameArrivedEvent)
        {
            // Store the state of the frame lock
            bool isColorBitmapLocked = false;
            bool isDepthBitmapLocked = false;

            // Acquire multisource frame containing the color and depth information of the kinect stream
            MultiSourceFrame multiSourceFrame = frameArrivedEvent.FrameReference.AcquireFrame();
            if (multiSourceFrame == null)
            {
                return;
            }

            // Sanity check on frames instantiation. Can't have one failing. 
            // Abort if either of the frames are null
            ColorFrame colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
            DepthFrame depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
            if (colorFrame == null || depthFrame == null)
            {
                return;
            }

            // Using a try/finally structure allows us to liberate/dispose of the elements even if there was an error
            try
            {
                // ===============================
                // ColorFrame code block
                // ===============================   
                FrameDescription colorDescription = colorFrame.FrameDescription;

                using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                {
                    // Lock the colorBitmap while we write in it.
                    this.colorBitmap.Lock();
                    isColorBitmapLocked = true;

                    // Check for correct size
                    if (colorDescription.Width == this.colorBitmap.Width && colorDescription.Height == this.colorBitmap.Height)
                    {
                        //write the new color frame data to the display bitmap
                        colorFrame.CopyConvertedFrameDataToIntPtr(this.colorBitmap.BackBuffer, (uint)(colorDescription.Width * colorDescription.Height * BYTESPERPIXELS), ColorImageFormat.Bgra);

                        // Mark the entire buffer as dirty to refresh the display
                        this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorDescription.Width, colorDescription.Height));
                    }

                    // Unlock the colorBitmap
                    this.colorBitmap.Unlock();
                    isColorBitmapLocked = false;
                }

                // ================================================================
                // DepthFrame code block
                // ================================================================
                FrameDescription depthDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

                // allocate space to put the pixels being received and converted
                this.depthPixels = new byte[colorDescription.Width * colorDescription.Height];

                using (KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                {
                    // Lock the depthBitmap while we write in it.
                    this.depthBitmap.Lock();
                    isDepthBitmapLocked = true;

                    // Check for correct size
                    if (depthDescription.Width == this.colorBitmap.Width && depthDescription.Height == this.colorBitmap.Height)
                    {
                        // Effectuer la correspondance espace Profondeur---Couleur 
                        this.kinectSensor.CoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                            depthBuffer.UnderlyingBuffer,
                            depthBuffer.Size,
                            new DepthSpacePoint[colorDescription.Width * colorDescription.Height]);
                    }

                    //---------------------------------------------------------------------------------------------------------
                    //  Modifier le code pour que depthBitmap contienne depthImageBgra au lieu du contenu trame couleur actuel
                    //---------------------------------------------------------------------------------------------------------
                    if (colorDescription.Width == this.colorBitmap.Width && colorDescription.Height == this.colorBitmap.Height)
                    {
                        // Note: In order to see the maximum reliable distance from the kinect (4.5meters), 
                        // we are using the DepthMaxReliableDistance methode.  
                        ushort maxDepth = ushort.MaxValue;
                        maxDepth = depthFrame.DepthMaxReliableDistance;

                        // Ajust the depth data and plot it.
                        this.ProcessDepthFrameData(
                            depthBuffer.UnderlyingBuffer, 
                            depthBuffer.Size, 
                            depthFrame.DepthMinReliableDistance, 
                            maxDepth,
                            depthDescription.BytesPerPixel);

                        //--------------------------------------------
                        // Creation de la seconde image
                        //--------------------------------------------
                        var newImage = GenerateNewImage(colorFrame, this.depthImageBgra.Bytes);

                        this.depthBitmap.WritePixels(
                                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                                newImage,
                                this.depthBitmap.PixelWidth * 4,
                                0);
                    }

                    // Unlock the depthBitmap
                    this.depthBitmap.Unlock();
                    isDepthBitmapLocked = false;
                }

                // We are done with the depthFrame, dispose of it
                depthFrame.Dispose();
                depthFrame = null;

                // We are done with the ColorFrame, dispose of it
                colorFrame.Dispose();
                colorFrame = null;
            }
            finally
            {
                if (isColorBitmapLocked)
                {
                    this.colorBitmap.Unlock();
                }

                if (isDepthBitmapLocked)
                {
                    this.depthBitmap.Unlock();
                }

                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                }

                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                }
            }
        }

        public ImageSource ColorSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        public ImageSource DepthSource
        {
            get
            {
                return this.depthBitmap;
            }
        }

        /// <summary>
        /// Used for setting the image presentation window.
        /// </summary>
        private void InitializeComponentsSize()
        {
            // Get the screen size
            Screen[] screens = Screen.AllScreens;
            this.screenWidth = screens[0].Bounds.Width;
            this.screenHeight = screens[0].Bounds.Height;

            // Make the application full screen
            this.Width = this.screenWidth;
            this.Height = this.screenHeight;
            this.MainWindow1.Width = this.screenWidth;
            this.MainWindow1.Height = this.screenHeight;

            // Make the Grid container full screen
            this.Grid1.Width = this.screenWidth;
            this.Grid1.Height = this.screenHeight;

            // Make the PictureBox1 half the screen width and full screen height
            this.PictureBox1.Width = this.screenWidth;
            this.PictureBox1.Height = this.screenHeight /2;

            // Make the PictureBox2 half the screen width and full screen height
            this.PictureBox2.Width = this.screenWidth;
            this.PictureBox2.Margin = new Thickness(0, 0, 0, 0);
            this.PictureBox2.Height = this.screenHeight /2;
        }

        /// <summary>
        /// Used for creating the header pixel code for 3D television decoder.
        /// </summary>
        private void InitializeHeader()
        {
            var applicationPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            var header = HeaderFactory.Create(HeaderType.TopAndDown);

            header.EnsureBitmap(applicationPath);

            this.HeaderImage.Source = header.HeaderImage.ToImageSource();
        }

        /// <summary>
        /// Used for disposing the frameReader et the KinectSensor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.frameReader != null)
            {
                this.frameReader.Dispose();
                this.frameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }


        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// 
        /// That code was inspired from the documentation of the SDK module: DepthBasics-WPF
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth, uint bytesPerPixel)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            //Transformation of the depth in mm into a intensity of grey
            int MapDepthToByte = -1 * (maxDepth / 256);

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / bytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }

            //  Utiliser la ligne ci-dessous pour l'image de profondeur
            Image<Gray, byte> depthImageGray = new Image<Gray, byte>(RAWCOLORWIDTH, RAWCOLORHEIGHT);
            
            //-----------------------------------------------------------
            // Traiter l'image de profondeur 
            //-----------------------------------------------------------
            depthImageGray.Bytes = depthPixels;

            // Une fois traitée convertir l'image en Bgra
            depthImageBgra = depthImageGray.Convert<Bgra, byte>();

            //Application d'un filtre
            depthImageBgra = depthImageBgra.SmoothGaussian(3);
        }

        /// <summary>
        ///  Method to calculate the dimension of the Dimenco TV.
        /// </summary>
        private void SetTelevisionDimension()
        {
            H = Math.Sqrt(Math.Pow(T, 2) * Math.Pow(TV_Ratio_Height, 2) / (Math.Pow(TV_Ratio_Width, 2) + Math.Pow(TV_Ratio_Height, 2)));
            W = Math.Sqrt(Math.Pow(T, 2) - Math.Pow(H, 2));
            D = 3 * H;
        }

        private byte[] GenerateNewImage(ColorFrame colorFrame, byte[] depthValues)
        {
            // The total size of the color stream, multiplied by a factor of 4 because of the BGRA format
            var colorSize = RAWCOLORHEIGHT * RAWCOLORWIDTH * 4;

            // Source color image
            var leftImage = new byte[colorSize];             
            colorFrame.CopyConvertedFrameDataToArray(leftImage, ColorImageFormat.Bgra);

            // The constructed image to return, initialized with original color data (without disparity)
            var newImage = new byte[colorSize];
            colorFrame.CopyConvertedFrameDataToArray(newImage, ColorImageFormat.Bgra);

            for (int colorIndex = 0; colorIndex < colorSize; colorIndex += 4)
            {
                // Calculating Zp
                var zp = W * ((depthValues[colorIndex] / byte.MaxValue) * (knear + kfar) - kfar);

                // Calculating Disparity
                var p = tc * (1 - (D / (D - zp)));
                var disparity = Convert.ToInt32(Math.Round(p * 1920 / W));

                // Applying the same disparity to each subsequent 4 bytes representing the BGRA color format (1 byte each)
                for (int bgraIndex = 0; bgraIndex < 4; bgraIndex ++)
                {
                    var pixelPosition = colorIndex + bgraIndex;
                    var newPixelPosition = pixelPosition + disparity * 4;

                    // Sanity check to ensure the new pixel position falls within the range of the new image indexes
                    if (newPixelPosition >= 0 && newPixelPosition < newImage.Length)
                    {
                        newImage[newPixelPosition] = leftImage[pixelPosition];
                    }
                }
            }

            return newImage;
        }        
    }
}