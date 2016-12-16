using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;

namespace FYP
{
    /// <summary>
    /// TestHarness class is a special version of Webcam class that contains a testing routine and outputs statistics about this testing
    /// </summary>
    public partial class TestHarness : Form
    {
        private Capture videoCap;  //Declare video capture variable
        private int fps = 0;  //Variable to count how many frames per second have been processed
        private Face mainFace;  //Declare mainFace as class global variable
        private Expression expression;  //Declare expression object
        private bool testing = false;  //Stores state of testing
        //Varables for holding test data
        private int framesPassed = 0;  //Stores number of frames tested so far
        private int faces = 0;  //Stores number of faces detected
        private int mouths = 0;  //Stores number of mouths
        private int reye = 0;
        private int leye = 0;
        private int reyebrow = 0;
        private int leyebrow = 0;
        DateTime testStartTime;  //Stores the test start time
        //Arrays store counters for frameexpressions for relevant test period
        // 0 - None; 1 - happy; 2 - surprise; 3 - angry; 4 - sad
        int[] neutral = new int[5];
        int[] smile = new int[5];
        int[] surprise = new int[5];
        int[] angry = new int[5];
        int[] sad = new int[5];
        //Strings store current expression value at end of test period
        string currentExpSmile;
        string currentExpNeutral;
        string currentExpSad;
        string currentExpSurprise;
        string currentExpAngry;
        
        //Stores last seen face location this is used to reduce the area to be searched for the face, reducing CPU time
        private Rectangle lastFaceLocation = new Rectangle(0,0,0,0);

        /// <summary>
        /// Constructor initialises all objects on the form and creates the webcam camera capture.
        /// </summary>
        public TestHarness()
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

                    //If testing is in progress, send face, and both current and frame expressions to testOutput method
                    if (testing)
                    {
                        testOutput(mainFace, expression.tempExpression, expression.CurrentExpression);
                    }
                    
                    //Writes the frame (with bounding boxes) to the Windows form
                    videoFeed.Image = nextFrame.Bitmap;
                    //Updates expression label with expression
                    expressionLabel.Text = expression.CurrentExpression;
                    
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

        /// <summary>
        /// Method compiles and outputs test report to a txt file.
        /// </summary>
        /// <param name="face">Current face detected</param>
        /// <param name="frameExpression">The current FRAME's expression</param>
        /// <param name="currentExpression">The current expression sent to the AIBO</param>
        private void testOutput(Face face, string frameExpression, string currentExpression)
        {
            if (framesPassed == 0)
            {
                //Starts the timer
                testStartTime = DateTime.Now;
            }
            
            //Add one to frames
            framesPassed++;

            //Count up stats for neutral expression periods
            if (testInstructions.Text == "Neutral")
            {
                updateArray(neutral, frameExpression);
            }
            else if (testInstructions.Text == "Smile")
            {
                updateArray(smile, frameExpression);
            }
            else if (testInstructions.Text == "Surprise")
            {
                updateArray(surprise, frameExpression);
            }
            else if (testInstructions.Text == "Sad")
            {
                updateArray(sad, frameExpression);
            }
            else if (testInstructions.Text == "Frown")
            {
                updateArray(angry, frameExpression);
            }
            
            //Count up general stats
            if (face.Location != Rectangle.Empty)
            {
                faces++;

                if (face.RightEye.Location != Rectangle.Empty)
                {
                    reye++;
                    if (face.RightEyeBrow.EyeBrowContour != null)
                    {
                        reyebrow++;
                    }
                }

                if (face.LeftEye.Location != Rectangle.Empty)
                {
                    leye++;
                    if (face.LeftEyeBrow.EyeBrowContour != null)
                    {
                        leyebrow++;
                    }
                }

                if (face.Mouth.LipContour != null)
                {
                    mouths++;
                }
            }


            //Change test period
            switch (framesPassed)
            {
                case 20: testInstructions.Text = "Smile";
                    currentExpSmile = currentExpression;
                    break;
                case 120: testInstructions.Text = "Neutral";
                    break;
                case 140: testInstructions.Text = "Frown";
                    currentExpAngry = currentExpression;
                    break;
                case 240: testInstructions.Text = "Neutral";
                    break;
                case 260: testInstructions.Text = "Sad";
                    currentExpSad = currentExpression;
                    break;
                case 360: testInstructions.Text = "Neutral";
                    break;
                case 380: testInstructions.Text = "Surprise";
                    currentExpSurprise = currentExpression;
                    break;
                case 480: testInstructions.Text = "Neutral";
                    break;
            }

            //Code to end test and write results to a file
            if (framesPassed == 500)
            {
                currentExpNeutral = currentExpression;

                //Code below writes results to a file
                //Creates new stream writer
                string path = Path.Combine(Application.StartupPath, @"Tests\" + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + @".txt");
                StreamWriter writer = new StreamWriter(path);
                double timeTaken = DateTime.Now.Subtract(testStartTime).TotalSeconds;
                writer.WriteLine("Time:" + timeTaken.ToString());  //Writes time taken for testing to complete to string
                writer.WriteLine("FPS:" + (500 / timeTaken).ToString());  //Calculates and writes FPS
                
                writer.WriteLine("Neutral:None:" + neutral[0].ToString());
                writer.WriteLine("Neutral:Smile:" + neutral[1].ToString());
                writer.WriteLine("Neutral:Surprised:" + neutral[2].ToString());
                writer.WriteLine("Neutral:Angry:" + neutral[3].ToString());
                writer.WriteLine("Neutral:Sad:" + neutral[4].ToString());
                writer.WriteLine("Neutral:CurrentExp:" + currentExpNeutral);

                writer.WriteLine("Smile:None:" + smile[0].ToString());
                writer.WriteLine("Smile:Smile:" + smile[1].ToString());
                writer.WriteLine("Smile:Surprised:" + smile[2].ToString());
                writer.WriteLine("Smile:Angry:" + smile[3].ToString());
                writer.WriteLine("Smile:Sad:" + smile[4].ToString());
                writer.WriteLine("Smile:CurrentExp:" + currentExpSmile);

                writer.WriteLine("Surprised:None:" + surprise[0].ToString());
                writer.WriteLine("Surprised:Smile:" + surprise[1].ToString());
                writer.WriteLine("Surprised:Surprised:" + surprise[2].ToString());
                writer.WriteLine("Surprised:Angry:" + surprise[3].ToString());
                writer.WriteLine("Surprised:Sad:" + surprise[4].ToString());
                writer.WriteLine("Surprised:CurrentExp:" + currentExpSurprise);

                writer.WriteLine("Angry:None:" + angry[0].ToString());
                writer.WriteLine("Angry:Smile:" + angry[1].ToString());
                writer.WriteLine("Angry:Surprised:" + angry[2].ToString());
                writer.WriteLine("Angry:Angry:" + angry[3].ToString());
                writer.WriteLine("Angry:Sad:" + angry[4].ToString());
                writer.WriteLine("Angry:CurrentExp:" + currentExpAngry);

                writer.WriteLine("Sad:None:" + sad[0].ToString());
                writer.WriteLine("Sad:Smile:" + sad[1].ToString());
                writer.WriteLine("Sad:Surprised:" + sad[2].ToString());
                writer.WriteLine("Sad:Angry:" + sad[3].ToString());
                writer.WriteLine("Sad:Sad:" + sad[4].ToString());
                writer.WriteLine("Sad:CurrentExp:" + currentExpSad);

                writer.WriteLine("Faces:Detected:" + faces.ToString());
                writer.WriteLine("Mouth:Detected:" + mouths.ToString());
                writer.WriteLine("RightEye:Detected:" + reye.ToString());
                writer.WriteLine("LeftEye:Detected:" + leye.ToString());
                writer.WriteLine("RightEyeBrow:Detected:" + reyebrow.ToString());
                writer.WriteLine("LeftEyeBrow:Detected:" + leyebrow.ToString());

                //Save file
                writer.Close();

                //reset Variables
                framesPassed = 0;
                testInstructions.Text = "Test Instructions";
                testing = false;
                testButton.Visible = true;
                faces = 0;
                mouths = 0;
                reye = 0;
                reyebrow = 0;
                leye = 0;
                leyebrow = 0;
                Array.Clear(smile, 0, 5);
                Array.Clear(neutral, 0, 5);
                Array.Clear(sad, 0, 5);
                Array.Clear(surprise, 0, 5);
                Array.Clear(angry, 0, 5);
            }

        }


        /// <summary>
        /// Updates test data arrays
        /// </summary>
        /// <param name="array">Array to update</param>
        /// <param name="frameExpression">frame expression</param>
        private void updateArray(int[] array, string frameExpression)
        {
            if (frameExpression == "none")
            {
                array[0]++;
            }
            else if (frameExpression == "happy")
            {
                array[1]++;
            }
            else if (frameExpression == "surprise")
            {
                array[2]++;
            }
            else if (frameExpression == "angry")
            {
                array[3]++;
            }
            else if (frameExpression == "sad")
            {
                array[4]++;
            }
        }

        /// <summary>
        /// Fires when test button is clicked; changes state of class variable to testing and hides button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testButton_Click(object sender, EventArgs e)
        {
            //Changes testing state
            testing = true;
            //Turns button off
            testButton.Visible = false;
            //Change label
            testInstructions.Text = "Neutral";
        }
    }

}
