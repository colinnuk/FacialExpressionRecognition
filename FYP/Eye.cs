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
    class Eye
    {
        //Private variables
        private Rectangle _location;  //Stores the local location
        private Image<Gray, byte> roiFrame;  //Set by constructor method; stores grayscale frame image of ROI
        private HaarCascade eyeHaar;  //Set by constructor method; stores haar cascade
        private Rectangle regionLocation;  //Set by constructor method; stores region location

        /// <summary>
        /// Returns the global location of the eye
        /// (Location of the eye in the whole frame)
        /// </summary>
        public Rectangle Location
        {
            get { return globalLocation(); }
        }

        /// <summary>
        /// Constructor for Eye class; assigns private variables for Eye class.
        /// Variables assigned are: roiFrame, eyeHaar, regionLocation.
        /// </summary>
        /// <param name="frame">Current grayscale frame to look for eye in</param>
        /// <param name="haar">Haar cascade to find relevant eye (right or left)</param>
        /// <param name="regionLoc">Location of the region of the eye</param>
        public Eye(Image<Gray, byte> frame, HaarCascade haar, Rectangle regionLoc)
        {
            //Must clone frame to prevent race conditions when multithreading
            roiFrame = frame.Clone();
            //Sets the frame's region of interest (ROI) to correct region and puts it in class variable
            roiFrame.ROI = regionLoc;
            //Stores region location
            regionLocation = regionLoc;
            //Stores Haar cascade
            eyeHaar = haar;
        }

        /// <summary>
        /// Detects the Eye in the given image using Haar Cascades. 
        /// Uses class variables: roiFrame, eyeHaar. 
        /// Modififes: _location.
        /// </summary>
        public void DetectEye()
        {
            //Detects all matching eye Haar cascades in the frame and adds to eyes array (note:
            // there should only be one eye)
            var eyes = roiFrame.DetectHaarCascade(eyeHaar, 1.3, 2,
                            HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                            new Size(18, 12))[0];

            //Checks to make sure at least one eye is found
            if (eyes.Length > 0)
            {
                MCvAvgComp mainEye = eyes[0];  //Variable stores eye
                foreach (var eye in eyes)
                {
                    //eyes are tested against the current biggest eye; if bigger they replace it
                    if ((mainEye.rect.Height * mainEye.rect.Width) < (eye.rect.Height * eye.rect.Width))
                    {
                        mainEye = eye;
                    }
                }

                this._location = mainEye.rect;  //Assigns mainEye to location of this object
            }
            else
            {
                //Turns location to empty as no eye was detected
                this._location.Width = 0;
                this._location.Height = 0;
                this._location.X = 0;
                this._location.Y = 0;
            }
        }

        /// <summary>
        /// Calculates and returns the global location of the eye. 
        /// Uses class variables: regionLocation, _location. 
        /// </summary>
        /// <returns>The global location of the eye</returns>
        private Rectangle globalLocation()
        {
            Rectangle globalLoc = new Rectangle(_location.X + regionLocation.X,
                _location.Y + regionLocation.Y, _location.Width, _location.Height);

            return globalLoc;
        }
        
    }
}
