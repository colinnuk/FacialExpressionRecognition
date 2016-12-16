using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;

namespace FYP
{
    class Face
    {
        //Initialises and loads HaarCascade objects
        private static HaarCascade faceHaar = new HaarCascade(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, @"Cascades\haarcascade_frontalface_alt.xml"));  //Initialises face Haar cascade
        private static HaarCascade rightEyeHaar = new HaarCascade(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, @"Cascades\haarcascade_righteye.xml"));  //Initialises right eye Haar cascade
        private static HaarCascade leftEyeHaar = new HaarCascade(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, @"Cascades\haarcascade_lefteye.xml"));  //Initialises left eye Haar cascade
        
        //Sets face region proportions: top 25% is forehead, middle 25% are eyes and eyebrows, bottom 50% is lower face
        private const float REGION_FOREHEAD = 0.25F;
        private const float REGION_EYES = 0.25F;
        private const float REGION_LOWER = 0.5F;

        #region Public Accessors for Properties
        //Public accessors and private backing stores
        private Rectangle _location;
        ///<summary>
        ///Location of the face as a Rectangle
        ///</summary>
        public Rectangle Location
        {
            get { return this._location; }
        }

        private Rectangle _faceRegion = new Rectangle(0,0,0,0);
        /// <summary>
        /// Current face region being used for detection
        /// </summary>
        public Rectangle FaceRegion
        {
            get { return this._faceRegion; }
        }

        //Properties for eyes, eyebrows and mouth
        private Eye _rightEye;
        ///<summary>
        ///The face's right eye
        ///</summary>
        public Eye RightEye
        {
            get { return this._rightEye; }
        }

        private Eye _leftEye;
        ///<summary>
        ///The face's left eye
        ///</summary>
        public Eye LeftEye
        {
            get { return this._leftEye; }
        }

        private EyeBrow _rightEyeBrow;
        ///<summary>
        ///The face's right eye brow
        ///</summary>
        public EyeBrow RightEyeBrow
        {
            get { return this._rightEyeBrow; }
        }

        private EyeBrow _leftEyeBrow;
        ///<summary>
        ///The face's left eye brow
        ///</summary>
        public EyeBrow LeftEyeBrow
        {
            get { return this._leftEyeBrow; }
        }

        private Mouth _mouth;
        ///<summary>
        ///The face's mouth
        ///</summary>
        public Mouth Mouth
        {
            get { return this._mouth; }
        }

        //Face segment Rectangle variables
        private Rectangle _foreheadRegion;
        private Rectangle _eyeRegion;
        private Rectangle _lowerFaceRegion;
        private Rectangle _leftEyeRegion;
        private Rectangle _rightEyeRegion;

        #region Face Segment Accessors
        public Rectangle ForeheadRegion
        {
            get { return this._foreheadRegion; }
        }

        public Rectangle EyeRegion
        {
            get { return this._eyeRegion; }
        }

        public Rectangle LowerFaceRegion
        {
            get { return this._lowerFaceRegion; }
        }

        public Rectangle LeftEyeRegion
        {
            get { return this._leftEyeRegion; }
        }

        public Rectangle RightEyeRegion
        {
            get { return this._rightEyeRegion; }
        }
        #endregion
        #endregion

        /// <summary>
        /// Detects faces and chooses largest face as the 'main face'
        /// </summary>
        /// <param name="frame">Grayscale frame that will be used for object detection</param>
        /// <param name="lastFaceLocation">Location of the last face</param>
        public void DetectFace(Image<Bgr, byte> frame, Rectangle lastFaceLocation)
        {

            //Code converts each frame from full colour image to a grayscale image for face
            // and some facial feature object detection
            Image<Gray, byte> grayFrame = frame.Convert<Gray, byte>();
            
            Rectangle lastFaceRegion = new Rectangle(0,0,0,0);
            if (lastFaceLocation.IsEmpty == false)
            {
                lastFaceRegion = lastFace(lastFaceLocation);
                grayFrame.ROI = lastFaceRegion;
            }

            //Detects all matching face Haar cascades in the face region and adds to faces array
            var faces = grayFrame.DetectHaarCascade(faceHaar, 1.3, 4,
                            HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                            new Size(120, 120))[0];
            //Sets Frame region of interest back to empty (so as not to break anything else needing the whole frame)
            grayFrame.ROI = Rectangle.Empty;

            //Checks if at least one face is present
            if (faces.Length > 0)
            {
                MCvAvgComp mainFace = faces[0];  //Initialises variable to store the main face
                //Finds the largest face from the detected array
                foreach (var face in faces)
                {
                    //faces are tested against the current biggest face; if bigger they replace it
                    if ((mainFace.rect.Height * mainFace.rect.Width) < (face.rect.Height * face.rect.Width))
                    {
                        mainFace = face;
                    }
                }

                //Crops 10% off each side of the face, then assigns global result to Location property of this class
                this._location = cropFace(mainFace.rect, lastFaceRegion);

                //Segments face and updates global variables with segmented regions
                segmentFace(_location);

                //Initialises facial feature objects and detection
                initialiseFeatures(grayFrame, frame, true);

            }
            else
            {
                //Turns location to empty as there is no face
                this._location.Width = 0;
                this._location.Height = 0;
                this._location.X = 0;
                this._location.Y = 0;
            }
        }

        /// <summary>
        /// This method cuts 20% off the width of the default Haar cascade as it is too wide.
        /// This method also converts location to the global frame location. 
        /// </summary>
        /// <param name="loc">The location rectangle of the current face</param>
        /// <param name="lastFaceRegion">Location of the last face</param>
        /// <returns>A new rectangle with 10% of width cropped off</returns>
        private Rectangle cropFace(Rectangle loc, Rectangle lastFaceRegion)
        {
            Rectangle slimLocation = loc;
            // Reduce the width by 20%
            slimLocation.Width = slimLocation.Width - (loc.Width / 5);
            // Shift the location 10% to the right
            slimLocation.X = slimLocation.X + (loc.Width / 10);

            //Turns the location into a global location
            Rectangle globalLoc = new Rectangle(slimLocation.X + lastFaceRegion.X,
                slimLocation.Y + lastFaceRegion.Y, slimLocation.Width, slimLocation.Height);
            return globalLoc;
        }

        /// <summary>
        /// Segments the face into different regions and updates global rectangle variables of each segment
        /// </summary>
        /// <param name="loc">Current face location</param>
        private void segmentFace(Rectangle loc)
        {
            _foreheadRegion = loc;
            _foreheadRegion.Height = (int) (loc.Height * REGION_FOREHEAD);

            _eyeRegion = loc;
            _eyeRegion.Y = _foreheadRegion.Bottom;
            _eyeRegion.Height = (int) (loc.Height * REGION_EYES);

            _lowerFaceRegion = loc;
            _lowerFaceRegion.Y = _eyeRegion.Bottom;
            _lowerFaceRegion.Height = (int) (loc.Height * REGION_LOWER);

            //Halves the eye region and splits between left and right
            int eyeWidth = _eyeRegion.Width / 2;

            _leftEyeRegion = _eyeRegion;
            _leftEyeRegion.Width = eyeWidth;
            
            _rightEyeRegion = _eyeRegion;
            _rightEyeRegion.X = _leftEyeRegion.Right;
            _rightEyeRegion.Width = eyeWidth;
        }

        /// <summary>
        /// Calculates the region of the last face. Region is location + 50 or 55 pixels around each side.
        /// Updates class variable _faceRegion.
        /// </summary>
        /// <param name="lastFaceLocation">The last face location.</param>
        /// <returns>A rectangle representing the region the last face was in</returns>
        private Rectangle lastFace(Rectangle lastFaceLocation)
        {
            Rectangle lastFaceRegion = new Rectangle(0,0,0,0);
            //Calculations for X axis; code ensures rectangle is not bigger than image edges
            if (lastFaceLocation.X < 56)
            {
                lastFaceRegion.X = 0;
                lastFaceRegion.Width = lastFaceLocation.Width + 110;
            }
            else if (lastFaceLocation.X + lastFaceLocation.Width + 55 > 640)
            {
                lastFaceRegion.Width = 640 - lastFaceLocation.X + 55;
                lastFaceRegion.X = lastFaceLocation.X - 55;
            }
            else
            {
                lastFaceRegion.X = lastFaceLocation.X - 55;
                lastFaceRegion.Width = lastFaceLocation.Width + 110;
            }

            //Calculations for Y axis; code ensures rectangle is not bigger than image edges
            if (lastFaceLocation.Y < 51)
            {
                lastFaceRegion.Y = 0;
                lastFaceRegion.Height = lastFaceLocation.Height + 100;
            }
            else if (lastFaceLocation.Y + lastFaceLocation.Height + 50 > 480)
            {
                lastFaceRegion.Height = 480 - lastFaceLocation.Y + 50;
                lastFaceRegion.Y = lastFaceLocation.Y - 50;
            }
            else
            {
                lastFaceRegion.Y = lastFaceLocation.Y - 50;
                lastFaceRegion.Height = lastFaceLocation.Height + 100;
            }

            //Updates class global variable with face region
            _faceRegion = lastFaceRegion;

            return lastFaceRegion;
        }


        /// <summary>
        /// Initialises facial feature objects; Eyes, eye brows and mouth
        /// </summary>
        /// <param name="grayFrame">Current frame to do detection on, in grayscale</param>
        /// <param name="frame">Current frame to do detection on, in colour</param>
        /// <param name="threading">True: enables threading</param>
        private void initialiseFeatures(Image<Gray, byte> grayFrame, Image<Bgr, byte> frame, bool threading)
        {
            //If threading true -> thread; else -> don't thread
            if (threading)
            {
                initialiseFeatures_Threaded(grayFrame, frame);
            }
            else
            {
                //Creates new Eye for RightEye
                this._rightEye = new Eye(grayFrame, rightEyeHaar, RightEyeRegion);
                this._rightEye.DetectEye();

                //Creates new Eye for LeftEye
                this._leftEye = new Eye(grayFrame, leftEyeHaar, LeftEyeRegion);
                this._leftEye.DetectEye();

                //Mouth
                this._mouth = new Mouth(frame, LowerFaceRegion);
                this._mouth.DetectMouth();

                //Right EyeBrow; uses eye position for likely position of eye brow
                Rectangle rightEyeBrowRect = new Rectangle(RightEyeRegion.X,
                    RightEyeRegion.Y - this._rightEye.Location.Height,
                    Convert.ToInt32(RightEyeRegion.Width * 0.95),
                    RightEyeRegion.Height);
                this._rightEyeBrow = new EyeBrow(frame, rightEyeBrowRect);
                this._rightEyeBrow.DetectEyeBrow();

                //Left EyeBrow
                Rectangle leftEyeBrowRect = new Rectangle(Convert.ToInt32(LeftEyeRegion.X * 1.03),
                    LeftEyeRegion.Y - this._leftEye.Location.Height,
                    Convert.ToInt32(LeftEyeRegion.Width * 0.9),
                    LeftEyeRegion.Height);
                this._leftEyeBrow = new EyeBrow(frame, leftEyeBrowRect);
                this._leftEyeBrow.DetectEyeBrow();
            }
        }

        /// <summary>
        /// THREADED: Initialises facial feature objects; Eyes, eye brows and mouth
        /// This method uses threads to increase performance by running feature detection
        /// simultaneously on several threads
        /// </summary>
        /// <param name="grayFrame">Current frame to do detection on, in grayscale</param>
        /// <param name="frame">Current frame to do detection on, in colour</param>
        private void initialiseFeatures_Threaded(Image<Gray, byte> grayFrame, Image<Bgr, byte> frame)
        {
            //Creates temporary objects and threads and starts threads for eyes and mouth
            //Eye position is used for eyebrow location so these threads must complete first
            //RightEye
            Eye tempRightEye = new Eye(grayFrame, rightEyeHaar, RightEyeRegion);
            Thread rightEyeThread = new Thread(tempRightEye.DetectEye);
            rightEyeThread.Start();
            //LeftEye
            Eye tempLeftEye = new Eye(grayFrame, leftEyeHaar, LeftEyeRegion);
            Thread leftEyeThread = new Thread(tempLeftEye.DetectEye);
            leftEyeThread.Start();
            //Mouth
            Mouth tempMouth = new Mouth(frame, LowerFaceRegion);
            Thread mouthThread = new Thread(tempMouth.DetectMouth);
            mouthThread.Start();

            //Waits for eye threads to finish and assigns objects
            rightEyeThread.Join();
            leftEyeThread.Join();
            this._rightEye = tempRightEye;
            this._leftEye = tempLeftEye;
            
            //Creates and starts threads for eyebrows; uses eye position for likely position of eye brow
            //Right EyeBrow
            Rectangle rightEyeBrowRect = new Rectangle(RightEyeRegion.X,
                RightEyeRegion.Y - Convert.ToInt32(this._rightEye.Location.Height * 1.1),
                Convert.ToInt32(RightEyeRegion.Width * 0.95),
                Convert.ToInt32(RightEyeRegion.Height * 0.95));
            EyeBrow tempRightEyeBrow = new EyeBrow(frame, rightEyeBrowRect);
            Thread rightEyeBrowThread = new Thread(tempRightEyeBrow.DetectEyeBrow);
            rightEyeBrowThread.Start();
            
            //Left EyeBrow
            Rectangle leftEyeBrowRect = new Rectangle(Convert.ToInt32(LeftEyeRegion.X * 1.03),
                LeftEyeRegion.Y - Convert.ToInt32(this._leftEye.Location.Height * 1.1),
                Convert.ToInt32(LeftEyeRegion.Width * 0.9),
                Convert.ToInt32(LeftEyeRegion.Height * 0.95));
            EyeBrow tempLeftEyeBrow = new EyeBrow(frame, leftEyeBrowRect);
            Thread leftEyeBrowThread = new Thread(tempLeftEyeBrow.DetectEyeBrow);
            leftEyeBrowThread.Start();
            
            //Waits for mouth and eyebrow threads to finish
            mouthThread.Join();
            rightEyeBrowThread.Join();
            leftEyeBrowThread.Join();
            
            //Assigns mouth and eyebrow objects
            this._mouth = tempMouth;
            this._rightEyeBrow = tempRightEyeBrow;
            this._leftEyeBrow = tempLeftEyeBrow;
        }
    }
}
