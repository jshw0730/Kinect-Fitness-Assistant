//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace KinectStreams {
    using System;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;

    /*
    using WindowsPreview.Kinect;
    using Windows.UI.Xaml;
    using WindowsPreview.Data;
    */

    /// <summary>
    /// Gesture Detector class which listens for VisualGestureBuilderFrame events from the service
    /// and updates the associated GestureResultView object with the latest results for the gesture
    /// </summary>
    public class GestureDetector : IDisposable {
        //public RoutedEventHandler GestureRecognized { get; set; }

        //private readonly string gestureDatabase = @"Database\SampleDatabase.gbd";


            
        private readonly string gestureDatabase_forTest = @"Database\gestureForTest.gbd";
        private readonly string gestureForTest = "gestureForTest";



        
       

        private readonly string gestureDatabase_squat = @"Database\gesture_squat.gbd";
        private readonly string squatGestureName = "gesture_squat";
        private readonly string squatProgGestureName = "gesture_squatProgress";


        private readonly string gestureDatabase_sidelift = @"Database\gesture_sidelift.gbd";
            private readonly string sideliftGestureName = "gesture_sidelift";
            private readonly string sideliftGestureName_A = "gesture_sideliftA";
            private readonly string sideliftGestureName_B = "gesture_sideliftB";
            private readonly string sideliftProgGestureName = "gesture_sideliftProgress";

        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /// <summary>
        /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        /// </summary>
        /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
        /// <param name="gestureResultView">GestureResultView object to store gesture results of a single body to</param>
        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView) {
            if ( kinectSensor == null ) {
                throw new ArgumentNullException("kinectSensor");
            }

            if ( gestureResultView == null ) {
                throw new ArgumentNullException("gestureResultView");
            }

            this.GestureResultView = gestureResultView;

            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if ( this.vgbFrameReader != null ) {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }

            // load the gesture from the gesture database

            using ( VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_sidelift) ) {

                foreach ( Gesture gesture in database.AvailableGestures ) {
                    if ( gesture.Name.Equals(this.sideliftGestureName) ) {
                        this.vgbFrameSource.AddGesture(gesture);
                        
                    }
                    if ( gesture.Name.Equals(this.sideliftGestureName_A) ) {
                        this.vgbFrameSource.AddGesture(gesture);

                    }
                    if ( gesture.Name.Equals(this.sideliftGestureName_B) ) {
                        this.vgbFrameSource.AddGesture(gesture);

                    }


                }
            }

            /**yes this for test**/
            using ( VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_forTest) ) {

                foreach ( Gesture gesture in database.AvailableGestures ) {
                    if ( gesture.Name.Equals(this.gestureForTest) ) {
                        this.vgbFrameSource.AddGesture(gesture);

                    }
                }
            }


            using ( VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_squat) ) {

                foreach ( Gesture gesture in database.AvailableGestures ) {
                    if ( gesture.Name.Equals(this.squatGestureName) ) {
                        this.vgbFrameSource.AddGesture(gesture);
                    }

                    if ( gesture.Name.Equals(this.squatProgGestureName) ) {
                        this.vgbFrameSource.AddGesture(gesture);
                    }
                }
            }
        }



        /// <summary> Gets the GestureResultView object which stores the detector results for display in the UI </summary>
        public GestureResultView GestureResultView { get; private set; }

        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId {
            get {
                return this.vgbFrameSource.TrackingId;
            }

            set {
                if ( this.vgbFrameSource.TrackingId != value ) {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused {
            get {
                return this.vgbFrameReader.IsPaused;
            }

            set {
                if ( this.vgbFrameReader.IsPaused != value ) {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        public bool GestureRecognized {
            get {
                return this.GestureRecognized;
            }

            set {
                if ( this.GestureRecognized != value ) {
                    this.GestureRecognized = value;
                }
            }
        }

        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing) {
            if ( disposing ) {
                if ( this.vgbFrameReader != null ) {
                    this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if ( this.vgbFrameSource != null ) {
                    this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        /// <summary>
        /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e) {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using ( VisualGestureBuilderFrame frame = frameReference.AcquireFrame() ) {
                if ( frame != null ) {
                    // get the discrete gesture results which arrived with the latest frame

                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                    IReadOnlyDictionary<Gesture, ContinuousGestureResult> continuousResults = frame.ContinuousGestureResults;

                    // we only have one gesture in this source object, but you can get multiple gestures
                    if ( discreteResults != null ) {
                        foreach ( Gesture gesture in this.vgbFrameSource.Gestures ) {
                            
                            if ( gesture.GestureType == GestureType.Discrete ) {

                                
                                //sidelift
                                if ( gesture.Name.Equals(this.sideliftGestureName) ) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if ( result != null ) {
                                        if ( result.Confidence > 0.2 )
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, DisplayTypes.showUpperSide, "sidelift"); }
                                }

                                if ( gesture.Name.Equals(this.sideliftGestureName_A) ) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if ( result != null ) {
                                        if(result.Confidence > 0.2)
                                        this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, DisplayTypes.showUpperSide, "sideliftA");
                                    }
                                }

                                if ( gesture.Name.Equals(this.sideliftGestureName_B) ) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    if ( result != null ) {
                                        if ( result.Confidence > 0.1 )
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, DisplayTypes.showUpperSide, "sideliftB");
                                    }
                                }
                                
                                //oh yes this is for test
                                if ( gesture.Name.Equals(this.gestureForTest) ) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    // update the GestureResultView object with new gesture result values
                                    if ( result != null ) {
                                        if ( result.Confidence > 0.2 )
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, DisplayTypes.showUpperSide, "sidelift"); }
                                }
                                
                            }
                        }
                    }
                    

                    if( continuousResults != null ) {

                        foreach ( Gesture gesture in this.vgbFrameSource.Gestures ) {
                            
                            //sidelift
                            /*
                            if ( gesture.Name.Equals(this.sideliftProgGestureName) && gesture.GestureType == GestureType.Continuous ) {
                                ContinuousGestureResult result = null;
                                continuousResults.TryGetValue(gesture, out result);


                                if ( result != null ) {
                                    var progress = result.Progress;
                                    if(progress > 0.5 && progress < 1 ) {
                                        this.GestureResultView.UpdateGestureResult(true, true, result.Progress);
                                    }
                                    else {
                                        this.GestureResultView.UpdateGestureResult(true, false, result.Progress);
                                    }
                                    //this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                                }
                            }*/
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Handles the TrackingIdLost event for the VisualGestureBuilderSource object
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e) {
            // update the GestureResultView object to show the 'Not Tracked' image in the UI
            this.GestureResultView.UpdateGestureResult(false, false, 0.0f,0,null);
        }
    }
}
