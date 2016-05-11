using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectStreams {
    class MotionCheck {

        public bool[] checkSidelift = new bool[3] { false, false, false };
        public bool[] checkSquat = new bool[3] { false, false, false };
        public bool[] checkShoulderpress = new bool[3] { false, false, false };
        public bool[] checkRow = new bool[3] { false, false, false };
        public bool[] checkLunge = new bool[3] { false, false, false };
        public bool[] checkFrontlift = new bool[3] { false, false, false };
        public bool[] checkDeadlift = new bool[3] { false, false, false };
        public bool[] checkBiceps_curl = new bool[3] { false, false, false };


        public int showInfoSidelift = DisplayTypes.showUpperSide;
        public int showInfoSquat = DisplayTypes.showAllSide;
        public int showInfoShoulderpress = DisplayTypes.showUpperSide;
        public int showInfoRow = DisplayTypes.showUpperSide;
        public int showInfoLunge = DisplayTypes.showDownerSide;
        public int showInfoFrontlift = DisplayTypes.showUpperSide;
        public int showInfoDeadlift = DisplayTypes.showAllSide;
        public int showInfoBiceps_curl = DisplayTypes.showUpperSide;

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
