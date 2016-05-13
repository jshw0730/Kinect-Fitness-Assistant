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

    static class gKind
    {
        public const int gestureA = 0;
        public const int gestureB = 1;
        public const int gestureC = 2;
        public const int gesture = 2;

        /*
        public const int sideliftA = 1;
        public const int sideliftB = 2;
        public const int sidelift = 3;

        public const int squatA = 4;
        public const int squatB = 5;
        public const int squat = 6;

        public const int shoulderpressA = 7;
        public const int shoulderpressB = 8;
        public const int shoulderpress = 9;

        public const int rowA = 10;
        public const int rowB = 11;
        public const int row = 12;

        public const int lungeA = 13;
        public const int lungeB = 14;
        public const int lunge = 15;

        public const int frontliftA = 16;
        public const int frontliftB = 17;
        public const int frontlift = 18;

        public const int deadliftA = 19;
        public const int deadliftB = 20;
        public const int deadlift = 21;

        public const int biceps_curlA = 22;
        public const int biceps_curlB = 23;
        public const int biceps_curl = 24;
        */ 
    }

    public partial class MainWindow : Window 
        {

        #region gesture_fields
  
        private List<GestureDetector> gestureDetectorList = null;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        
        #region Members

        Mode _mode = Mode.Depth;

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        KinectJointFilter _filter = new KinectJointFilter();
        CameraSpacePoint[] _filteredJoints;
        

        bool _displayBody = true;//false;
        bool _displayCoord = false;

        int countGes = 0; //checking count
        int timeStamp = 180; //time limit for one Gesture (for count)

        bool visitB = false;
        bool visitC = false;

        int cntGesNum = 0;
        string cntGesture = "any";
        string b4Gesture = "any";

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


            // If the Frame has expired by the time we process this event, return.
            if (reference == null)
            {
                return;
            }
   

            //text display part
            --timeStamp;
            this.Timestamp.Text = timeStamp.ToString();
            if (timeStamp <= 0) {
                motionChecker.countSetter(cntGesture,
                                           motionChecker.countGetter(cntGesture) + countGes);
                countGes = 0; 
                _displayDegree = DisplayTypes.showDefault;
                this.motioncheckerTF.Text = "";
                b4Gesture = "any";
            }
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

                    int bodycounts = frame.BodyFrameSource.BodyCount;
                    _bodies = new Body[bodycounts];

                    frame.GetAndRefreshBodyData(_bodies);


                    /****Closest person gets control*****///
/*
private void Reader_MultiSourceFrameArrived(
      MultiSourceFrameReader sender, 
      MultiSourceFrameArrivedEventArgs e)
{
   MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
   
   // If the Frame has expired by the time we process this event, return.
   if (multiSourceFrame == null)
   {
      return;
   }
   
   using (bodyFrame = 
      multiSourceFrame.BodyFrameReference.AcquireFrame())
   {
      int activeBodyIndex = -1; // Default to impossible value.
      Body[] bodiesArray = new Body[
         this.kinectSensor.BodyFrameSource.BodyCount];
	  
      if (bodyFrame != null)
      {
         bodyFrame.GetAndRefreshBodyData(bodies);
         
         // Iterate through all bodies, 
         // no point persisting activeBodyIndex because must 
         // compare with depth of all bodies so no gain in efficiency.

         float minZPoint = float.MaxValue; // Default to impossible value
         for (int i = 0; i < bodiesArray.Length; i++)
         {
            body = bodiesArray[i];
            if (body.IsTracked)
            {
               float zMeters = 
                  body.Joints[JointType.SpineBase].Position.Z;
               if (zMeters < minZPoint)
               {
                  minZPoint = zMeters;
                  activeBodyIndex = i;
               }
            }
         }

         
         // If active body is still active after checking and 
         // updating, use it
         if (activeBodyIndex != -1)
         {
            Body body = bodiesArray[activeBodyIndex];
            // Do stuff with known active body.
         }
      }
   }
}
*/

/*
 * 
 * 현재 문제인점.
 * foreach를 갈아야한다. 
 * 이걸 갈아서 하나의 body에 대해서만 값을 보도록 설정해야함.
 * 이 작업을 하면 하나의 body에 대해서만 작업을 하도록 만들 수 있고, 
 * 또한 모든 body에 대해서 작업하지 않으므로 작업량 자체가 줄어들게 된다.
 * 물론 실질적인 작업량은 크게 줄지 않고 (화면에 대한 표시만 안하게 되므로...)
 * 겉보기에만 덜 작업하는 것으로 판별될 것.
 * 가장 큰 문제는 표기는 하나로 되는데 제스쳐는 따로 인식하는 경우인데, 이때는 진짜 답이없다. 시발.
*/


   

                    foreach (Body body in _bodies) {
                        if (body != null) {
                            if (body.IsTracked) {
             


                                // Draw skeleton.
                                if ( _displayBody) {

                                    _filter.UpdateFilter(body);
                                    _filteredJoints = _filter.GetFilteredJoints();


                                    canvas.DrawSkeleton(_filteredJoints);//, body);
                                    
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


        //gesture
        void GestureResult_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            GestureResultView result = sender as GestureResultView;
            
            //part of canvas text
            this.GestureNotifier.Text = result.GestureKind; //show what gesture is getting
            this.GestureNotifier.Text += result.GestureNumber;


            #region common logic            
            
            cntGesNum = result.GestureNumber;
            cntGesture = result.GestureKind;
            
            if ((cntGesture == b4Gesture) || (b4Gesture == "any")) {

                switch (cntGesNum) {
                    case gKind.gestureB: {
                            motionChecker.checkSetter(cntGesture, cntGesNum, true); //B on
                            if (motionChecker.checkGetter(cntGesture, gKind.gestureC)) {
                                visitC = true;  //두번쨰 B방문시 앞서 C를 방문했는지 체크함 (a->B->C->B ?)
                            }
                            break;
                        }
                    case gKind.gestureC: {
                            motionChecker.checkSetter(cntGesture, cntGesNum, true); //C on
                            if (motionChecker.checkGetter(cntGesture, gKind.gestureB)) {
                                visitB = true; //C방문시 앞서 B를 방문했는지 체크함(a->B->C ?)
                            }
                            break;
                        }
                        
                    case gKind.gestureA: {

                            //counting part
                            if (motionChecker.checkGetter(cntGesture, gKind.gestureB)
                              && motionChecker.checkGetter(cntGesture, gKind.gestureC)
                              && visitB 
                              && visitC  
                             ) {      // (a-B-C-B-A ?)

                                //it's end of one motion
                                motionChecker.initializeChecker();          //initializing checkers to false

                                this.motioncheckerTF.Text += cntGesture; //for debug, display current gesture

                                b4Gesture = cntGesture; //save current ges for lock

                                motionChecker.checkMotionEnd = true;        //운동동작 하나 완성 

                                _displayDegree = motionChecker.showInfoGetter(cntGesture);  //해당 운동에 맞는 정보 표시
                                timeStamp = 180; //reset timestamp 
                            }

                            //initializing part
                            motionChecker.initializeChecker(); 
                            visitB = false;
                            visitC = false; 

                            motionChecker.checkSetter(cntGesture, cntGesNum, true); //A on
                            break;
                        }

                }
            }
            #endregion
                  

            if ( motionChecker.checkMotionEnd ) {
                motionChecker.checkMotionEnd = false;
                this.motioncheckerTF.Text += " ";

                motionChecker.initializeChecker();


                ++countGes;

            } //if time is not over, then go up for count


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
