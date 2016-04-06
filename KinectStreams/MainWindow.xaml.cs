using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using System.ComponentModel;

/*
using global::Windows.Storage;
using global::Windows.Storage.Pickers;
using global::Windows.Graphics.Display;
using global::Windows.Graphics.Imaging;
*/


namespace KinectStreams
{

    public partial class MainWindow : Window 
        {

        #region gesture_fields
        /// <summary>
        /// Interaction logic for MainWindow.xaml
        /// </summary>
        //Face Orientation Shaping
        private double prevPitch = 0.0f;
        private double prevYaw = 0.0f;

        /// <summary> List of gesture detectors, 
        ///there will be one detector created for each potential body
        /// (max of 6) </summary>
        private List<GestureDetector> gestureDetectorList = null;
        public bool isTakingScreenshot = false;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion



        #region Members

        Mode _mode = Mode.Color;

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        KinectJointFilter _filter = new KinectJointFilter();
        CameraSpacePoint[] _filteredJoints;
        

        bool _displayBody = true;//false;
        bool _displayCoord = false;
        bool _displayDegree = true;

        #endregion

        #region Constructor

        public MainWindow() {
            InitializeComponent();
        }

        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            _sensor = KinectSensor.GetDefault();
            
            if (_sensor != null) {
                _sensor.Open();      

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Infrared | FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }


            ///adding event
            // Initialize the gesture detection objects for our gestures
            this.gestureDetectorList = new List<GestureDetector>();

            // Create a gesture detector for each body (6 bodies => 6 detectors)
            int maxBodies = _sensor.BodyFrameSource.BodyCount;
            for ( int i = 0; i < maxBodies; ++i ) {
                GestureResultView result = new GestureResultView(i, false, false, 0.0f);
                GestureDetector detector = new GestureDetector(this._sensor  , result);
                result.PropertyChanged += GestureResult_PropertyChanged;
                this.gestureDetectorList.Add(detector);
            }
            //////////////////////////
        }

        private void Window_Closed(object sender, EventArgs e) {
            if (_reader != null) {
                _reader.Dispose();
            }

            if (_sensor != null) {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e) {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame()) {
                if (frame != null) {
                    if (_mode == Mode.Color) {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame()) {
                if (frame != null) {
                    if (_mode == Mode.Depth) {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame()) {
                if (frame != null) {
                    if (_mode == Mode.Infrared) {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame()) {
                if (frame != null) {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies) {
                        if (body != null) {
                            if (body.IsTracked) {
                                // Draw skeleton.
                                if (_displayBody) {

                                    _filter.UpdateFilter(body);
                                    _filteredJoints = _filter.GetFilteredJoints();
                                    canvas.DrawSkeleton(_filteredJoints);//, body);
                                    
                                    //canvas.DrawSkeleton(body);
                                    //Gesture.WaveHand(this.canvas, body);
                                    //Gesture.Swipe(this.canvas, body);
                                    /**********************************/
                                    if (_displayCoord) {
                                        canvas.DrawSkeletonCoord(_filteredJoints);
                                   //     Gesture.WaveHand(this.canvas, body);
                                    }

                                    if (_displayDegree) {
                                        canvas.DrawDegreeOnBody(_filteredJoints);
                                    }
                                    /**********************************/
                                    
                                }
                            }
                        }
                    }
                }
            }

            ////////added for gesture
            using ( var bodyFrame = reference.BodyFrameReference.AcquireFrame() ) {
                RegisterGesture(bodyFrame);
            }
            /////gesture added///////////

        }//end_method


        //////////added for gesture
        void GestureResult_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            GestureResultView result = sender as GestureResultView;
            if ( result.Confidence > 0.8 ) {
                System.Windows.MessageBox.Show("detected");
             
               // Screenshot();
            }        
        }



        private void Color_Click(object sender, RoutedEventArgs e) {
            _mode = Mode.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e) {
            _mode = Mode.Depth;
        }

        private void Infrared_Click(object sender, RoutedEventArgs e) {
            _mode = Mode.Infrared;
        }

        private void Body_Click(object sender, RoutedEventArgs e) {
            _displayBody = !_displayBody;
        }


        #region addedevent
        private void Coord_Click(object sender, RoutedEventArgs e) {

            if (_displayBody == true) {
                if (_displayDegree == true) {
                    _displayDegree = !_displayDegree;
                    _displayCoord = !_displayCoord;
                }
                else { _displayCoord = !_displayCoord; }
            }
        }

        private void Degree_Click(object sender, RoutedEventArgs e) {

            if (_displayBody == true) {
                if (_displayCoord == true) {
                    _displayCoord = !_displayCoord;
                    _displayDegree = !_displayDegree;
                }
                else { _displayDegree = !_displayDegree; }
            }


        }
        #endregion addedevent

        #endregion





            /*
        async private void Screenshot() {
            // Thread protetction on FileIO actions
            if ( !isTakingScreenshot ) {
                isTakingScreenshot = true;
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
                await renderTargetBitmap.Render(grid)
                var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

                var savePicker = new FileSavePicker();
                savePicker.DefaultFileExtension = ".png";
                savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                savePicker.SuggestedFileName = "snapshot.png";

                // Prompt the user to select a file
                var saveFile = await savePicker.PickSaveFileAsync();

                // Verify the user selected a file
                if ( saveFile != null ) {
                    // Encode the image to the selected file on disk
                    using ( var fileStream =
                        await saveFile.OpenAsync(FileAccessMode.ReadWrite) ) {
                        var encoder =
                            await BitmapEncoder.CreateAsync(
                                  BitmapEncoder.PngEncoderId,
                                  fileStream);
                        encoder.SetPixelData(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Ignore,
                            (uint)renderTargetBitmap.PixelWidth,
                            (uint)renderTargetBitmap.PixelHeight,
                            DisplayInformation.GetForCurrentView().LogicalDpi,
                            DisplayInformation.GetForCurrentView().LogicalDpi,
                            pixelBuffer.ToArray());
                        await encoder.FlushAsync();
                    }
                }
                isTakingScreenshot = false;
            }
        }
            */

        private void RegisterGesture(BodyFrame bodyFrame) {
            bool dataReceived = false;
            Body[] bodies = null;

            if ( bodyFrame != null ) {
                if ( bodies == null ) {
                    // Creates an array of 6 bodies, which is the max 
                    // number of bodies the Kinect can track simultaneously
                    bodies = new Body[bodyFrame.BodyCount];
                }

                // The first time GetAndRefreshBodyData is called, 
                // allocate each Body in the array.
                // As long as those body objects are not disposed and 
                // not set to null in the array,
                // those body objects will be re-used.
                bodyFrame.GetAndRefreshBodyData(bodies);
                dataReceived = true;
            }

            if ( dataReceived ) {
                // We may have lost/acquired bodies, 
                // so update the corresponding gesture detectors
                if ( bodies != null ) {
                    // Loop through all bodies to see if any 
                    // of the gesture detectors need to be updated
                    for ( int i = 0; i < bodyFrame.BodyCount; ++i ) {
                        Body body = bodies[i];
                        ulong trackingId = body.TrackingId;

                        // If the current body TrackingId changed, 
                        // update the corresponding gesture detector with 
                        // the new value

                        
                        if ( trackingId != this.gestureDetectorList[i].TrackingId ) {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // If the current body is tracked, unpause its 
                            // detector to get 
                            // VisualGestureBuilderFrameArrived events
                            // If the current body is NOT tracked, pause its
                            // detector so we don't waste resources trying to get 
                            // invalid gesture results
                            this.gestureDetectorList[i].IsPaused =
                                 trackingId == 0;
                        }
                        
                    }
                }
            }
        }

    }

    public enum Mode
    {
        Color,
        Depth,
        Infrared
    }
}
