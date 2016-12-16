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
    class EyeBrow
    {
        //Private variables
        private Image<Bgr, byte> roiFrame;  //Set by constructor method; stores colour frame image of ROI
        private Rectangle regionLocation;  //Set by constructor method; stores region location
        private Contour<Point> _eyeBrowContour;  //Set by the detectEyeBrow() method; stores local location of eye brow
        private Point _right;  //Stores right most point
        private Point _left;  //Stores left most point
        private Point _mid;  //Stores mid point

        /// <summary>
        /// Returns the global region location of the eyebrow
        /// (Location of the eyebrow in the whole frame)
        /// </summary>
        public Rectangle Location
        {
            get { return regionLocation; }
        }

        /// <summary>
        /// Returns the local location of the lip contour
        /// Use with Location property to find the global location
        /// </summary>
        public Contour<Point> EyeBrowContour
        {
            get { return _eyeBrowContour; }
        }

        /// <summary>
        /// Returns global location of right most point of eye brow
        /// </summary>
        public Point Right
        {
            get { return _right; }
        }

        /// <summary>
        /// Returns global location of left most point of eye brow
        /// </summary>
        public Point Left
        {
            get { return _left; }
        }

        /// <summary>
        /// Returns global location of mid point of eye brow
        /// </summary>
        public Point Mid
        {
            get { return _mid; }
        }

        /// <summary>
        /// Returns a two element array of LineSegments connecting the left, middle and right of the eye brow
        /// </summary>
        public LineSegment2D[] EyeBrowLine
        {
            get { return lineGenerator(); }
        }

        /// <summary>
        /// Constructor for eye brow class; assigns private variables for eyebrow class.
        /// Variables assigned are: roiFrame, regionLocation.
        /// </summary>
        /// <param name="frame">Current colour frame to look for eye brow in</param>
        /// <param name="regionLoc">Location of the region of the eye brow</param>
        public EyeBrow(Image<Bgr, byte> frame, Rectangle regionLoc)
        {
            //Must clone frame to prevent race conditions when multithreading
            roiFrame = frame.Clone();
            //Sets the frame's region of interest (ROI) to correct region and puts it in class variable
            roiFrame.ROI = regionLoc;
            //Stores region location
            regionLocation = regionLoc;
        }

        /// <summary>
        /// Detects the eye brow in the given image using edge detection. 
        /// Uses class variables: roiFrame, regionLocation.
        /// Modififes: _eyeBrowContour.
        /// </summary>
        public void DetectEyeBrow()
        {
            //Some noise reduction
            Image<Gray, byte> grayFrame = roiFrame.Convert<Gray, byte>();
            grayFrame = grayFrame.PyrUp().PyrDown();
            grayFrame = grayFrame.Erode(3).AbsDiff(grayFrame.Dilate(3));
            grayFrame._ThresholdBinary(new Gray(40), new Gray(255));
            
            //Canny edge detection
            Image<Gray, byte> edgeFrame = grayFrame.Canny(new Gray(100), new Gray(30));

            //Find contours
            Contour<Point> contour1;
            Contour<Point> contour2;
            Contours.ValidContours(edgeFrame.FindContours(), regionLocation.Width, out contour1, out contour2);
            _eyeBrowContour = contour1;

            //Ensures contour is present
            if (_eyeBrowContour != null)
            {
                //Extracts left, right and mid points from the contour
                Contours.ExtractPoints(_eyeBrowContour, out _left, out _right, out _mid);

                //Adds global offset to points
                _left.Offset(regionLocation.Location);
                _mid.Offset(regionLocation.Location);
                _right.Offset(regionLocation.Location);
            }
            else
            {
                _left = Point.Empty;
                _mid = Point.Empty;
                _right = Point.Empty;
            }
        }

        /// <summary>
        /// Generates a two element array that contains LineSegments to connect the left, middle and right of the eye brow
        /// </summary>
        /// <returns>[0] - Left to Middle Line; [1] - Middle to Right Line</returns>
        private LineSegment2D[] lineGenerator()
        {
            //Creates the return array
            LineSegment2D[] array = new LineSegment2D[2];

            //Creates the new line segments and puts them into the array
            array[0] = new LineSegment2D(this._left, this._mid);
            array[1] = new LineSegment2D(this._mid, this._right);

            return array;
        }
    }
}
