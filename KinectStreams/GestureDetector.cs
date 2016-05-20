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

        //gesture - sidelift
        private readonly string gestureDatabase_sidelift = @"Database\gesture_sidelift.gbd";
            private readonly string sideliftGestureName = "gesture_sideliftC";
            private readonly string sideliftGestureName_A = "gesture_sideliftA";
            private readonly string sideliftGestureName_B = "gesture_sideliftB";
        
        //gesture - squat
        private readonly string gestureDatabase_squat = @"Database\gesture_squat.gbd";
            private readonly string squatGestureName = "gesture_squatC";
            private readonly string squatGestureName_A = "squatA";
            private readonly string squatGestureName_B = "SquatB";
        //gesture - shoulderpress
        private readonly string gestureDatabase_shoulderpress = @"Database\gesture_shoulderpress.gbd";
            private readonly string shoulderpressGestureName = "gesture_shoulderpressC";
            private readonly string shoulderpressGestureName_A = "gesture_shoulderpressA";
            private readonly string shoulderpressGestureName_B = "gesture_shoulderpressB";
        

        //gesture - row
        private readonly string gestureDatabase_row = @"Database\gesture_row.gbd";
            private readonly string rowGestureName = "gesture_rowC";
            private readonly string rowGestureName_A = "gesture_rowA";
            private readonly string rowGestureName_B = "gesture_rowB";
        //gesture - lunge
        private readonly string gestureDatabase_lunge = @"Database\gesture_lunge.gbd";
            private readonly string lungeGestureName = "gesture_lungeC";
            private readonly string lungeGestureName_A = "gesture_lungeA";
            private readonly string lungeGestureName_B = "gesture_lungeB";
        //gesture - frontlift
        private readonly string gestureDatabase_frontlift = @"Database\gesture_frontlift.gbd";
            private readonly string frontliftGestureName = "gesture_frontliftC";
            private readonly string frontliftGestureName_A = "gesture_frontliftA";
            private readonly string frontliftGestureName_B = "gesture_frontliftB";
        //gesture - deadlift
        private readonly string gestureDatabase_deadlift = @"Database\gesture_deadlift.gbd";
            private readonly string deadliftGestureName = "gesture_deadliftC";
            private readonly string deadliftGestureName_A = "gesture_deadliftA";
            private readonly string deadliftGestureName_B = "gesture_deadliftB";
        //gesture - biceps_curl
        private readonly string gestureDatabase_biceps_curl = @"Database\gesture_biceps_curl.gbd";
            private readonly string biceps_curlGestureName = "gesture_biceps_curlC";
            private readonly string biceps_curlGestureName_A = "gesture_biceps_curlA";
            private readonly string biceps_curlGestureName_B = "gesture_biceps_curlB";
         
        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        private MultiSourceFrame frameReference = null;
        private MultiSourceFrameReader frameReader = null;

        /// <summary>
        /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        /// </summary>
        /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
        /// <param name="gestureResultView">GestureResultView object to store gesture results of a single body to</param>
        public GestureDetector(KinectSensor kinectSensor,  GestureResultView gestureResultView) {
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


            #region load the gesture from the gesture database
            
            using ( VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_sidelift) ) {
                foreach ( Gesture gesture in database.AvailableGestures ) {
                    if ( gesture.Name.Equals(this.sideliftGestureName) )   { this.vgbFrameSource.AddGesture(gesture); }
                    if ( gesture.Name.Equals(this.sideliftGestureName_A) ) { this.vgbFrameSource.AddGesture(gesture); }
                    if ( gesture.Name.Equals(this.sideliftGestureName_B) ) { this.vgbFrameSource.AddGesture(gesture); }
                }
            }
            

            using ( VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_squat) ) {
                foreach ( Gesture gesture in database.AvailableGestures ) {
                    if ( gesture.Name.Equals(this.squatGestureName) )   { this.vgbFrameSource.AddGesture(gesture); }
                    if ( gesture.Name.Equals(this.squatGestureName_A) ) { this.vgbFrameSource.AddGesture(gesture); }
                    if ( gesture.Name.Equals(this.squatGestureName_B) ) { this.vgbFrameSource.AddGesture(gesture); }
                }
            }

            /*
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_shoulderpress)) {
                foreach (Gesture gesture in database.AvailableGestures) {
                    if (gesture.Name.Equals(this.shoulderpressGestureName)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.shoulderpressGestureName_A)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.shoulderpressGestureName_B)) { this.vgbFrameSource.AddGesture(gesture); }
                }
            }
            
            
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_row)) {
                foreach (Gesture gesture in database.AvailableGestures) {
                    if (gesture.Name.Equals(this.rowGestureName)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.rowGestureName_A)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.rowGestureName_B)) { this.vgbFrameSource.AddGesture(gesture); }
                }
            }
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_lunge)) {
                foreach (Gesture gesture in database.AvailableGestures) {
                    if (gesture.Name.Equals(this.lungeGestureName)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.lungeGestureName_A)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.lungeGestureName_B)) { this.vgbFrameSource.AddGesture(gesture); }
                }
            }
            
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_frontlift)) {
                foreach (Gesture gesture in database.AvailableGestures) {
                    if (gesture.Name.Equals(this.frontliftGestureName)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.frontliftGestureName_A)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.frontliftGestureName_B)) { this.vgbFrameSource.AddGesture(gesture); }
                }
            }
             

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_deadlift)) {
                foreach (Gesture gesture in database.AvailableGestures) {
                    if (gesture.Name.Equals(this.deadliftGestureName)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.deadliftGestureName_A)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.deadliftGestureName_B)) { this.vgbFrameSource.AddGesture(gesture); }
                }
            }
            
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase_biceps_curl)) {
                foreach (Gesture gesture in database.AvailableGestures) {
                    if (gesture.Name.Equals(this.biceps_curlGestureName)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.biceps_curlGestureName_A)) { this.vgbFrameSource.AddGesture(gesture); }
                    if (gesture.Name.Equals(this.biceps_curlGestureName_B)) { this.vgbFrameSource.AddGesture(gesture); }
                }
            }
            */

            

            #endregion
            
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
            float vConfidence = 0.5f;

            using ( VisualGestureBuilderFrame frame = frameReference.AcquireFrame() ) {
                if ( frame != null ) {
                    // get the discrete gesture results which arrived with the latest frame

                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                    IReadOnlyDictionary<Gesture, ContinuousGestureResult> continuousResults = frame.ContinuousGestureResults;

                    // we only have one gesture in this source object, but you can get multiple gestures
                    if ( discreteResults != null ) {
                        foreach ( Gesture gesture in this.vgbFrameSource.Gestures ) {
                            
                            if ( gesture.GestureType == GestureType.Discrete ) {


                                #region sidelift
                                if ( gesture.Name.Equals(this.sideliftGestureName) ) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if ( result != null ) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureC, "sidelift"); 
                                    }
                                }

                                if ( gesture.Name.Equals(this.sideliftGestureName_A) ) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if ( result != null ) {
                                        if (result.Confidence > vConfidence)
                                        this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureA, "sidelift");
                                    }
                                }

                                if ( gesture.Name.Equals(this.sideliftGestureName_B) ) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    if ( result != null ) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureB, "sidelift");
                                    }
                                }
                                #endregion sidelift

                                #region squat
                                if (gesture.Name.Equals(this.squatGestureName)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureC, "squat");
                                    }
                                }

                                if (gesture.Name.Equals(this.squatGestureName_A)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureA, "squat");
                                    }
                                }

                                if (gesture.Name.Equals(this.squatGestureName_B)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureB, "squat");
                                    }
                                }
                                #endregion squat
                                
                                #region shoulderpress
                                if (gesture.Name.Equals(this.shoulderpressGestureName)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureC, "shoulderpress");
                                    }
                                }

                                if (gesture.Name.Equals(this.shoulderpressGestureName_A)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureA, "shoulderpress");
                                    }
                                }

                                if (gesture.Name.Equals(this.shoulderpressGestureName_B)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureB, "shoulderpress");
                                    }
                                }
                                #endregion 

                                #region row
                                if (gesture.Name.Equals(this.rowGestureName)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureC, "row");
                                    }
                                }

                                if (gesture.Name.Equals(this.rowGestureName_A)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureA, "row");
                                    }
                                }

                                if (gesture.Name.Equals(this.rowGestureName_B)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureB, "row");
                                    }
                                }
                                #endregion 

                                #region lunge
                                if (gesture.Name.Equals(this.lungeGestureName)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureC, "lunge");
                                    }
                                }

                                if (gesture.Name.Equals(this.lungeGestureName_A)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureA, "lunge");
                                    }
                                }

                                if (gesture.Name.Equals(this.lungeGestureName_B)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureB, "lunge");
                                    }
                                }
                                #endregion 

                                #region frontlift
                                if (gesture.Name.Equals(this.frontliftGestureName)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureC, "frontlift");
                                    }
                                }

                                if (gesture.Name.Equals(this.frontliftGestureName_A)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureA, "frontlift");
                                    }
                                }

                                if (gesture.Name.Equals(this.frontliftGestureName_B)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureB, "frontlift");
                                    }
                                }
                                #endregion 

                                #region deadlift
                                if (gesture.Name.Equals(this.deadliftGestureName)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureC, "deadlift");
                                    }
                                }

                                if (gesture.Name.Equals(this.deadliftGestureName_A)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureA, "deadlift");
                                    }
                                }

                                if (gesture.Name.Equals(this.deadliftGestureName_B)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureB, "deadlift");
                                    }
                                }
                                #endregion 

                                #region biceps_curl
                                if (gesture.Name.Equals(this.biceps_curlGestureName)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureC, "biceps_curl");
                                    }
                                }

                                if (gesture.Name.Equals(this.biceps_curlGestureName_A)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureA, "biceps_curl");
                                    }
                                }

                                if (gesture.Name.Equals(this.biceps_curlGestureName_B)) {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);
                                    if (result != null) {
                                        if (result.Confidence > vConfidence)
                                            this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence, gKind.gestureB, "biceps_curl");
                                    }
                                }
                                #endregion 

                            }
                        }
                    }
                    

                    if( continuousResults != null ) {

                        foreach ( Gesture gesture in this.vgbFrameSource.Gestures ) {                            
  
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
