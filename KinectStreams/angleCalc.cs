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




        #region DrawingDegree


        public static double CalcP2PDistancePow(CameraSpacePoint joint1, CameraSpacePoint joint2){

            float _joint1PositionX = joint1.X;
            float _joint1PositionY = joint1.Y;
            float _joint1PositionZ = joint1.Z;

            float _joint2PositionX = joint2.X;
            float _joint2PositionY = joint2.Y;
            float _joint2PositionZ = joint2.Z;

            return ( Math.Pow(_joint1PositionX - _joint2PositionX, 2)
                     + Math.Pow(_joint2PositionY - _joint2PositionY, 2)
                     + Math.Pow(_joint1PositionZ - _joint2PositionZ, 2));

        }

        public static float CalcDegreeA(CameraSpacePoint jointA, CameraSpacePoint jointB, CameraSpacePoint jointC) {

            double powedLengthAB = CalcP2PDistancePow(jointA, jointB);
            double powedLengthBC = CalcP2PDistancePow(jointB, jointC);
            double powedLengthCA = CalcP2PDistancePow(jointC, jointA);

            double lengthAB = Math.Sqrt(powedLengthAB);
            double lengthCA = Math.Sqrt(powedLengthCA);

            return (float)(Math.Acos((powedLengthCA + powedLengthAB - powedLengthBC) 
                                                            / (2 * lengthAB * lengthCA)) 
                                                                        * 180/Math.PI);
        }
        

        public static void DrawDegree(this Canvas canvas, CameraSpacePoint jointA, CameraSpacePoint jointB, CameraSpacePoint jointC)    {

            
            //if (jointA.TrackingState == TrackingState.NotTracked || 
            //    jointB.TrackingState == TrackingState.NotTracked || 
            //    jointC.TrackingState == TrackingState.NotTracked) return;
                

            float degreeA = CalcDegreeA(jointA, jointB, jointC);

            CameraSpacePoint joint;

            joint = jointA.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);


            TextBlock textBlock = new TextBlock
            {
                FontSize = 20,
                Width = 100,
                Height = 100,
                Foreground = new SolidColorBrush(Colors.Red),
                Text = degreeA.ToString()

            };


            Canvas.SetLeft(textBlock, joint.X - textBlock.Width / 2);
            Canvas.SetTop(textBlock, joint.Y - textBlock.Height / 2);


            canvas.Children.Add(textBlock);

        }


        public static void DrawDegreeOnBody(this Canvas canvas, CameraSpacePoint[] filteredJoints) {
           // if (body == null) return;
            
            canvas.DrawDegree(filteredJoints[(int)JointType.ElbowLeft], filteredJoints[(int)JointType.ShoulderLeft], filteredJoints[(int)JointType.WristLeft]);
            canvas.DrawDegree(filteredJoints[(int)JointType.ElbowRight], filteredJoints[(int)JointType.ShoulderRight], filteredJoints[(int)JointType.WristRight]);

        }
        
        #endregion DrawingDegree
    }
}
