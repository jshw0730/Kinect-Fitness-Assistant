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

        #region DrawingCoord

/****/
        public static void DrawSkeletonCoord(this Canvas canvas, CameraSpacePoint[] filteredJoints)
        {
            //if (body == null) return;

            for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++)
            {
                canvas.DrawCoord(filteredJoints[(int)jt]);
            }
        }



/****/
        public static void DrawCoord(this Canvas canvas, CameraSpacePoint joint)
        {

            //if (joint.TrackingState == TrackingState.NotTracked) return;

            joint = joint.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);


            float _jointPositionX = joint.X;
            float _jointPositionY = joint.Y;
            float _jointPositionZ = joint.Z;

            TextBlock textBlockX = new TextBlock
            {
                FontSize = 20,
                Width = 100,
                Height = 100,
                Foreground = new SolidColorBrush(Colors.Red),
                Text = _jointPositionX.ToString()

            };


            TextBlock textBlockY = new TextBlock
            {
                FontSize = 20,
                Width = 100,
                Height = 60,
                Foreground = new SolidColorBrush(Colors.Green),
                Text = _jointPositionY.ToString()
            };

            TextBlock textBlockZ = new TextBlock
            {
                FontSize = 20,
                Width = 100,
                Height = 20,
                Foreground = new SolidColorBrush(Colors.Blue),
                Text = _jointPositionZ.ToString()
            };



            Canvas.SetLeft(textBlockX, joint.X - textBlockX.Width / 2);
            Canvas.SetTop(textBlockX, joint.Y - textBlockX.Height / 2);

            Canvas.SetLeft(textBlockY, joint.X - textBlockY.Width / 2);
            Canvas.SetTop(textBlockY, joint.Y - textBlockY.Height / 2);

            Canvas.SetLeft(textBlockZ, joint.X - textBlockZ.Width / 2);
            Canvas.SetTop(textBlockZ, joint.Y - textBlockZ.Height / 2);


            canvas.Children.Add(textBlockX);
            canvas.Children.Add(textBlockY);
            canvas.Children.Add(textBlockZ);
        }

        #endregion DrawingCoord


        #region Calculation

        public static double CalcP2PDistance( CameraSpacePoint joint1, CameraSpacePoint joint2){

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

        public static double CalcDegreeA(CameraSpacePoint jointA/*drawPoint*/, CameraSpacePoint jointB/*otherP1*/, CameraSpacePoint jointC/*otherP2*/) {

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

            return (double)(Math.Acos((powedLengthCA + powedLengthAB - powedLengthBC)/ (2 * lengthAB * lengthCA)) * 180/Math.PI);
        }

        #endregion

        #region Draw string method

        //first parameter for position, second parameter for value(double)
        public static void DrawString(this Canvas canvas, CameraSpacePoint displayPosJointA/*drawingpart*/, string string2draw, Color strColor)    {
                        
            //if (jointA.TrackingState == TrackingState.NotTracked || 
            //    jointB.TrackingState == TrackingState.NotTracked || 
            //    jointC.TrackingState == TrackingState.NotTracked) return;
                

           // float degreeA = CalcDegreeA(jointA, jointB, jointC);

            CameraSpacePoint joint;

            joint = displayPosJointA.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            TextBlock textBlock = new TextBlock {
                
                FontSize = 50,
                Width = 100,
                Height = 100,
                Foreground = new SolidColorBrush(strColor),
                FontWeight = FontWeights.UltraBold,

                Text = string2draw,
               
            };

            Canvas.SetLeft(textBlock, joint.X - textBlock.Width / 2);
            Canvas.SetTop(textBlock, joint.Y - textBlock.Height / 2);
            
            canvas.Children.Add(textBlock);
        }

        public static void DrawString(this Canvas canvas, CameraSpacePoint displayPosJointA/*drawingpart*/, double string2draw, Color strColor) {

            //if (displayPosJointA.TrackingState == TrackingState.NotTracked ) return;

            int String2draw = (int)string2draw;
            CameraSpacePoint joint;

            joint = displayPosJointA.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            TextBlock textBlock = new TextBlock {
                FontSize = 50,
                Width = 100,
                Height = 100,
                Foreground = new SolidColorBrush(strColor),
                                FontWeight = FontWeights.UltraBold,

                //Background = new SolidColorBrush(Colors.Black),
                Text = String2draw.ToString()

            };

            Canvas.SetLeft(textBlock, joint.X - textBlock.Width / 2);
            Canvas.SetTop(textBlock, joint.Y - textBlock.Height / 2);

            canvas.Children.Add(textBlock);
        }

        #endregion


        #region Parts
        //1,2 show elbow angle
        public static void DrawDegreeUpperBody(this Canvas canvas, CameraSpacePoint[] filteredJoints, Color strColor) {
            //if (filteredJoints == null) return;

            canvas.DrawString(
                            filteredJoints[(int)JointType.ElbowLeft],   CalcDegreeA(filteredJoints[(int)JointType.ElbowLeft], 
                                                                                    filteredJoints[(int)JointType.ShoulderLeft], 
                                                                                    filteredJoints[(int)JointType.WristLeft]),  strColor );
            canvas.DrawString(
                            filteredJoints[(int)JointType.ElbowRight], CalcDegreeA( filteredJoints[(int)JointType.ElbowRight], 
                                                                                    filteredJoints[(int)JointType.ShoulderRight], 
                                                                                    filteredJoints[(int)JointType.WristRight]), strColor );
        }


        //3,4 show shoulder angle
        public static void DrawShoulderDegree(this Canvas canvas, CameraSpacePoint[] filteredJoints, Color strColor) {
            // if (body == null) return;

            double result1 = CalcDegreeA(filteredJoints[(int)JointType.ShoulderLeft], 
                                        filteredJoints[(int)JointType.ElbowLeft], 
                                        filteredJoints[(int)JointType.SpineShoulder]
                                        );
            double result2 = CalcDegreeA(filteredJoints[(int)JointType.ShoulderRight], 
                                        filteredJoints[(int)JointType.ElbowRight], 
                                        filteredJoints[(int)JointType.SpineShoulder]
                                        );


            //왼손쪽 어꺠 표시
            if ( result1 - 90.0 <= 0 ) {    //0보다 클경우 그대로 표시
                canvas.DrawString(filteredJoints[(int)JointType.ShoulderLeft], result1 - 90.0,  strColor); //base angle is gonna be 90(degree)
            }
            else {                          //0보다 작으면 0으로 표시
                canvas.DrawString(filteredJoints[(int)JointType.ShoulderLeft], 0.0 ,            strColor); //base angle is gonna be 90(degree)
            }

            //오른쪽 어깨표시
            if ( result2 - 90.0 <= 0 ) {    //0보다 클경우 그대로 표시
                canvas.DrawString(filteredJoints[(int)JointType.ShoulderRight], result2 - 90.0, strColor);
            }
            else {                          //0보다 작으면 0으로 표시한다
                canvas.DrawString(filteredJoints[(int)JointType.ShoulderRight], 0.0,            strColor);
            }
        }
        
                
        //5,6 show knee angle
        public static void DrawDegreeDownerBody(this Canvas canvas, CameraSpacePoint[] filteredJoints, Color strColor) {
            // if (body == null) return;

            canvas.DrawString( filteredJoints[(int)JointType.KneeLeft],  CalcDegreeA(filteredJoints[(int)JointType.KneeLeft], 
                                                                                    filteredJoints[(int)JointType.HipLeft], 
                                                                                    filteredJoints[(int)JointType.AnkleLeft]), strColor);
            canvas.DrawString( filteredJoints[(int)JointType.KneeRight], CalcDegreeA( filteredJoints[(int)JointType.KneeRight], 
                                                                                    filteredJoints[(int)JointType.HipRight], 
                                                                                    filteredJoints[(int)JointType.AnkleRight]), strColor);
        }


        //7 width ankle to ankle
        public static void DrawLeg2LegWidth(this Canvas canvas, CameraSpacePoint[] filteredJoints, Color strColor) {
            // if (body == null) return;
            canvas.DrawString( filteredJoints[(int)JointType.SpineBase], CalcP2PDistance(filteredJoints[(int)JointType.AnkleLeft], 
                                                                                       filteredJoints[(int)JointType.AnkleRight]), strColor);
                                  

        }


        //8 is_straight Neck
        public static void DrawIsStraightNeck(this Canvas canvas, CameraSpacePoint[] filteredJoints, Color strColor) {
            if(CalcDegreeA(filteredJoints[(int)JointType.Neck],
                           filteredJoints[(int)JointType.SpineShoulder],
                           filteredJoints[(int)JointType.Head])            < 165.0 ) {
                //not straight
                canvas.DrawString(filteredJoints[(int)JointType.Neck], "not straight", strColor); 
            }
        }

        //9, 10 is_straight Hand
        public static void DrawIsStraightHand(this Canvas canvas, CameraSpacePoint[] filteredJoints, Color strColor) {
            if ( CalcDegreeA(filteredJoints[(int)JointType.WristLeft],
                             filteredJoints[(int)JointType.ElbowLeft],
                             filteredJoints[(int)JointType.HandLeft]) < 100.0 ) {
                //not straight
                canvas.DrawString(filteredJoints[(int)JointType.WristLeft], "not straight" , strColor);
            }

            if ( CalcDegreeA(filteredJoints[(int)JointType.WristRight],
                             filteredJoints[(int)JointType.ElbowRight],
                             filteredJoints[(int)JointType.HandRight]) < 100.0 ) {
                //not straight
                canvas.DrawString(filteredJoints[(int)JointType.WristRight], "not straight" , strColor);
            }
        }
  

        //11, 12 is_straight ankle
        public static void DrawIsStraightAnkle(this Canvas canvas, CameraSpacePoint[] filteredJoints, Color strColor) {

            double result1 = CalcDegreeA(filteredJoints[(int)JointType.AnkleLeft], 
                                        filteredJoints[(int)JointType.FootLeft], 
                                        filteredJoints[(int)JointType.KneeLeft]);
            double result2 = CalcDegreeA(filteredJoints[(int)JointType.AnkleRight], 
                                        filteredJoints[(int)JointType.FootRight], 
                                        filteredJoints[(int)JointType.KneeRight]);

            if ( (result1 > 120.0) || (result1 < 80.0) ) {
                canvas.DrawString(filteredJoints[(int)JointType.AnkleLeft], "not straight" , strColor); //it is not straight
            }

            if ( (result2 > 120.0) || (result2 < 80.0) ) {
                canvas.DrawString(filteredJoints[(int)JointType.AnkleRight], "not straight", strColor);//it is not straight
            }
        }

   
        //0 is_straight Spine
        public static void DrawIsStraightSpine(this Canvas canvas, CameraSpacePoint[] filteredJoints, Color strColor) {
            if ( CalcDegreeA(filteredJoints[(int)JointType.SpineMid],
                            filteredJoints[(int)JointType.SpineShoulder],
                            filteredJoints[(int)JointType.SpineBase]) < 160.0 ) {
                
                canvas.DrawString(filteredJoints[(int)JointType.SpineMid], "not straight", strColor); //not straight
            }
        }      

        #endregion DrawingDegree

    }
}
