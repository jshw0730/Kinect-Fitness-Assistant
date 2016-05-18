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
    }

    public partial class MainWindow : Window
    {

        #region gesture_fields

        //private List<GestureDetector> gestureDetectorList = null;
        private GestureDetector gestureDetector = null;
        //public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Members

        public enum Mode
        {
            Color,
            Depth,
            Infrared
        }

        Mode _mode = Mode.Depth;    //시작모드 [color,depth,infrared]

        KinectSensor _sensor;   //센서
        MultiSourceFrameReader _reader; //프레임리더 
        //IList<Body> _bodies;    //6개 바디 저장하는 리스트
        //Body[] bodiesArray;

        KinectJointFilter _filter = new KinectJointFilter();    //노이즈 잡는 필터
        CameraSpacePoint[] _filteredJoints;                     //필터에 들어갔다온 joint


        bool _displayBody = true;//false;   //body 표시
        bool _displayCoord = false;         //좌표 표시

        int countGes = 0; //checking count
        int timeStamp = 180; //time limit for one Gesture (for count)

        bool visitB = false;    //b방문여부, abC에서 체크
        bool visitC = false;    //C방문여부, abcB 에서 체크

        int cntGesNum = 0;      //현재 제스쳐 번호(A=0, B=1, C=2)
        string cntGesture = "any";  //현 동작 이름, 초기화 any 
        string b4Gesture = "any";   //이전 동작 이름,  초기화 any

        MotionCheck motionChecker = new MotionCheck();  //모션체커, 1회운동 판단에 사용


        public static int _displayDegree = DisplayTypes.showAllSide;//DisplayTypes.showDefault;   //현 화면에 보여줄 세팅

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

            GestureResultView result = new GestureResultView(1, false, false, 0.0f, 0, null);
            GestureDetector detector = new GestureDetector(this._sensor, result);
            result.PropertyChanged += GestureResult_PropertyChanged;
            this.gestureDetector = detector;
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
            if (reference == null) {
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
                    if (_mode == Mode.Color) { camera.Source = frame.ToBitmap(); }
                }
            }
            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame()) {
                if (frame != null) {
                    if (_mode == Mode.Depth) { camera.Source = frame.ToBitmap(); }
                }
            }
            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame()) {
                if (frame != null) {
                    if (_mode == Mode.Infrared) { camera.Source = frame.ToBitmap(); }
                }
            }

            // Body       
            using (var frame = reference.BodyFrameReference.AcquireFrame()) {

                int activeBodyIndex = -1; // Default to impossible value.        
                
                if (frame != null) {
                    canvas.Children.Clear();

                    int bodycounts = frame.BodyFrameSource.BodyCount;
                    Body[] bodiesArray = new Body[bodycounts];
                    frame.GetAndRefreshBodyData(bodiesArray);

                    Body body = null;

                    //for now , closest person first
                    float minZPoint = float.MaxValue; // Default to impossible value
                    for (int i = 0; i < bodiesArray.Length; i++) {
                        body = bodiesArray[i];
                        if (body.IsTracked) {
                            float zMeters =
                               body.Joints[JointType.SpineBase].Position.Z;
                            if (zMeters < minZPoint) {
                                minZPoint = zMeters;
                                activeBodyIndex = i;
                            }
                        }
                    }

                    // If active body is still active after checking and updating, use it
                    if (activeBodyIndex != -1) {
                        body = bodiesArray[activeBodyIndex];
                        // Do stuff with known active body.
                    }


                    //foreach (Body body in _bodies) {
                    if (body != null) {
                        if (body.IsTracked) {

                            RegisterGesture(body);

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
                                switch (_displayDegree) {

                                    case DisplayTypes.showDefault: { //0
                                            //canvas.DrawIsStraightSpine(_filteredJoints);
                                            break;
                                        }

                                    case DisplayTypes.showUpperSide: {//1
                                            //canvas.DrawIsStraightNeck(_filteredJoints);
                                            //canvas.DrawShoulderDegree(_filteredJoints);
                                            //canvas.DrawDegreeUpperBody(_filteredJoints);
                                            //canvas.DrawIsStraightHand(_filteredJoints);
                                            break;
                                        }

                                    case DisplayTypes.showDownerSide: {//2

                                            //if (jointA.TrackingState == TrackingState.NotTracked || 
                                            //    jointB.TrackingState == TrackingState.NotTracked || 
                                            //    jointC.TrackingState == TrackingState.NotTracked) return;
                                        /*
                                            canvas.DrawDegreeDownerBody(_filteredJoints);
                                            canvas.DrawIsStraightAnkle(_filteredJoints);
                                            canvas.DrawLeg2LegWidth(_filteredJoints);
                                         */ 
                                            break;
                                        }
                                    case DisplayTypes.showAllSide: {
                                        /*
                                            canvas.DrawDegreeDownerBody(_filteredJoints);
                                            canvas.DrawIsStraightAnkle(_filteredJoints);
                                            canvas.DrawLeg2LegWidth(_filteredJoints);
                                            canvas.DrawIsStraightNeck(_filteredJoints);
                                            canvas.DrawShoulderDegree(_filteredJoints);
                                            canvas.DrawDegreeUpperBody(_filteredJoints);
                                            canvas.DrawIsStraightHand(_filteredJoints);
                                         */ 
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


            if (motionChecker.checkMotionEnd) {
                motionChecker.checkMotionEnd = false;

                motionChecker.initializeChecker();

                ++countGes;

                this.motioncheckerTF.Text += " ";

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

        }

        private void Degree_Click(object sender, RoutedEventArgs e) {


        }

        #endregion

        private void RegisterGesture(Body body) {

            if (body != null) {
             
                ulong trackingId = body.TrackingId;

                if (trackingId != this.gestureDetector.TrackingId) {
                    this.gestureDetector.TrackingId = trackingId;
                    this.gestureDetector.IsPaused = trackingId == 0;
                }
            }
        }

    }

}
