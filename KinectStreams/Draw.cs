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
using System.Threading.Tasks;

namespace KinectStreams
{
    public static class Draw
    {
        #region DrawingCoord

        /****/
        public static void DrawSkeletonCoord(this Canvas canvas, CameraSpacePoint[] filteredJoints) {
            //if (body == null) return;

            for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++) {
                canvas.DrawCoord(filteredJoints[(int)jt]);
            }
        }



        /****/
        public static void DrawCoord(this Canvas canvas, CameraSpacePoint joint) {

            //if (joint.TrackingState == TrackingState.NotTracked) return;

            joint = joint.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);


            float _jointPositionX = joint.X;
            float _jointPositionY = joint.Y;
            float _jointPositionZ = joint.Z;

            TextBlock textBlockX = new TextBlock {
                FontSize = 20,
                Width = 100,
                Height = 100,
                Foreground = new SolidColorBrush(Colors.Red),
                Text = _jointPositionX.ToString()

            };


            TextBlock textBlockY = new TextBlock {
                FontSize = 20,
                Width = 100,
                Height = 60,
                Foreground = new SolidColorBrush(Colors.Green),
                Text = _jointPositionY.ToString()
            };

            TextBlock textBlockZ = new TextBlock {
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


        #region Draw string method

        //first parameter for position, second parameter for value(double)
        public static void DrawString(this Canvas canvas, CameraSpacePoint displayPosJointA/*drawingpart*/, string string2draw, Color strColor) {

            CameraSpacePoint joint;

            joint = displayPosJointA.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            TextBlock textBlock = new TextBlock {

                FontSize = 40,
                Width = 600,
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

            int String2draw = (int)string2draw;
            CameraSpacePoint joint;

            joint = displayPosJointA.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            TextBlock textBlock = new TextBlock {
                FontSize = 80,
                Width = 150,
                Height = 100,
                Foreground = new SolidColorBrush(strColor),
                FontWeight = FontWeights.UltraBold,
                Text = String2draw.ToString()

            };

            Canvas.SetLeft(textBlock, joint.X - textBlock.Width / 2);
            Canvas.SetTop(textBlock, joint.Y - textBlock.Height / 2);

            canvas.Children.Add(textBlock);
        }


        public static void DrawColoredAngle(Canvas canvas, CameraSpacePoint disPosJoint, 
            double inpAngle, double minAngle, double inAngle1, double inAngle2, double maxAngle  ) {
            byte gColor = 0;
            byte rColor = 0;

            double angle = inpAngle;    //inp angle
            if (angle > inAngle2 && maxAngle < 180) {       //inAngle2안정권2~maxAngle최대각도
                gColor = (byte)((angle / inAngle2) * 255);   //0~80, 0~1
                rColor = (byte)((1- angle / inAngle2) * 255);         //0~80, 1~0
            }
            else if (angle > minAngle && angle < inAngle1) { //minAngle최소각도 ~ inAnlge1안정권1
                gColor = (byte)(angle / inAngle1 * 255);          //0~80, 0~1
                rColor = (byte)((1 - angle / inAngle1) * 255);    //0~80, 1~0
            }
            else if (angle >= inAngle1 && angle <= inAngle2) { //inAngle1안정권1 ~ inAngle2안정권2
                rColor = (byte)0;
                gColor = (byte)255;
            }
            else {
                rColor = (byte)255;
                gColor = (byte)0;
            }
            Color showColor = System.Windows.Media.Color.FromRgb(rColor, gColor, 0);
            DrawString(canvas, disPosJoint, angle, showColor);

        }



        #endregion



        #region Part Infomations
        // 1. sidelift
        public static void DrawSideliftInfo(this Canvas canvas, CameraSpacePoint[] fJoints) {

            /*
             * * 1 사이드레터럴레이즈는 elbow의 각도가 150이하일 경우 표시해준다. 
             * 
             * * 2  겨드랑이 각도를 계속해서 표시해주되, 일정량 이상 올라갈 경우 빨강색이 된다. 
             * 0도 ~ 60도까진 노랑색, 이후 100도정도까지 서서히 녹색이 되다가, 120도가 넘으면 점점 노란색이 된다. 이후 180도는 빨강색
             * System.Windows.Media.Color.FromRgb(255, 0,0); 을 이용하여 색상을 조절한다.
             * 
             * * 3 허리가 곧은지 여부를 판단한다.
             * 
             * * 4 발과 발 사이가 어꺠넓이 이상일 경우 좀 이상하다.
             * 
             * * 5 목이 곧은가
             */

            // 1 팔꿈치
            if (angleCalc.dgLeftElbow(fJoints) < 130.0f) {
                DrawString(canvas, fJoints[(int)JointType.ElbowLeft], "팔꿈치펴라", Colors.Orange);
            }
            if (angleCalc.dgRightElbow(fJoints) < 130.0f) {
                DrawString(canvas, fJoints[(int)JointType.ElbowRight], "팔꿈치펴라", Colors.Orange);
            }


            // 2 겨드랑이
            DrawColoredAngle(canvas,fJoints[(int)JointType.ShoulderLeft],angleCalc.dgLeftShoulder(fJoints),0,80,100,180);
            DrawColoredAngle(canvas, fJoints[(int)JointType.ShoulderRight], angleCalc.dgRightShoulder(fJoints), 0, 80, 100, 180);

            // 3 허리가 곧은가
            if (angleCalc.isStraightBack(fJoints)) {
                DrawString(canvas, fJoints[(int)JointType.SpineMid], "허리펴라", Colors.Pink);
            }

            // 4 다리사이 거리
            if (angleCalc.dsFoot(fJoints) > angleCalc.dsShoulder(fJoints)) {                
                DrawString(canvas, angleCalc.MiddleJoint(fJoints[(int)JointType.FootLeft], fJoints[(int)JointType.FootRight]), "발좁혀라", Colors.Yellow);
            }

            // 5 목이 곧은가
            if (angleCalc.isSraightNeck(fJoints)) {
                DrawString(canvas, fJoints[(int)JointType.Neck], "목펴라", Colors.Red);
            }

        }

        // 2. squat
        public static void DrawSquatInfo(this Canvas canvas, CameraSpacePoint[] fJoints) {
            /*
             * * 발이 어꺠넓이로 벌려져있는지
             * * 무릎의 방향이 발목방향과 같은지, 무릎의 방향은 안쪽이면 안된다. 
             *      ==> 벡터 구현해서 yz평면에서의 방향을 구하면 가능할거같다.
             * 
             * * 내려오는 엉덩이의 깊이가 적절한지
             * 
             * * 항시 knee의 각도가 필요하다. 
             *  색상조절은 필요없을 듯
             */
            // 무릎
            DrawColoredAngle(canvas, fJoints[(int)JointType.KneeLeft], angleCalc.dgLeftKnee(fJoints), 0, 10, 120, 180);
            DrawColoredAngle(canvas, fJoints[(int)JointType.KneeRight], angleCalc.dgRightKnee(fJoints), 0, 10, 120, 180);
            
            // 발사이 거리
                   if (angleCalc.dsFoot(fJoints) < angleCalc.dsShoulder(fJoints)) {
                       DrawString(canvas, angleCalc.MiddleJoint(fJoints[(int)JointType.FootLeft], fJoints[(int)JointType.FootRight]), "더 벌려", Colors.Yellow);
            }
            
            // 엉덩이
           // DrawColoredAngle(canvas, fJoints)

        }

        // 3. shoulderpress
        public static void DrawShoulderpressInfo(this Canvas canvas, CameraSpacePoint[] fJoints) {
            /*
             * * 숄더프레스의 경우 elbow의 각도 항시 표시
             * * shoulder의 각도를 항시 표시
             *  이 둘은 노랑색 -> 초록색의 색상조절이 필요해보인다.
             */
            //elbow
            DrawColoredAngle(canvas, fJoints[(int)JointType.ElbowLeft], angleCalc.dgLeftElbow(fJoints),0,150,180,190);
            DrawColoredAngle(canvas, fJoints[(int)JointType.ElbowRight], angleCalc.dgRightElbow(fJoints), 0, 150, 180, 190);

            //shoulder
            DrawColoredAngle(canvas, fJoints[(int)JointType.ShoulderLeft],angleCalc.dgLeftShoulder(fJoints),0,100,180,200);
            DrawColoredAngle(canvas, fJoints[(int)JointType.ShoulderRight], angleCalc.dgRightShoulder(fJoints), 0, 100, 180, 200);
            
                
        }

        // 4. row
        public static void DrawRowInfo(this Canvas canvas, CameraSpacePoint[] fJoints) {
            /*
             * 
             */ 

        }

        // 5. lunge
        public static void DrawLungeInfo(this Canvas canvas, CameraSpacePoint[] fJoints) {
            /*
             * 
             */ 

        }

        // 6. frontlift
        public static void DrawFrontliftInfo(this Canvas canvas, CameraSpacePoint[] fJoints) {
            /*
             * * elbow 160~180
             * * shoulder 0~120
             * * 목, 허리
             */
            DrawColoredAngle(canvas, fJoints[(int)JointType.ElbowLeft], angleCalc.dgLeftElbow(fJoints), 90, 180, 180, 200);
            DrawColoredAngle(canvas, fJoints[(int)JointType.ElbowRight], angleCalc.dgRightElbow(fJoints), 90, 180, 180, 200);

            DrawColoredAngle(canvas, fJoints[(int)JointType.ShoulderLeft], angleCalc.dgLeftShoulder(fJoints), 0, 100, 120, 180);
            DrawColoredAngle(canvas, fJoints[(int)JointType.ShoulderRight], angleCalc.dgRightShoulder(fJoints), 0, 100, 120, 180);
        }

        // 7. deadlift
        public static void DrawDeadliftInfo(this Canvas canvas, CameraSpacePoint[] fJoints) {
            /*
             * 
             */

        }

        // 8. becips_curl
        public static void DrawBiceps_curlInfo(this Canvas canvas, CameraSpacePoint[] fJoints) {
            /*
             * 
             */

        }



        #endregion

    }
}
