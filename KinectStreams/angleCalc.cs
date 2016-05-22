using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Windows;

namespace KinectStreams
{

    public static class angleCalc
    {
        #region Calculation

        public static CameraSpacePoint MiddleJoint(CameraSpacePoint j1, CameraSpacePoint j2) {

            CameraSpacePoint midj;

            midj.X = (j1.X + j2.X) / 2;
            midj.Y = (j1.Y + j2.Y) / 2;
            midj.Z = (j1.Z + j2.Z) / 2;

            return midj;

        }


        private static double CalcP2PDistance( CameraSpacePoint joint1, CameraSpacePoint joint2){

            float _joint1PositionX = joint1.X;
            float _joint1PositionY = joint1.Y;
            float _joint1PositionZ = joint1.Z;

            float _joint2PositionX = joint2.X;
            float _joint2PositionY = joint2.Y;
            float _joint2PositionZ = joint2.Z;

            return (double)( Math.Pow(_joint1PositionX - _joint2PositionX, 2)
                     + Math.Pow(_joint1PositionY - _joint2PositionY, 2)
                     + Math.Pow(_joint1PositionZ - _joint2PositionZ, 2));

        }

        private static double CalcDistance(CameraSpacePoint joint1, CameraSpacePoint joint2) {

            float _joint1PositionX = joint1.X;
            float _joint1PositionY = joint1.Y;
            float _joint1PositionZ = joint1.Z;

            float _joint2PositionX = joint2.X;
            float _joint2PositionY = joint2.Y;
            float _joint2PositionZ = joint2.Z;

            return Math.Sqrt((Math.Pow(_joint1PositionX - _joint2PositionX, 2)
                            + Math.Pow(_joint1PositionY - _joint2PositionY, 2)
                            + Math.Pow(_joint1PositionZ - _joint2PositionZ, 2)));

        }


        private static double CalcDegreeA(CameraSpacePoint jointA/*drawPoint*/, CameraSpacePoint jointB/*otherP1*/, CameraSpacePoint jointC/*otherP2*/) {

            float _joint1PositionX = jointA.X;
            float _joint1PositionY = jointA.Y;
            float _joint1PositionZ = jointA.Z;

            float _joint2PositionX = jointB.X;
            float _joint2PositionY = jointB.Y;
            float _joint2PositionZ = jointB.Z;

            float _joint3PositionX = jointC.X;
            float _joint3PositionY = jointC.Y;
            float _joint3PositionZ = jointC.Z;

            double powedLengthAB = (Math.Pow(_joint1PositionX - _joint2PositionX, 2)
                                    + Math.Pow(_joint1PositionY - _joint2PositionY, 2)
                                    + Math.Pow(_joint1PositionZ - _joint2PositionZ, 2)); 
            double powedLengthBC = (Math.Pow(_joint2PositionX - _joint3PositionX, 2)
                                    + Math.Pow(_joint2PositionY - _joint3PositionY, 2)
                                    + Math.Pow(_joint2PositionZ - _joint3PositionZ, 2)); 
            double powedLengthCA = (Math.Pow(_joint3PositionX - _joint1PositionX, 2)
                                    + Math.Pow(_joint3PositionY - _joint1PositionY, 2)
                                    + Math.Pow(_joint3PositionZ - _joint1PositionZ, 2));

            double lengthAB = Math.Sqrt(powedLengthAB);
            double lengthCA = Math.Sqrt(powedLengthCA);

            return (Math.Acos((powedLengthCA + powedLengthAB - powedLengthBC)/ (2 * lengthAB * lengthCA)) * 180/Math.PI);
        }

        #endregion


        #region dgParts

        //1 Lelbow
        public static double dgLeftElbow(CameraSpacePoint[] filteredJoints) {
            return CalcDegreeA(filteredJoints[(int)JointType.ElbowLeft],filteredJoints[(int)JointType.ShoulderLeft],filteredJoints[(int)JointType.WristLeft]);
        }

        //2 Relbow
        public static double dgRightElbow(CameraSpacePoint[] filteredJoints) {
            return CalcDegreeA(filteredJoints[(int)JointType.ElbowRight],filteredJoints[(int)JointType.ShoulderRight],filteredJoints[(int)JointType.WristRight]);
        }

        //3 Lshoulder
        public static double dgLeftShoulder(CameraSpacePoint[] filteredJoints) {
            return CalcDegreeA(filteredJoints[(int)JointType.ShoulderLeft],filteredJoints[(int)JointType.ElbowLeft],filteredJoints[(int)JointType.HipLeft]);
        }

        //4 Rshoulder
        public static double dgRightShoulder(CameraSpacePoint[] filteredJoints) {
            return CalcDegreeA(filteredJoints[(int)JointType.ShoulderRight],filteredJoints[(int)JointType.ElbowRight],filteredJoints[(int)JointType.HipRight]);
        }
       
        //5 Lknee
        public static double dgLeftKnee(CameraSpacePoint[] filteredJoints) {
            return CalcDegreeA(filteredJoints[(int)JointType.KneeLeft], filteredJoints[(int)JointType.HipLeft], filteredJoints[(int)JointType.AnkleLeft]);
        }

        //6 Rknee
        public static double dgRightKnee(CameraSpacePoint[] filteredJoints) {
            return CalcDegreeA(filteredJoints[(int)JointType.KneeRight], filteredJoints[(int)JointType.HipRight], filteredJoints[(int)JointType.AnkleRight]);
        }

        // add
        public static double dgHip(CameraSpacePoint[] filteredjoints) {
            CameraSpacePoint midKnee = angleCalc.MiddleJoint(filteredjoints[(int)JointType.KneeLeft], filteredjoints[(int)JointType.KneeRight]);
            CameraSpacePoint midHip = angleCalc.MiddleJoint(filteredjoints[(int)JointType.HipLeft], filteredjoints[(int)JointType.HipRight]);
            CameraSpacePoint midShoulder = angleCalc.MiddleJoint(filteredjoints[(int)JointType.ShoulderLeft], filteredjoints[(int)JointType.ShoulderRight]);
            return CalcDegreeA(midHip, midKnee, midShoulder);
        }


        //사용 불가능.
        public static double dgSpine(CameraSpacePoint[] filteredjoints) {
            //CameraSpacePoint midKnee = angleCalc.MiddleJoint(filteredjoints[(int)JointType.KneeLeft], filteredjoints[(int)JointType.KneeRight]);
            CameraSpacePoint midHip = angleCalc.MiddleJoint(filteredjoints[(int)JointType.HipLeft], filteredjoints[(int)JointType.HipRight]);
            CameraSpacePoint midShoulder = angleCalc.MiddleJoint(filteredjoints[(int)JointType.ShoulderLeft], filteredjoints[(int)JointType.ShoulderRight]);
            return CalcDegreeA(filteredjoints[(int)JointType.SpineMid], midHip, midShoulder);
        }


        //7 distance of foot
        public static double dsFoot(CameraSpacePoint[] filteredJoints) {
            return CalcDistance(filteredJoints[(int)JointType.AnkleLeft],filteredJoints[(int)JointType.AnkleRight]);
        }

        //7-1 distance of shoulder
        public static double dsShoulder(CameraSpacePoint[] filteredJoints) {
            return CalcDistance(filteredJoints[(int)JointType.ShoulderLeft], filteredJoints[(int)JointType.ShoulderRight]);
        }




        
        //8 is Straight Neck
        public static bool isSraightNeck(CameraSpacePoint[] filteredJoints) {
            if ((CalcDegreeA(filteredJoints[(int)JointType.Neck],filteredJoints[(int)JointType.SpineShoulder],filteredJoints[(int)JointType.Head]) < 140.0)
                || (filteredJoints[(int)JointType.Neck].Y *1.1 < filteredJoints[(int)JointType.Head].Y) 
                || (filteredJoints[(int)JointType.Neck].Y *0.9 > filteredJoints[(int)JointType.Head].Y)) 
            {
                return false;
            }
            return true;
        }

        //9 is Straight Lwrist
        public static bool isSraightLeftWrist(CameraSpacePoint[] filteredJoints) {
            if (CalcDegreeA(filteredJoints[(int)JointType.WristLeft], filteredJoints[(int)JointType.HandLeft], filteredJoints[(int)JointType.ElbowLeft]) < 165.0) {
                return false;
            }
            return true;
        }

        //10 is Straight Rwrist
        public static bool isSraightRightWrist(CameraSpacePoint[] filteredJoints) {
            if (CalcDegreeA(filteredJoints[(int)JointType.WristRight], filteredJoints[(int)JointType.HandRight], filteredJoints[(int)JointType.ElbowRight]) < 165.0) {
                return false;
            }
            return true;
        }

        //11 is Straight Lankle
        public static bool isSraightLeftAnkle(CameraSpacePoint[] filteredJoints) {
            if (CalcDegreeA(filteredJoints[(int)JointType.AnkleLeft], filteredJoints[(int)JointType.KneeLeft], filteredJoints[(int)JointType.FootLeft]) < 165.0) {
                return false;
            }
            return true;
        }

        //12 is Straight Rankle
        public static bool isSraightRightAnkle(CameraSpacePoint[] filteredJoints) {
            if (CalcDegreeA(filteredJoints[(int)JointType.AnkleRight], filteredJoints[(int)JointType.KneeRight], filteredJoints[(int)JointType.FootRight]) < 165.0) {
                return false;
            }
            return true;
        }

        //13 is straight back
        public static bool isStraightBack(CameraSpacePoint[] filteredJoints) {
            if (CalcDegreeA(filteredJoints[(int)JointType.SpineMid], filteredJoints[(int)JointType.SpineBase], filteredJoints[(int)JointType.SpineShoulder]) < 150.0) {
                return false;
            }
            return true;
        }

        #endregion
    }
}
