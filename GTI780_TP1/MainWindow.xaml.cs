using System;
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
//using System.Windows.Shapes;

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
        private CoordinateMapper coordinateMapper;

        
        //A revoir
        private CameraSpacePoint cameraPoint;
        private CameraSpacePoint body;
        private CameraMode _mode = CameraMode.Color; //Un enum

        public MainWindow()
        {
            InitializeComponent();

            // Sets the correct size to the display components
            InitializeComponentsSize();

            // Set Header image for 2d color + depth side by side format
            InitializeHeader();

            // Instanciate the WriteableBitmaps used to display the kinect frames
            this.colorBitmap = new WriteableBitmap(RAWCOLORWIDTH, RAWCOLORHEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.depthBitmap = new WriteableBitmap(RAWDEPTHWIDTH, RAWDEPTHHEIGHT, 96.0, 96.0, PixelFormats.Gray16, null); //Quel type de gris??

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


            IList<Body> bodies;

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

                using (KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                {
                    // Lock the depthBitmap while we write in it.
                    this.depthBitmap.Lock();
                    isDepthBitmapLocked = true;

                    // Check for correct size
                   // if (depthDescription.Width == this.colorBitmap.Width && depthDescription.Height == this.colorBitmap.Height)
                    //{   //Simon besoin ou pas?

                        //-----------------------------------------------------------
                        // Effectuer la correspondance espace Profondeur---Couleur 
                        //-----------------------------------------------------------
                        //  Utiliser la ligne ci-dessous pour l'image de profondeur
                        Image<Gray, byte> depthImageGray = new Image<Gray, byte>(RAWDEPTHWIDTH, RAWDEPTHHEIGHT);

                        //-----------------------------------------------------------
                        // Traiter l'image de profondeur 
                        //-----------------------------------------------------------

                        //A revoir!!!
                        DepthSpacePoint[] depthSpacePointTable = null;
                        FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;

                        int colorFrameWidht = colorFrameDescription.Width;
                        int colorFrameHeight = colorFrameDescription.Height;
                        depthSpacePointTable = new DepthSpacePoint[colorFrameWidht * colorFrameHeight];
                        this.kinectSensor.CoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthSpacePointTable);

                        // Une fois traitée convertir l'image en Bgra
                        Image<Bgra, byte> depthImageBgra = depthImageGray.Convert<Bgra, byte>();
                  //  }

                    //---------------------------------------------------------------------------------------------------------
                    //  Modifier le code pour que depthBitmap contienne depthImageBgra au lieu du contenu trame couleur actuel
                    //---------------------------------------------------------------------------------------------------------
                    if (depthDescription.Width == this.depthBitmap.Width && depthDescription.Height == this.depthBitmap.Height)
                    {
                        //depthFrame.CopyFrameDataToIntPtr(this.depthBitmap.BackBuffer, (uint)(depthDescription.Width * depthDescription.Height * 2));
                        depthFrame.CopyFrameDataToIntPtr(this.depthBitmap.BackBuffer, (uint)(depthDescription.Width * depthDescription.Height *2 ));

                        // Mark the entire buffer as dirty to refresh the display
                        this.depthBitmap.AddDirtyRect(new Int32Rect(0, 0, depthDescription.Width, depthDescription.Height));
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
            this.PictureBox1.Width = this.screenWidth / 2;
            this.PictureBox1.Height = this.screenHeight;

            // Make the PictureBox2 half the screen width and full screen height
            this.PictureBox2.Width = this.screenWidth / 2;
            this.PictureBox2.Margin = new Thickness(0, 0, 0, 0);
            this.PictureBox2.Height = this.screenHeight;
        }

        private void InitializeHeader()
        {
            var applicationPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
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

        //Simon
        private void DrawPoint(ColorSpacePoint point)
        {
            // Create an ellipse.
            System.Windows.Shapes.Ellipse ellipse = new System.Windows.Shapes.Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = System.Windows.Media.Brushes.Red
            };

            // Position the ellipse according to the point's coordinates.
            Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

            // Add the ellipse to the canvas.
            this.Grid1.Children.Add(ellipse);
        }

        //Simon
        enum CameraMode
        {
            Color,
            Depth,
            Infrared
        }



    }
}