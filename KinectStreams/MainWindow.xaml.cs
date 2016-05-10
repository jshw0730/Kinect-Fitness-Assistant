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

    static class DisplayTypes {

        public const int showDefault = 0;
        public const int showUpperSide = 1;
        public const int showDownerSide = 2;
        public const int showAllSide = 3;
    }

    public partial class MainWindow : Window 
        {

        #region gesture_fields
  
        private List<GestureDetector> gestureDetectorList = null;
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

        int countGes = 0; //checking count
        int timeStamp = 180; //time limit for one Gesture (for count)
        int savingBeforeGesture = 0;

        MotionCheck motionChecker = new MotionCheck();


        public static int _displayDegree =  DisplayTypes.showDefault;
            
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
                GestureResultView result = new GestureResultView(i, false, false, 0.0f,0, null);
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



            //text display part
            --timeStamp;
            this.Timestamp.Text = timeStamp.ToString();
            if ( timeStamp <= 0 ) { countGes = 0; _displayDegree = DisplayTypes.showDefault; }
            this.count.Text = "count : " + countGes.ToString();


            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame()) {
                if (frame != null) {  
                    if (_mode == Mode.Color) {   camera.Source = frame.ToBitmap();   }   }
            }
            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame()) {
                if (frame != null) {  
                    if (_mode == Mode.Depth) {   camera.Source = frame.ToBitmap();   }   }
            }
            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame()) {
                if (frame != null) {  
                    if (_mode == Mode.Infrared) {  camera.Source = frame.ToBitmap();  } }
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


                                    //canvas.DrawSkeleton(_filteredJoints);//, body);
                                    
                                    //canvas.DrawSkeleton(body);
                                    //Gesture.WaveHand(this.canvas, body);
                                    //Gesture.Swipe(this.canvas, body);
                                    /**********************************/


                                    if (_displayCoord) {
                                        canvas.DrawSkeletonCoord(_filteredJoints);
                                    }

                                    /*
                                    canvas.DrawIsStraightNeck(_filteredJoints);
                                    canvas.DrawShoulderDegree(_filteredJoints);
                                    canvas.DrawDegreeUpperBody(_filteredJoints);
                                    canvas.DrawIsStraightHand(_filteredJoints);

                                    canvas.DrawIsStraightSpine(_filteredJoints);

                                    canvas.DrawDegreeDownerBody(_filteredJoints);
                                    canvas.DrawIsStraightAnkle(_filteredJoints);
                                    canvas.DrawLeg2LegWidth(_filteredJoints);
                                    */


                                    //have been changed
                                    switch ( _displayDegree ) {

                                    case DisplayTypes.showDefault: { //0
                                            canvas.DrawIsStraightSpine(_filteredJoints);
                                            break;
                                        }

                                    case DisplayTypes.showUpperSide: {//1
                                            canvas.DrawIsStraightNeck(_filteredJoints);
                                            canvas.DrawShoulderDegree(_filteredJoints);
                                            canvas.DrawDegreeUpperBody(_filteredJoints);
                                            canvas.DrawIsStraightHand(_filteredJoints);
                                            break;
                                        }

                                    case DisplayTypes.showDownerSide: {//2

                                            //if (jointA.TrackingState == TrackingState.NotTracked || 
                                            //    jointB.TrackingState == TrackingState.NotTracked || 
                                            //    jointC.TrackingState == TrackingState.NotTracked) return;
                                                                          
                                            canvas.DrawDegreeDownerBody(_filteredJoints);
                                            canvas.DrawIsStraightAnkle(_filteredJoints);
                                            canvas.DrawLeg2LegWidth(_filteredJoints);
                                            break;
                                        }
                                    case DisplayTypes.showAllSide: {
                                            canvas.DrawDegreeDownerBody(_filteredJoints);
                                            canvas.DrawIsStraightAnkle(_filteredJoints);
                                            canvas.DrawLeg2LegWidth(_filteredJoints);
                                            canvas.DrawIsStraightNeck(_filteredJoints);
                                            canvas.DrawShoulderDegree(_filteredJoints);
                                            canvas.DrawDegreeUpperBody(_filteredJoints);
                                            canvas.DrawIsStraightHand(_filteredJoints);
                                            break;

                                        }
                                    default: {
                                            System.Windows.MessageBox.Show("something's going wrong");
                                            break;
                                            }

                                    }
                                    
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

        }//end_method


        //////////added for gesture
        void GestureResult_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            GestureResultView result = sender as GestureResultView;
            
            //part of canvas text
            this.GestureNotifier.Text = result.GestureKind;

            //sidelift counting algorithm
            switch (result.GestureKind) {


                #region sidelift

                case "sidelift" :{                    
                    switch (result.GestureNumber) {
                        case 1: {
                                motionChecker.checkSidelift[0] = true;
                                break;
                            }

                        case 2: {
                                motionChecker.checkSidelift[1] = true;

                                //counting part
                                if (motionChecker.checkSidelift[1]) {

                                    //it's end of one motion
                                    if (motionChecker.checkSidelift[2]) {               // A-B-C-B 도달
                                        motionChecker.initializeChecker();              //initializing checkers to false

                                        motionChecker.checkMotionEnd = true;            //운동동작 하나 완성 
                                        _displayDegree = motionChecker.showInfoSidelift;    //해당 운동에 맞는 정보 표시
                                        timeStamp = 180;
                                    }

                                    // it's starting of one motion
                                    else { 
                                        motionChecker.checkSidelift[1] = true;
                                    }
                                }

                                break;
                            }

                        case 3: {
                                motionChecker.checkSidelift[2] = true;
                                break;
                            }
                    }
                    break;
                    }

                #region b4 code
                /*
                case "sideliftA": {
                    motionChecker.checkSidelift[0] = true;
                    //_displayDegree = DisplayTypes.showUpperSide;
                    break;
                }

                case "sideliftB": { 
                    motionChecker.checkSidelift[1] = true;

                    //counting part
                    if (motionChecker.checkSidelift[1]) { 
                        
                        if( motionChecker.checkSidelift[2]) {               // A-B-C-B 도달
                            motionChecker.initializeChecker();              //initializing checkers to false

                            motionChecker.checkMotionEnd = true;            //운동동작 하나 완성 
                            _displayDegree = DisplayTypes.showUpperSide;    //해당 운동에 맞는 정보 표시
                            timeStamp = 180;
                        }//it's end of one motion

                        else{   //A-B 도달
                            motionChecker.checkSidelift[1] = true;
                        }// it's starting of one motion 
                    }
                    
                    break;                    
                }

                case "sidelift": {
                    motionChecker.checkSidelift[2] = true;
                    break;
                    }
                 */
                #endregion

                #endregion sidelift

                #region squat

                case "squat": {
                        switch (result.GestureNumber) {
                            case 1: {
                                motionChecker.checkSquat[0] = true;
                                break;
                            }

                            case 2: {
                                motionChecker.checkSquat[1] = true;

                                //counting part
                                if (motionChecker.checkSquat[1]) {

                                    //it's end of one motion
                                    if (motionChecker.checkSquat[2]) {                  // A-B-C-B 도달
                                        motionChecker.initializeChecker();          //initializing checkers to false

                                        motionChecker.checkMotionEnd = true;        //운동동작 하나 완성 
                                        _displayDegree = motionChecker.showInfoSquat;  //해당 운동에 맞는 정보 표시
                                        timeStamp = 180;
                                    }

                                    // it's starting of one motion
                                    else {
                                        motionChecker.checkSquat[1] = true;
                                    }
                                }

                                 break;
                            }

                            case 3: {
                                motionChecker.checkSquat[2] = true;
                                break;
                            }
                    }
                    break;
                }
                #endregion squat

                #region shoulderpress

                case "shoulderpress": {
                        switch (result.GestureNumber) {
                            case 1: {
                                     motionChecker.checkShoulderpress[0] = true;
                                    break;
                                }

                            case 2: {
                                    motionChecker.checkShoulderpress[1] = true;

                                    //counting part
                                    if (motionChecker.checkShoulderpress[1]) {

                                        //it's end of one motion
                                        if (motionChecker.checkShoulderpress[2]) {      // A-B-C-B 도달
                                            motionChecker.initializeChecker();          //initializing checkers to false

                                            motionChecker.checkMotionEnd = true;        //운동동작 하나 완성 
                                            _displayDegree = motionChecker.showInfoShoulderpress;  //해당 운동에 맞는 정보 표시
                                            timeStamp = 180;
                                        }

                                        // it's starting of one motion
                                        else {
                                            motionChecker.checkShoulderpress[1] = true;
                                        }
                                    }

                                    break;
                                }

                            case 3: {
                                    motionChecker.checkShoulderpress[2] = true;
                                    break;
                                }
                        }
                        break;
                    }
                #endregion shoulderpress



            }//end all


                    /*
            if ( result.GestureKind == "sideliftB" ) {
                motionChecker.checkSidelift[1] = true;
                _displayDegree = result.GestureNumber;

                //counting part
                if ( motionChecker.checkSidelift[1] && motionChecker.checkSidelift[2] ) {
                    motionChecker.initializeChecker();    //initializing checkers to false

                    motionChecker.checkMotionEnd = true;
                    timeStamp = 180;

                }//both are true, its end of one motion

                else if ( motionChecker.checkSidelift[1] && !motionChecker.checkSidelift[2] ) { 
                    motionChecker.checkSidelift[1] = true; 
                } // it's starting of one motion 
            }


            if ( result.GestureKind == "sidelift" ) { 
                motionChecker.checkSidelift[2] = true;
                _displayDegree = result.GestureNumber;
            }
            if ( result.GestureKind == "sideliftA" ) { 
                motionChecker.checkSidelift[0] = true;
                _displayDegree = result.GestureNumber;
            }
            */

            if ( motionChecker.checkMotionEnd ) {// &&_displayDegree == savingBeforeGesture   ) { 
                ++countGes;
                motionChecker.checkMotionEnd = false;
                motionChecker.initializeChecker();
            } //if time is not over, then go up for count

            savingBeforeGesture = _displayDegree; //backing up beforeGesture

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


        private void Coord_Click(object sender, RoutedEventArgs e) {
            /*
            if (_displayBody == true) {
                if (_displayDegree == true) {
                    _displayDegree = !_displayDegree;
                    _displayCoord = !_displayCoord;
                }
                else { _displayCoord = !_displayCoord; }
            }
            */

            //_displayDegree = DisplayTypes.showDownerSide;
        }

        private void Degree_Click(object sender, RoutedEventArgs e) {
            /*
            if (_displayBody == true) {
                if (_displayCoord == true) {
                    _displayCoord = !_displayCoord;
                    _displayDegree = !_displayDegree;
                }
                else { _displayDegree = !_displayDegree; }
            }
            */
            //_displayDegree = DisplayTypes.showUpperSide;

        }

        #endregion


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
