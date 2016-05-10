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

        public int showInfoSidelift = DisplayTypes.showUpperSide;
        public int showInfoSquat = DisplayTypes.showAllSide;
        public int showInfoShoulderpress = DisplayTypes.showUpperSide;

        public bool checkMotionEnd = false;

        public void initializeChecker() {
            for (int i = 0; i < 3; i++) {
                checkSidelift[i] = false;
                checkSquat[i] = false;
                checkShoulderpress[i] = false;
            }
        }


    }
}
