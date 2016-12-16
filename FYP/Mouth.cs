using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;

namespace FYP
{
    class Mouth
    {
        //Private variables
        private Image<Bgr, byte> roiFrame;  //Set by constructor method; stores colour frame image of ROI
        private Rectangle regionLocation;  //Set by constructor method; stores mouth region location
        private Contour<Point> _lipContour;  //Set by the detectMouth() method; Stores the local location of the lip contour
        private Point _right;  //Stores right most point
        private Point _left;  //Stores left most point
        private Point _top;  //Stores top point
        private Point _bottom;  //Stores bottom point
        private Point _mid;  //Stores middle mouth point

        /// <summary>
        /// Returns the global location of the mouth
        /// (Location of the mouth in the whole frame)
        /// </summary>
        public Rectangle Location
        {
            get { return regionLocation; }
        }

        /// <summary>
        /// Returns the local location of the lip contour.
        /// Use with Location property to find the global location.
        /// </summary>
        public Contour<Point> LipContour
        {
            get { return _lipContour; }
        }
        
        /// <summary>
        /// Returns global location of right most point of mouth
        /// </summary>
        public Point Right
        {
            get { return _right; }
        }

        /// <summary>
        /// Returns global location of left most point of mouth
        /// </summary>
        public Point Left
        {
            get { return _left; }
        }

        /// <summary>
        /// Returns global location of top most point of mouth
        /// </summary>
        public Point Top
        {
            get { return _top; }
        }

        /// <summary>
        /// Returns global location of bottom point of mouth
        /// </summary>
        public Point Bottom
        {
            get { return _bottom; }
        }

        /// <summary>
        /// Returns global location of middle mout point
        /// </summary>
        public Point Mid
        {
            get { return _mid; }
        }

        //DEBUGGING
        public Image<Gray, byte> tempImage1;
        //public Image<Gray, byte> debugImage2;

        /// <summary>
        /// Constructor for mouth class; assigns private variables for mouth class.
        /// Variables assigned are: roiFrame, mouthHaar, regionLocation.
        /// </summary>
        /// <param name="frame">Current colour frame to look for mouth in</param>
        /// <param name="regionLoc">Location of the lower face region</param>
        public Mouth(Image<Bgr, byte> frame, Rectangle regionLoc)
        {
            //Code narrows down lower face area to the mouth
            regionLoc.Width = Convert.ToInt32(regionLoc.Width * 0.6);
            regionLoc.X = Convert.ToInt32(regionLoc.X + (regionLoc.Width / 0.6 * 0.2));
            regionLoc.Y = Convert.ToInt32(regionLoc.Y * 1.12);
            regionLoc.Height = Convert.ToInt32(regionLoc.Height * 0.7);

            //Must clone frame to prevent race conditions when multithreading
            roiFrame = frame.Clone();
            //Sets the frame's region of interest (ROI) to correct region and puts it in class variable
            roiFrame.ROI = regionLoc;
            //Stores region location
            regionLocation = regionLoc;
        }

        /// <summary>
        /// Detects the mouth in the given image using a colour transformation and then edge detection. 
        /// Uses class variables: roiFrame. 
        /// Modififes: _lipContour.
        /// </summary>
        public void DetectMouth()
        {
            //Performs colour transformation to accentuate the lips
            //roiFrame._EqualizeHist();
            Image<Gray, byte>[] channels = roiFrame.Split();
            channels[0] = channels[0] * 0.5;
            channels[1] = channels[1] * 2.0;
            Image<Gray, byte> mouthTransform = 200 * (channels[1] - channels[2] - channels[0]);
            mouthTransform = mouthTransform.Erode(1);
            tempImage1 = mouthTransform.Clone();  //DEBUGGING

            //Canny edge detection for the mouthTransform image
            mouthTransform = mouthTransform.Canny(new Gray(1355), new Gray(400));

            //Closing with a 3x3 element to connect up nearby contours
            mouthTransform._MorphologyEx(new StructuringElementEx(3, 3, 1, 1, CV_ELEMENT_SHAPE.CV_SHAPE_CROSS), CV_MORPH_OP.CV_MOP_CLOSE, 1);

            //debugImage2 = mouthTransform.Clone();   //DEBUGGING

            //Finds the contours then finds the longest contours, most likely to be the lips
            Contour<Point> contour1;
            Contour<Point> contour2;
            Contours.ValidContours(mouthTransform.FindContours(), regionLocation.Width, out contour1, out contour2);
            
            
            //Draws the 2 longest contours onto a new image
            // inside a try-catch as EmguCV has problems drawing contours from time to time
            Image<Gray, byte> largestContours = new Image<Gray,byte>(mouthTransform.Width, mouthTransform.Height);
            //Check for a contour before proceeding
            if (contour1 != null)
            {
                try { largestContours.Draw(contour1, new Gray(1500), new Gray(1500), 0, 2); }
                catch { }

                if (contour2 != null)
                {
                    try { largestContours.Draw(contour2, new Gray(1500), new Gray(1500), 0, 2); }
                    catch { }
                }

                //Canny detection for the new image
                largestContours = largestContours.Canny(new Gray(100), new Gray(100));
                //debugImage1 = largestContours.Clone();  //DEBUGGING

                //Only gets external valid contours (the outside of the lips rather than inside)
                Contour<Point> contour3;
                Contour<Point> contour4;
                Contours.ValidContours(largestContours.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_EXTERNAL), regionLocation.Width, out contour3, out contour4);
                _lipContour = contour3;
                
                //Extracts left, right, top, bottom, and middle top points from the contour 
                // (the higher the Y value the lower the point on the image; the origin is in the top left)
                Contours.ExtractPoints(_lipContour, out _left, out _right, out _bottom, out _top);

                //Code which finds the location of the halfway point of the lip, used for 'sad' expression 
                // (in this case the mid point of the lip is higher than the edges of the mouth to create the n shape)
                Image<Gray, byte> midLipImg = new Image<Gray, byte>(mouthTransform.Width, mouthTransform.Height);
                try { midLipImg.Draw(_lipContour, new Gray(1500), new Gray(1500), 0, 1); }
                catch { }

                //debugImage1 = midLipImg.Clone();  //DEBUGGING

                if (midLipImg.Width != 0 && midLipImg.Height != 0)
                {
                    //Gets the halfway point of the mouth
                    int midXLine = (_left.X + _right.X) / 2;
                    int[] whitePixels = new int[2];  //Array to store returned pixels; should be no more than 2 pixels in this, one for top edge, one for bottom
                    int c = 0;  //Array counting variable
                    //Iterates through each pixel on this column returning the row values of any white pixels
                    for (int i = 0; i < midLipImg.Rows; i++)
                    {
                        //If the pixel is above a threshold (not black) add it's row number to the array
                        if (midLipImg[i, midXLine].Intensity > 0.0)
                        {
                            whitePixels[c] = i;
                            c++;
                            //Check to ensure no buffer overruns on array size; breaks from loop if array is full
                            if (c == 2)
                            {
                                break;
                            }
                        }
                    }

                    //Assigns value to _mid
                    _mid.X = midXLine;
                    _mid.Y = (whitePixels[0] + whitePixels[1]) / 2;
                }

                //Adds global offset to points
                _left.Offset(regionLocation.Location);
                _bottom.Offset(regionLocation.Location);
                _right.Offset(regionLocation.Location);
                _top.Offset(regionLocation.Location);
                _mid.Offset(regionLocation.Location);
            }
            else
            {
                //Set mouth locations to empty
                _bottom = Point.Empty;
                _left = Point.Empty;
                _mid = Point.Empty;
                _right = Point.Empty;
                _top = Point.Empty;
            }
        }
    }
}
