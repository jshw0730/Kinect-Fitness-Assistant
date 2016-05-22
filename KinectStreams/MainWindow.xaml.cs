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

    static class GestureTypes {
        public const int sidelift = 1;
        public const int squat = 2;
        public const int shoulderpress = 3;
        public const int row = 4;

        public const int lunge = 5;
        public const int frontlift = 6;
        public const int deadlift = 7;
        public const int biceps_curl = 8;

        public const int showDefault = 0;

        public const int showAllSide = 100;
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

        #region education mode fields

        //public 


        #endregion


        #region gesture_fields

        //private List<GestureDetector> gestureDetectorList = null;
        private GestureDetector[] gestureDetector = new GestureDetector[8];
       
        //public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Members

        public enum Mode
        {
            Color,
            Depth,
            Infrared
        }

        Mode _mode = Mode.Color;    //시작모드 [color,depth,infrared]

        KinectSensor _sensor;   //센서
        MultiSourceFrameReader _reader; //프레임리더 
        //IList<Body> _bodies;    //6개 바디 저장하는 리스트
        //Body[] bodiesArray;

        KinectJointFilter _filter = new KinectJointFilter();    //노이즈 잡는 필터
        CameraSpacePoint[] _filteredJoints;                     //필터에 들어갔다온 joint

       
        bool _displayBody = true;//false;   //body 표시
        bool _displayCoord = false;         //좌표 표시

        int countGes = 0; //checking count
        int timeStamp = 18000; //time limit for one Gesture (for count)
        int motionInterval = 30;

        bool visitB = false;    //b방문여부, abC에서 체크
        bool visitC = false;    //C방문여부, abcB 에서 체크

        int cntGesNum = 0;      //현재 제스쳐 번호(A=0, B=1, C=2)
        string cntGesture = "any";  //현 동작 이름, 초기화 any 
        string b4Gesture = "any";   //이전 동작 이름,  초기화 any

        MotionCheck motionChecker = new MotionCheck();  //모션체커, 1회운동 판단에 사용


        public static int _displayDegree = GestureTypes.showAllSide;//GestureTypes.showDefault;   //현 화면에 보여줄 세팅

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

            GestureResultView[] result;
            result = new GestureResultView[8];

            GestureDetector[] detector;
            detector = new GestureDetector[8];

            for(int i =0; i<8 ; i++){
                result[i] = new GestureResultView(0,false,false,0.0f,0,null);
                detector[i] = new GestureDetector(this._sensor, result[i], i+1);
            }

            for (int i = 0; i < 8; i++) {
                result[i].PropertyChanged += GestureResult_PropertyChanged;
                this.gestureDetector[i] = detector[i];
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
            if (reference == null) {
                return;
            }



            //text display part
            --timeStamp;
            --motionInterval;
            this.Timestamp.Text = timeStamp.ToString();
            this.motionTimer.Text = motionInterval.ToString();
            if (timeStamp <= 0) {
                motionChecker.countSetter(cntGesture,motionChecker.countGetter(cntGesture) + countGes);
                countGes = 0;
                _displayDegree = GestureTypes.showDefault;
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
                    skeletonViwer.Children.Clear();

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

                    if (body != null) {
                        if (body.IsTracked) {


                            // Draw skeleton.
                            if (_displayBody) {
                                //copy joints to check motion
                                motionChecker.setJoints(_filteredJoints);

                                //convert body data to be smoothed
                                _filter.UpdateFilter(body);
                                _filteredJoints = _filter.GetFilteredJoints();

                                if (body.IsTracked) { 
                                    skeletonViwer.DrawSkeleton( _filteredJoints, true);
                                }

                                //display xyz coordinate
                                if (_displayCoord) {
                                    canvas.DrawSkeletonCoord(_filteredJoints);
                                }

                                for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++) {

                                    // 3D space point
                                    CameraSpacePoint jointPosition = _filteredJoints[(int)jt];

                                    // 2D space point
                                    Point point = new Point();

                                    if (_mode == Mode.Color) {
                                        ColorSpacePoint colorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(jointPosition);

                                        point.X = double.IsInfinity(colorPoint.X) ? 0 : colorPoint.X;
                                        point.Y = double.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y;
                                    }
                                    else if (_mode == Mode.Depth || _mode == Mode.Infrared) {// Change the Image and Canvas dimensions to 512x424 {
                                        DepthSpacePoint depthPoint = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);

                                        point.X = double.IsInfinity(depthPoint.X) ? 0 : depthPoint.X;
                                        point.Y = double.IsInfinity(depthPoint.Y) ? 0 : depthPoint.Y;
                                    }


                                    _filteredJoints[(int)jt].X = Convert.ToSingle(point.X);
                                    _filteredJoints[(int)jt].Y = Convert.ToSingle(point.Y);
                                }
                            
                                //canvas.DrawSkeleton(_filteredJoints);


                                //have been changed
                                switch (_displayDegree) {

                                    case GestureTypes.showDefault: { //0
                                            //canvas.DrawIsStraightSpine(_filteredJoints);
                                            break;
                                    }


                                    case GestureTypes.sidelift: {
                                            canvas.DrawSideliftInfo(_filteredJoints);
                                            break;
                                    }
                                    case GestureTypes.squat: {//2
                                            canvas.DrawSquatInfo(_filteredJoints);
                                            break;
                                    }
                                    case GestureTypes.shoulderpress: {//3
                                            canvas.DrawShoulderpressInfo(_filteredJoints);
                                            break;
                                    }
                                    case GestureTypes.row: {//4
                                            canvas.DrawRowInfo(_filteredJoints);
                                            break;
                                    }



                                    case GestureTypes.lunge: {//5
                                            canvas.DrawLungeInfo(_filteredJoints);
                                            break;
                                    }
                                    case GestureTypes.frontlift: {//6
                                            canvas.DrawFrontliftInfo(_filteredJoints);
                                            break;
                                    }
                                    case GestureTypes.deadlift: {//7
                                            canvas.DrawDeadliftInfo(_filteredJoints);
                                            break;
                                    }
                                    case GestureTypes.biceps_curl: {//8
                                            canvas.DrawBiceps_curlInfo(_filteredJoints);
                                            break;
                                    }

                                    case GestureTypes.showAllSide: {//100
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


                        RegisterGesture(body);
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
                        /*    
                        CameraSpacePoint[] cntJoints = motionChecker.getJoints();
                            
                            //sidelift and shoulderpress classifier
                            if ( cntJoints != null) {
                                if (cntJoints[(int)JointType.HandLeft].Y > cntJoints[(int)JointType.Head].Y) {
                                    //손이 머리위로 가는 경우는 숄더프레스의 C동작밖에 없다. 
                                    //따라서 이 안에 사이드리프트와 별도로 잡는 로직을 투입한다.
                                    //
                                }
                            }
                        */
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
                              && (motionInterval<0)
                             ) {      // (a-B-C-B-A ?)

                                //it's end of one motion
                                motionChecker.initializeChecker();          //initializing checkers to false
                                visitB = false;
                                visitC = false;

                                this.motioncheckerTF.Text = cntGesture; //for debug, display current gesture

                                b4Gesture = cntGesture; //save current ges for lock

                                motionChecker.checkMotionEnd = true;        //운동동작 하나 완성 


                                _displayDegree = motionChecker.showInfoGetter(cntGesture);  //해당 운동에 맞는 정보 표시
                                timeStamp = 180; //reset timestamp 
                                motionInterval = 45;
                            }

                            //initializing part
                            //motionChecker.initializeChecker();
                            //visitB = false;
                            //visitC = false;
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
            timeStamp = 10000;
        }

        private void Degree_Click(object sender, RoutedEventArgs e) {
            timeStamp = 1;

        }

        #endregion

        private void RegisterGesture(Body body) {

            if (body != null) {
             
                ulong trackingId = body.TrackingId;

                for (int i = 0; i < 8; i++) {
                    if (trackingId != this.gestureDetector[i].TrackingId) {
                        this.gestureDetector[i].TrackingId = trackingId;
                        this.gestureDetector[i].IsPaused = trackingId == 0;
                    }
                }
            }
        }

    }

}
