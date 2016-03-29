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
    public static class _Gesture
    {
        static int flag = 0;
        static int flag_swipe_right = 1;
        static int flag_swipe_left = 1;
        static int color = 0;
        static int count = 0;
        public static void WaveHand(this Canvas canvas, Body body)
        {
 
            if (body.Joints[JointType.ElbowRight].Position.Y > body.Joints[JointType.ShoulderRight].Position.Y)
            {
                if (flag == 0) //직전에 촬영했던 프레임에서, 오른쪽 손이 오른쪽 엘보우보다 값이 클때(손이 엘보우보다 오른쪽) 
                {
                    // 이번에 찍은 프레임에서, 엘보우값이 핸드 값보다 크면(손이 엘보우보다 왼쪽) count++
                    if (body.Joints[JointType.ElbowRight].Position.X > body.Joints[JointType.HandRight].Position.X)
                    {
                        count++;
                        flag = 1;
                    }
                }
                else if (flag == 1) //직전에 촬영했던 프레임에서, 오른쪽 손이 오른쪽 엘보우보다 값이 작을때(손이 엘보우보다 왼쪽) 
                {
                    // 이번에 찍은 프레임에서, 엘보우값이 핸드 값보다 크면(손이 엘보우보다 왼쪽) count++
                    if (body.Joints[JointType.ElbowRight].Position.X < body.Joints[JointType.HandRight].Position.X)
                    {
                        count++;
                        flag = 0;
                    }
                }

                if (count == 4)
                {
                    //MessageBox.Show("WaveHand");
                    count = 0;
                }
            }
        }

        public static void Swipe(this Canvas canvas, Body body)
        {
            if (flag_swipe_right == 1)
            {
                if (body.Joints[JointType.HandTipRight].Position.X < body.Joints[JointType.ShoulderRight].Position.X)
                {
                    //MessageBox.Show("Swipe_right ");
                    color++;
                    if (color > 3) color = 0;
                    switch (color)
                    {
                        case 0:
                            canvas.Background = new SolidColorBrush(Colors.Black);
                            break;

                        case 1:
                            canvas.Background = new SolidColorBrush(Colors.White);
                            break;

                        case 2:
                            canvas.Background = new SolidColorBrush(Colors.Gray);
                            break;

                        case 3:
                            //canvas.Background = new SolidColorBrush(Colors.Transparent);
                            canvas.Background = new SolidColorBrush(Colors.Green);
                            break;
                    }
                    
                    flag_swipe_right = 0;
                }
            }
            else if (flag_swipe_right == 0)
            {
                if(body.Joints[JointType.HandTipRight].Position.X > body.Joints[JointType.ShoulderRight].Position.X)
                {
                    flag_swipe_right = 1;
                }
            }

            if (flag_swipe_left == 1)
            {
                if (body.Joints[JointType.HandTipLeft].Position.X > body.Joints[JointType.ShoulderLeft].Position.X)
                {
                    //MessageBox.Show("Swipe_left ");
                    color--;
                    if (color < 0) color = 3;
                    switch (color)
                    {
                        case 0:
                            canvas.Background = new SolidColorBrush(Colors.Black);
                            break;

                        case 1:
                            canvas.Background = new SolidColorBrush(Colors.White);
                            break;

                        case 2:
                            canvas.Background = new SolidColorBrush(Colors.Gray);
                            break;

                        case 3:
                            //canvas.Background = new SolidColorBrush(Colors.Transparent);
                            canvas.Background = new SolidColorBrush(Colors.Green);
                            break;
                    }
                    flag_swipe_left = 0;
                }
            }
            else if (flag_swipe_left == 0)
            {
                if (body.Joints[JointType.HandTipLeft].Position.X < body.Joints[JointType.ShoulderLeft].Position.X)
                {
                    flag_swipe_left = 1;
                }
            }

        }
    }
}
