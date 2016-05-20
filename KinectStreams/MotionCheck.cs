using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectStreams {
    class MotionCheck
    {
        private CameraSpacePoint[] joints = null;
        public void setJoints(CameraSpacePoint[] _joints){
            this.joints = _joints;
        }
        public CameraSpacePoint[] getJoints() {
            return this.joints;
        }

        #region count-fields

        private int countSidelift = 0;
        private int countSquat = 0;
        private int countShoulderpress = 0;
        private int countRow = 0;
        private int countLunge = 0;
        private int countFrontlift = 0;
        private int countDeadlift = 0;
        private int countBiceps_curl = 0;

        #endregion

        public int countGetter(string inputGesName) {
            switch (inputGesName) {
                case "sidelift":
                    return countSidelift;

                case "squat":
                    return countSquat;

                case "shoulderpress":
                    return countShoulderpress;

                case "row":
                    return countRow;

                case "lunge":
                    return countLunge;

                case "frontlift":
                    return countFrontlift;

                case "deadlift":
                    return countDeadlift;

                case "biceps_curl":
                    return countBiceps_curl;

                default:
                    return 0;
            }
        }

        public void countSetter(string inputGesName, int cNumber) {
            switch (inputGesName) {
                case "sidelift":
                    this.countSidelift = cNumber;
                    break;

                case "squat":
                    this.countSquat = cNumber;
                    break;

                case "shoulderpress":
                    this.countShoulderpress = cNumber;
                    break;

                case "row":
                    this.countRow = cNumber;
                    break;

                case "lunge":
                    this.countLunge = cNumber;
                    break;

                case "frontlift":
                    this.countFrontlift = cNumber;
                    break;

                case "deadlift":
                    this.countDeadlift = cNumber;
                    break;

                case "biceps_curl":
                    this.countBiceps_curl = cNumber;
                    break;

                default:
                    break;
            }

        }

        #region check-fields

        private bool[] checkSidelift = new bool[3] { false, false, false };
        private bool[] checkSquat = new bool[3] { false, false, false };
        private bool[] checkShoulderpress = new bool[3] { false, false, false };
        private bool[] checkRow = new bool[3] { false, false, false };
        private bool[] checkLunge = new bool[3] { false, false, false };
        private bool[] checkFrontlift = new bool[3] { false, false, false };
        private bool[] checkDeadlift = new bool[3] { false, false, false };
        private bool[] checkBiceps_curl = new bool[3] { false, false, false };

        private bool[] somethingWrong = new bool[3] { false, false, false };

        #endregion 

        public bool checkGetter(string inputGesName, int gKind) {
            switch (inputGesName) {
                case "sidelift":
                    return checkSidelift[gKind];

                case "squat":
                    return checkSquat[gKind];

                case "shoulderpress":
                    return checkShoulderpress[gKind];

                case "row":
                    return checkRow[gKind];

                case "lunge":
                    return checkLunge[gKind];

                case "frontlift":
                    return checkFrontlift[gKind];

                case "deadlift":
                    return checkDeadlift[gKind];

                case "biceps_curl":
                    return checkBiceps_curl[gKind];

                default:
                    return somethingWrong[gKind];
            }
        }

        public void checkSetter(string inputGesName, int gKind ,bool vBool) {
            switch (inputGesName) {
                case "sidelift":
                    this.checkSidelift[gKind] = vBool;
                    break;

                case "squat":
                    this.checkSquat[gKind] = vBool;
                    break;

                case "shoulderpress":
                    this.checkShoulderpress[gKind] = vBool;
                    break;

                case "row":
                    this.checkRow[gKind] = vBool;
                    break;

                case "lunge":
                    this.checkLunge[gKind] = vBool;
                    break;

                case "frontlift":
                    this.checkFrontlift[gKind] = vBool;
                    break;

                case "deadlift":
                    this.checkDeadlift[gKind] = vBool;
                    break;

                case "biceps_curl":
                    this.checkBiceps_curl[gKind] = vBool;
                    break;

                default:
                    this.somethingWrong[gKind] = vBool;
                    break;
            }
        }

        #region show-fields

        private int showInfoSidelift = GestureTypes.sidelift;
        private int showInfoSquat = GestureTypes.squat;
        private int showInfoShoulderpress = GestureTypes.shoulderpress;
        private int showInfoRow = GestureTypes.row;

        private int showInfoLunge = GestureTypes.lunge;
        private int showInfoFrontlift = GestureTypes.frontlift;
        private int showInfoDeadlift = GestureTypes.deadlift;
        private int showInfoBiceps_curl = GestureTypes.biceps_curl;

        private int showInfoSomethingWrong = GestureTypes.showDefault;

        #endregion

        public int showInfoGetter(string gestureKind) {
            switch (gestureKind) {
                case "sidelift":
                    return showInfoSidelift;

                case "squat":
                    return showInfoSquat;

                case "shoulderpress":
                    return showInfoShoulderpress;

                case "row":
                    return showInfoRow;

                case "lunge":
                    return showInfoLunge;

                case "frontlift":
                    return showInfoFrontlift;

                case "deadlift":
                    return showInfoDeadlift;

                case "biceps_curl":
                    return showInfoBiceps_curl;

                default:
                    return showInfoSomethingWrong;
            }
            
        }

        public bool checkMotionEnd = false;

        public void initializeChecker() {
            for (int i = 0; i < 3; i++) {
                this.checkSidelift[i] = false;
                this.checkSquat[i] = false;
                this.checkShoulderpress[i] = false;
                this.checkRow[i] = false;
                this.checkLunge[i] = false;
                this.checkFrontlift[i] = false;
                this.checkDeadlift[i] = false;
                this.checkBiceps_curl[i] = false;
            }
            
        }


    }
}
