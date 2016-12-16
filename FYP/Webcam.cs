using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;

namespace FYP
{
    /// <summary>
    /// Webcam class runs GUI, and contains main program loop (progTimer_Tick)
    /// </summary>
    public partial class Webcam : Form
    {
        private Capture videoCap;  //Declare video capture variable
        private int fps = 0;  //Variable to count how many frames per second have been processed
        private Face mainFace;  //Declare mainFace as class global variable
        private Expression expression;  //Declare expression object
        
        //Stores last seen face location this is used to reduce the area to be searched for the face, reducing CPU time
        private Rectangle lastFaceLocation = new Rectangle(0,0,0,0);

        /// <summary>
        /// Constructor initialises all objects on the form and creates the webcam camera capture.
        /// </summary>
        public Webcam()
        {            
            //Initialises Windows Forms components for the program's window
            InitializeComponent();
            //Creates new capture. Passing 0 gets default webcam
            videoCap = new Capture(0);
            //Sets the camera feed to use 640x480
            videoCap.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 640);
            videoCap.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 480);
            //Initialises Expression class (hence instantiating the AIBOConnection class)
            expression = new Expression();
        }

        /// <summary>
        /// Each time progTimer on the form 'ticks' (at an interval of 50ms; 20 times a second) 
        /// this method runs. Method calls face detection code in Faces class, then draws 
        /// bounding boxes to next frame and writes frame to imagebox.
        /// </summary>
        /// <param name="sender">Object that initiated event call to this method.</param>
        /// <param name="e">Event Arguments passed by sender object.</param>
        private void progTimer_Tick(object sender, EventArgs e)
        {
            using (Image<Bgr, byte> nextFrame = videoCap.QueryFrame())
            {
                //Check to make sure there is a next frame
                if (nextFrame != null)
                {
                    //Creates a clone of nextFrame that can be passed to next classes; so that they can be modified
                    // without affecting the principal webcam view
                    Image<Bgr, byte> tempFrame = nextFrame.Clone();

                    //Creates new Face object and calls detectFace to do face detection, passing the last location in
                    mainFace = new Face();
                    mainFace.DetectFace(tempFrame, lastFaceLocation);
                    
                    //Only attempts to draw facial features if a face has been found
                    if (mainFace.Location != Rectangle.Empty)
                    {
                        //Passes mainFace to the expression class so that expression can be evaluated
                        expression.Update(mainFace);

                        //Draws bounding boxes for face regions
                        nextFrame.Draw(mainFace.Location, new Bgr(Color.Black), 3);  //Main face bounding box
                        nextFrame.Draw(mainFace.RightEye.Location, new Bgr(Color.Yellow), 2);  //Right eye bounding box
                        nextFrame.Draw(mainFace.LeftEye.Location, new Bgr(Color.Yellow), 2);  //Left eye bounding box

                        //Draws the lip contour on the image, with an offset for the contour set as the last parameter
                        //Within a try-catch as there is a bug in EmguCV whereby a memory exception is sometimes observed; this will ignore it
                        if (mainFace.Mouth.LipContour != null)
                        {
                            try
                            {
                                nextFrame.Draw(mainFace.Mouth.LipContour, new Bgr(Color.GreenYellow), new Bgr(Color.Gray), 0, 2, mainFace.Mouth.Location.Location);
                            }
                            catch { }
                        }

                        //Draws the eyebrow contour on the image, with an offset for the contour set as the last parameter
                        //Within a try-catch as there is a bug in EmguCV whereby a memory exception is sometimes observed; this will ignore it
                        if (mainFace.RightEyeBrow.EyeBrowContour != null)
                        {
                            try
                            {
                                nextFrame.Draw(mainFace.RightEyeBrow.EyeBrowContour, new Bgr(Color.AntiqueWhite), new Bgr(Color.Gray), 0, 2, mainFace.RightEyeBrow.Location.Location);
                                nextFrame.Draw(mainFace.RightEyeBrow.EyeBrowLine[0], new Bgr(Color.Aquamarine), 2);
                                nextFrame.Draw(mainFace.RightEyeBrow.EyeBrowLine[1], new Bgr(Color.Red), 2);
                            }
                            catch { }
                        }

                        //Draws the eyebrow contour on the image, with an offset for the contour set as the last parameter
                        //Within a try-catch as there is a bug in EmguCV whereby a memory exception is sometimes observed; this will ignore it
                        if (mainFace.LeftEyeBrow.EyeBrowContour != null)
                        {
                            try
                            {
                                nextFrame.Draw(mainFace.LeftEyeBrow.EyeBrowContour, new Bgr(Color.AntiqueWhite), new Bgr(Color.Gray), 0, 2, mainFace.LeftEyeBrow.Location.Location);
                                nextFrame.Draw(mainFace.LeftEyeBrow.EyeBrowLine[0], new Bgr(Color.Aquamarine), 2);
                                nextFrame.Draw(mainFace.LeftEyeBrow.EyeBrowLine[1], new Bgr(Color.Red), 2);
                            }
                            catch { }
                        }

                    }
                    else
                    {
                        //If a face wasn't found, increment the expression class' no face counter (used to clear the expression history after 20 missed frames)
                        expression.NoFace();
                    }

                    //Updates expression label with expression
                    expressionLabel.Text = expression.CurrentExpression;

                    //Writes the frame (with bounding boxes) to the Windows form
                    videoFeed.Image = nextFrame.Bitmap;
                    fps++;  //Adds 1 to the fps count

                    //Updates lastFaceLocation with mainFace.Location
                    lastFaceLocation.Width = mainFace.Location.Width;
                    lastFaceLocation.Height = mainFace.Location.Height;
                    lastFaceLocation.X = mainFace.Location.X;
                    lastFaceLocation.Y = mainFace.Location.Y;
                }
            }
        }

        /// <summary>
        /// Ticks once a second to update fpsLabel with fps and reset the fps count.
        /// </summary>
        /// <param name="sender">Object that initiated event call to this method.</param>
        /// <param name="e">Event Arguments passed by sender object.</param>
        private void fpsTimer_Tick(object sender, EventArgs e)
        {
            fpsLabel.Text = fps.ToString();
            fps = 0;
        }
    }

}
