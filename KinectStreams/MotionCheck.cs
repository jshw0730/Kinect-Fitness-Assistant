using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectStreams {
    class MotionCheck {

        public bool[] checkSidelift = new bool[3] { false, false, false };
        public int sideliftNumber = 2;

        public bool checkMotionEnd = false;

    }
}
