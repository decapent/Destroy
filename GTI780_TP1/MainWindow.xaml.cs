﻿using System;
using System.Collections.Generic;//
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;//
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media; //
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
// using System.Windows.Shapes;

// Includes for the Lab
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Kinect; //
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Shapes; //Simon
using System.Windows.Media;  //Simon
using System.Windows;

using Emgu.CV;

using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using GTI780_TP1.Header;
using GTI780_TP1.Header.Entities;
using GTI780_TP1.Extensions;

namespace GTI780_TP1
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window

    {
        // Size of the raw depth stream
        private const int RAWDEPTHWIDTH = 512;
        private const int RAWDEPTHHEIGHT = 424;

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
        private FrameDescription depthFrameDescription = null;
        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 700 / 256;//**********





        public MainWindow()
        {
            InitializeComponent();

            // Sets the correct size to the display components
            InitializeComponentsSize();

            // Set Header image for 2d color + depth side by side format
            InitializeHeader();

            // Instanciate the WriteableBitmaps used to display the kinect frames
            this.colorBitmap = new WriteableBitmap(RAWCOLORWIDTH, RAWCOLORHEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.depthBitmap = new WriteableBitmap(RAWDEPTHWIDTH, RAWDEPTHHEIGHT, 96.0, 96.0, PixelFormats.Gray8, null); //Quel type de gris??

            // Connect to the Kinect Sensor
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the color frames
            //Modif Simon
            this.frameReader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body); // A modifier si on utilise ou pas le IR + Body

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


         

            if (colorFrame == null || depthFrame == null )
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
                // DepthFrame code block : À modifier et completer 
                // Remarque : Beaucoup de code à modifer/ajouter dans cette partie
                // ================================================================
                FrameDescription depthDescription = depthFrame.FrameDescription;

                // allocate space to put the pixels being received and converted
                this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

                this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

                using (KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                {
                    // Lock the depthBitmap while we write in it.
                    this.depthBitmap.Lock();
                    isDepthBitmapLocked = true;

                    // Check for correct size
                    if (depthDescription.Width == this.colorBitmap.Width && depthDescription.Height == this.colorBitmap.Height)
                    {

                        //-----------------------------------------------------------
                        // Effectuer la correspondance espace Profondeur---Couleur 
                        //-----------------------------------------------------------

                        DepthSpacePoint[] depthSpacePointTable = null;
                        FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;

                        int colorFrameWidht = colorFrameDescription.Width;
                        int colorFrameHeight = colorFrameDescription.Height;
                        depthSpacePointTable = new DepthSpacePoint[colorFrameWidht * colorFrameHeight];
                        this.kinectSensor.CoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthSpacePointTable);

                        //................................//


                        //  Utiliser la ligne ci-dessous pour l'image de profondeur
                        Image<Gray, byte> depthImageGray = new Image<Gray, byte>(RAWDEPTHWIDTH, RAWDEPTHHEIGHT);

                        //-----------------------------------------------------------
                        // Traiter l'image de profondeur 
                        //-----------------------------------------------------------

                      

                        // Une fois traitée convertir l'image en Bgra
                        Image<Bgra, byte> depthImageBgra = depthImageGray.Convert<Bgra, byte>();
                        depthImageBgra = depthImageBgra.SmoothMedian(15);

                    }

                    //---------------------------------------------------------------------------------------------------------
                    //  Modifier le code pour que depthBitmap contienne depthImageBgra au lieu du contenu trame couleur actuel
                    //---------------------------------------------------------------------------------------------------------
                    if (colorDescription.Width == this.colorBitmap.Width && colorDescription.Height == this.colorBitmap.Height)
                    {
                        //depthFrame.CopyFrameDataToIntPtr(this.depthBitmap.BackBuffer, (uint)(depthDescription.Width * depthDescription.Height * 2));
                        // depthFrame.CopyFrameDataToIntPtr(this.depthBitmap.BackBuffer, (uint)(depthDescription.Width * depthDescription.Height *2 ));

                        // Mark the entire buffer as dirty to refresh the display
                        //this.depthBitmap.AddDirtyRect(new Int32Rect(0, 0, depthDescription.Width, depthDescription.Height));



                        // Note: In order to see the full range of depth (including the less reliable far field depth)
                        // we are setting maxDepth to the extreme potential depth threshold
                        ushort maxDepth = ushort.MaxValue;

                        // If you wish to filter by reliable depth distance, uncomment the following line:
                        //// maxDepth = depthFrame.DepthMaxReliableDistance

                        this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                        this.RenderDepthPixels();

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
            this.PictureBox1.Width = this.screenWidth / 8;
            this.PictureBox1.Height = this.screenHeight;

            // Make the PictureBox2 half the screen width and full screen height
            this.PictureBox2.Width = this.screenWidth / 2;
            this.PictureBox2.Margin = new Thickness(0, 0, 0, 0);
            this.PictureBox2.Height = this.screenHeight;
        }

        private void InitializeHeader()
        {
            var applicationPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            var header = HeaderFactory.Create(HeaderType.Stereoscopic);

            header.EnsureBitmap(applicationPath);

            this.HeaderImage.Source = header.HeaderImage.ToImageSource();
        }

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
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }

        private void RenderDepthPixels()
        {
            this.depthBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                this.depthPixels,
                this.depthBitmap.PixelWidth,
                0);
        }
    }
}