using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KinectStreams
{
    public static class Extensions
    {
        #region Camera



        // RGB camera 
        public static ImageSource ToBitmap(this ColorFrame frame) {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            byte[] pixels = new byte[width * height * ((format.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra) {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        //depth image
        public static ImageSource ToBitmap(this DepthFrame frame) {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            ushort[] pixelData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(pixelData);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex) {
                ushort depth = pixelData[depthIndex];

                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red

                ++colorIndex;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }


        //infrared image
        public static ImageSource ToBitmap(this InfraredFrame frame) {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort[] frameData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(frameData);

            int colorIndex = 0;
            for (int infraredIndex = 0; infraredIndex < frameData.Length; infraredIndex++) {
                ushort ir = frameData[infraredIndex];

                byte intensity = (byte)(ir >> 7);

                pixels[colorIndex++] = (byte)(intensity / 1); // Blue
                pixels[colorIndex++] = (byte)(intensity / 1); // Green   
                pixels[colorIndex++] = (byte)(intensity / 0.4); // Red

                colorIndex++;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        #endregion

        #region Body


        public static CameraSpacePoint ScaleTo(this CameraSpacePoint joint, double width, double height, float skeletonMaxX, float skeletonMaxY)
        {

            joint.X = Scale(width, skeletonMaxX, joint.X);
            joint.Y = Scale(height, skeletonMaxY, -joint.Y);

            return joint;
        }

        public static CameraSpacePoint ScaleTo(this CameraSpacePoint joint, double width, double height)
        {
            return ScaleTo(joint, width, height, 1.0f, 1.0f);
        }




        private static float Scale(double maxPixel, double maxSkeleton, float position) {
            float value = (float)((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2));

            if (value > maxPixel) {
                return (float)maxPixel;
            }

            if (value < 0) {
                return 0;
            }

            return value;
        }

        #endregion

        #region Drawing



        public static void DrawSkeleton(this Canvas canvas, CameraSpacePoint[] filteredJoints, bool isViewer =false) {
            if (filteredJoints == null) return;
            if (isViewer) {

                canvas.DrawLine(filteredJoints[(int)JointType.Head], filteredJoints[(int)JointType.Neck], true);

                canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.Neck],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.ShoulderLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.ShoulderRight],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.SpineMid],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.ShoulderLeft], filteredJoints[(int)JointType.ElbowLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.ShoulderRight], filteredJoints[(int)JointType.ElbowRight],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.ElbowLeft], filteredJoints[(int)JointType.WristLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.ElbowRight], filteredJoints[(int)JointType.WristRight],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.WristLeft], filteredJoints[(int)JointType.HandLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.WristRight], filteredJoints[(int)JointType.HandRight],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.HandLeft], filteredJoints[(int)JointType.HandTipLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.HandRight], filteredJoints[(int)JointType.HandTipRight],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.HandLeft], filteredJoints[(int)JointType.ThumbLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.HandRight], filteredJoints[(int)JointType.ThumbRight],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.SpineMid], filteredJoints[(int)JointType.SpineBase], true);
                canvas.DrawLine(filteredJoints[(int)JointType.SpineBase], filteredJoints[(int)JointType.HipLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.SpineBase], filteredJoints[(int)JointType.HipRight],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.HipLeft], filteredJoints[(int)JointType.KneeLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.HipRight], filteredJoints[(int)JointType.KneeRight],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.KneeLeft], filteredJoints[(int)JointType.AnkleLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.KneeRight], filteredJoints[(int)JointType.AnkleRight],  true);

                canvas.DrawLine(filteredJoints[(int)JointType.AnkleLeft], filteredJoints[(int)JointType.FootLeft],  true);
                canvas.DrawLine(filteredJoints[(int)JointType.AnkleRight], filteredJoints[(int)JointType.FootRight],  true);

                for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++) {
                    if (jt == JointType.HandLeft || jt == JointType.HandRight || jt == JointType.HandTipLeft || jt == JointType.HandTipRight) {
                        canvas.DrawPoint(filteredJoints[(int)jt], 5, Colors.White, true);
                    }
                    else {
                        canvas.DrawPoint(filteredJoints[(int)jt], 5, Colors.White, true);
                    }
                }
            }

            else {

                canvas.DrawLine(filteredJoints[(int)JointType.Head], filteredJoints[(int)JointType.Neck]);

                canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.Neck]);

                canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.ShoulderLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.ShoulderRight]);
                canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.SpineMid]);

                canvas.DrawLine(filteredJoints[(int)JointType.ShoulderLeft], filteredJoints[(int)JointType.ElbowLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.ShoulderRight], filteredJoints[(int)JointType.ElbowRight]);

                canvas.DrawLine(filteredJoints[(int)JointType.ElbowLeft], filteredJoints[(int)JointType.WristLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.ElbowRight], filteredJoints[(int)JointType.WristRight]);

                canvas.DrawLine(filteredJoints[(int)JointType.WristLeft], filteredJoints[(int)JointType.HandLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.WristRight], filteredJoints[(int)JointType.HandRight]);

                canvas.DrawLine(filteredJoints[(int)JointType.HandLeft], filteredJoints[(int)JointType.HandTipLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.HandRight], filteredJoints[(int)JointType.HandTipRight]);

                canvas.DrawLine(filteredJoints[(int)JointType.HandLeft], filteredJoints[(int)JointType.ThumbLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.HandRight], filteredJoints[(int)JointType.ThumbRight]);

                canvas.DrawLine(filteredJoints[(int)JointType.SpineMid], filteredJoints[(int)JointType.SpineBase]);
                canvas.DrawLine(filteredJoints[(int)JointType.SpineBase], filteredJoints[(int)JointType.HipLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.SpineBase], filteredJoints[(int)JointType.HipRight]);

                canvas.DrawLine(filteredJoints[(int)JointType.HipLeft], filteredJoints[(int)JointType.KneeLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.HipRight], filteredJoints[(int)JointType.KneeRight]);

                canvas.DrawLine(filteredJoints[(int)JointType.KneeLeft], filteredJoints[(int)JointType.AnkleLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.KneeRight], filteredJoints[(int)JointType.AnkleRight]);

                canvas.DrawLine(filteredJoints[(int)JointType.AnkleLeft], filteredJoints[(int)JointType.FootLeft]);
                canvas.DrawLine(filteredJoints[(int)JointType.AnkleRight], filteredJoints[(int)JointType.FootRight]);

                for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++) {
                    if (jt == JointType.HandLeft || jt == JointType.HandRight || jt == JointType.HandTipLeft || jt == JointType.HandTipRight) {
                        canvas.DrawPoint(filteredJoints[(int)jt], 5, Colors.White, true);
                    }
                    else {
                        canvas.DrawPoint(filteredJoints[(int)jt], 5, Colors.White, true);
                    }
                }
            }
        }



        public static void DrawPoint(this Canvas canvas, CameraSpacePoint joint, int pSize ,Color pColor ,bool isViewer = false) {

            if (joint == null) return;

            if (isViewer) {
                joint = joint.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
            }

            

            Ellipse ellipse = new Ellipse {
                Width = pSize,
                Height = pSize,
                Fill = new SolidColorBrush(pColor)
            };

            Canvas.SetLeft(ellipse, joint.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, joint.Y - ellipse.Height / 2);


            canvas.Children.Add(ellipse);
        }

        public static void DrawLine(this Canvas canvas, CameraSpacePoint first, CameraSpacePoint second , bool isViewer = false) {

            if (first == null || second == null) return;

            if (isViewer) {
                first = first.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
                second = second.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
            }
            Line line = new Line {
                X1 = first.X,
                Y1 = first.Y,
                X2 = second.X,
                Y2 = second.Y,
                StrokeThickness = 10,
                Stroke = new SolidColorBrush(Colors.Red)
            };

            canvas.Children.Add(line);
        }



        #endregion

        /*
        #region colorSpcaePoints


        public static void DrawSkeleton(this Canvas canvas, ColorSpacePoint[] filteredJoints) {
            if (filteredJoints == null) return;

            for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++) {
                //if (filteredJoints[(int)jt].X != 0.0f || filteredJoints[(int)jt].Y != 0.0f || filteredJoints[(int)jt].Z != 0.0f)  {
                canvas.DrawPoint(filteredJoints[(int)jt]);
                //}
            }


            canvas.DrawLine(filteredJoints[(int)JointType.Head], filteredJoints[(int)JointType.Neck]);

            canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.Neck]);

            canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.ShoulderLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.ShoulderRight]);
            canvas.DrawLine(filteredJoints[(int)JointType.SpineShoulder], filteredJoints[(int)JointType.SpineMid]);

            canvas.DrawLine(filteredJoints[(int)JointType.ShoulderLeft], filteredJoints[(int)JointType.ElbowLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.ShoulderRight], filteredJoints[(int)JointType.ElbowRight]);

            canvas.DrawLine(filteredJoints[(int)JointType.ElbowLeft], filteredJoints[(int)JointType.WristLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.ElbowRight], filteredJoints[(int)JointType.WristRight]);

            canvas.DrawLine(filteredJoints[(int)JointType.WristLeft], filteredJoints[(int)JointType.HandLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.WristRight], filteredJoints[(int)JointType.HandRight]);

            canvas.DrawLine(filteredJoints[(int)JointType.HandLeft], filteredJoints[(int)JointType.HandTipLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.HandRight], filteredJoints[(int)JointType.HandTipRight]);

            canvas.DrawLine(filteredJoints[(int)JointType.HandLeft], filteredJoints[(int)JointType.ThumbLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.HandRight], filteredJoints[(int)JointType.ThumbRight]);

            canvas.DrawLine(filteredJoints[(int)JointType.SpineMid], filteredJoints[(int)JointType.SpineBase]);
            canvas.DrawLine(filteredJoints[(int)JointType.SpineBase], filteredJoints[(int)JointType.HipLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.SpineBase], filteredJoints[(int)JointType.HipRight]);

            canvas.DrawLine(filteredJoints[(int)JointType.HipLeft], filteredJoints[(int)JointType.KneeLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.HipRight], filteredJoints[(int)JointType.KneeRight]);

            canvas.DrawLine(filteredJoints[(int)JointType.KneeLeft], filteredJoints[(int)JointType.AnkleLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.KneeRight], filteredJoints[(int)JointType.AnkleRight]);

            canvas.DrawLine(filteredJoints[(int)JointType.AnkleLeft], filteredJoints[(int)JointType.FootLeft]);
            canvas.DrawLine(filteredJoints[(int)JointType.AnkleRight], filteredJoints[(int)JointType.FootRight]);
        }



        public static void DrawPoint(this Canvas canvas, ColorSpacePoint joint) {

            if (joint == null) return;

            //joint = joint.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);



            Ellipse ellipse = new Ellipse {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(Colors.LightBlue)
            };

            Canvas.SetLeft(ellipse, joint.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, joint.Y - ellipse.Height / 2);


            canvas.Children.Add(ellipse);
        }

        public static void DrawLine(this Canvas canvas, ColorSpacePoint first, ColorSpacePoint second) {

            if (first == null || second == null) return;

            first = first.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
            second = second.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            Line line = new Line {
                X1 = first.X,
                Y1 = first.Y,
                X2 = second.X,
                Y2 = second.Y,
                StrokeThickness = 20,
                Stroke = new SolidColorBrush(Colors.Red)
            };

            canvas.Children.Add(line);
        }
        #endregion

        */
    }
}
