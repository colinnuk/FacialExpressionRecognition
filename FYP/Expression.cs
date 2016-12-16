using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace FYP
{
    class Expression
    {
        //Arrays to store last 40 positions (enough for about 2 seconds of data) of each of the 4 location points I am using from the eyebrows and mouth
        Point[] mouthLeft = new Point[40];
        Point[] mouthRight = new Point[40];
        Point[] mouthTop = new Point[40];
        Point[] mouthBottom = new Point[40];
        Point[] mouthMid = new Point[40];

        Point[] rightEyeBrowLeft = new Point[40];
        Point[] rightEyeBrowRight = new Point[40];
        Point[] rightEyeBrowMid = new Point[40];

        Point[] leftEyeBrowLeft = new Point[40];
        Point[] leftEyeBrowRight = new Point[40];
        Point[] leftEyeBrowMid = new Point[40];

        //Array to store last 40 locations of the face
        Rectangle[] faceLocation = new Rectangle[40];

        //Arrays to store last 40 locations of right and left eye
        Rectangle[] rightEye = new Rectangle[40];
        Rectangle[] leftEye = new Rectangle[40];

        //Array pointer int stores next available space for filling arrays; if it reaches 40 the pointer wraps round back to 0
        //This makes accessing the arrays a bit like a queue
        int arrayPointer = 0;

        //Stores temporary expression before it has been 'confirmed' and held for 20 frames
        public string tempExpression = "none";
        int tempExpCounter = 0;  //Stores counter to see how many consecutive frames of the same expression we have had

        //Stores int counter to count faceless frames
        int noFace = 0;

        //Stores expression
        string _expression;
        /// <summary>
        /// Current confirmed expression
        /// </summary>
        public string CurrentExpression
        {
            get { return _expression; }
        }

        //Object for AIBOConnection
        AIBOConnection AIBOConnection;

        //Temp debugging output
        public string debug;

        /// <summary>
        /// Constructor for Expression class. Initialises and shows the GUI for the AIBOConnection class
        /// </summary>
        public Expression()
        {
            //Initialises new AIBOConnection to specified IP address and port
            AIBOConnection = new AIBOConnection();
            AIBOConnection.Show();  //Displays the AIBOConnection GUI on screen
        }

        /// <summary>
        /// Updates all arrays with relevant Point information from relevant objects from the face.
        /// Modifies: arrayPointer, noFace
        /// </summary>
        /// <param name="face">The current face object (with other relevant objects) to use to calculate expression.</param>
        public void Update(Face face)
        {
            //Sets noFace back to 0
            noFace = 0;
            
            //Updates mouth positions
            mouthLeft[arrayPointer] = face.Mouth.Left;
            mouthRight[arrayPointer] = face.Mouth.Right;
            mouthTop[arrayPointer] = face.Mouth.Top;
            mouthBottom[arrayPointer] = face.Mouth.Bottom;
            mouthMid[arrayPointer] = face.Mouth.Mid;

            //Updates rightEyeBrow positions (converts to global positions)
            rightEyeBrowLeft[arrayPointer] = face.RightEyeBrow.Left;
            rightEyeBrowRight[arrayPointer] = face.RightEyeBrow.Right;
            rightEyeBrowMid[arrayPointer] = face.RightEyeBrow.Mid;

            //Updates leftEyeBrow positions (converts to global positions)
            leftEyeBrowLeft[arrayPointer] = face.LeftEyeBrow.Left;
            leftEyeBrowRight[arrayPointer] = face.LeftEyeBrow.Right;
            leftEyeBrowMid[arrayPointer] = face.LeftEyeBrow.Mid;

            //Updates face and eye positions
            faceLocation[arrayPointer] = face.Location;
            rightEye[arrayPointer] = face.RightEye.Location;
            leftEye[arrayPointer] = face.LeftEye.Location;

            //Increment array pointer (if reaches 40, start from 0)
            arrayPointer++;
            if (arrayPointer == 40)
            {
                arrayPointer = 0;
            }

            //Evaluate the new data
            evaluate();
        }

        /// <summary>
        /// Counts faceless frames; once 20 has been reached historical face data is cleared.
        /// Modifies: noFace
        /// </summary>
        public void NoFace()
        {
            noFace++;  //Increments counter

            if (noFace == 20)
            {
                Clear();  //Clears face as 20 faceless frames have elapsed
            }
        }

        /// <summary>
        /// Clears all arrays of historical data.
        /// </summary>
        public void Clear()
        {
            //Clears mouth arrays
            Array.Clear(mouthLeft, 0, 40);
            Array.Clear(mouthRight, 0, 40);
            Array.Clear(mouthTop, 0, 40);
            Array.Clear(mouthBottom, 0, 40);
            Array.Clear(mouthMid, 0, 40);
            //Clears right eye brow arrays
            Array.Clear(rightEyeBrowLeft, 0, 40);
            Array.Clear(rightEyeBrowRight, 0, 40);
            Array.Clear(rightEyeBrowMid, 0, 40);
            //Clears left eye brow arrays
            Array.Clear(leftEyeBrowLeft, 0, 40);
            Array.Clear(leftEyeBrowRight, 0, 40);
            Array.Clear(leftEyeBrowMid, 0, 40);

            //Clears face and eye locations
            Array.Clear(faceLocation, 0, 40);
            Array.Clear(leftEye, 0, 40);
            Array.Clear(rightEye, 0, 40);

            //Sets arrayPointer back to 0
            arrayPointer = 0;

            //Sets expression to 'none'
            tempExpression = "none";
            sendExpression();

            debug = "Clear";
        }

        /// <summary>
        /// Evaluates expression data and determines an expression, if an expression has been held for 20 frames (about a second),
        /// it is sent to the AIBO via updateExpression.
        /// Modifies: tempExpression
        /// </summary>
        private void evaluate()
        {
            //Variables for average data is held here
            //Variables for mouth characteristics
            //Relative values are needed as absolute pixel values will depend on how close the face is to the camera
            int pixelMouthWidth = 0;  //Stores the mean pixel mouth width
            int pixelMouthHeight = 0;  //Stores mean pixel mouth height
            float relativeMouthWidth = 0;  //Stores relative mouth width; compared to face width
            float relativeMouthHeight = 0;  //Stores relative mouth height; compared to face height
            int averageMouthLeftH = 0;  //Stores average height of left point
            int averageMouthRightH = 0;  //Stores average height of right point
            int averageMouthMidH = 0;  //Stores average height of middle point
            //Variables for eyebrow characteristics
            int pixelRightEyeBrowDistance = 0;  //Stores mean pixel eyebrow height
            int pixelLeftEyeBrowDistance = 0;  //Stores mean pixel eyebrow height
            int pixelEyeBrowGap = 0;  //Stores mean pixel gap between eyebrows
            float relativeRightEyeBrowDist = 0;  //Stores relative eyebrow height; compared to face height
            float relativeLeftEyeBrowDist = 0;  //Stores relative eyebrow height; compared to face height
            float relativeEyeBrowGap = 0;  //Stores relative eyebrow gap; compared to face width
            //Variables to store face characteristics
            int averageFaceWidth = 0;  //Stores average face width
            int averageFaceHeight = 0; //Stores average face height

            //Loops through arrays averaging the raw data
            for (int i = 0; i < 40; i++)
            {
                //Calculates the pixel mouth width, ignoring any operands which = 0
                //Updates the mouth left, right and mid averages
                if (mouthLeft[i].X != 0 && mouthRight[i].X != 0)
                {                    
                    pixelMouthWidth = (pixelMouthWidth + mouthRight[i].X - mouthLeft[i].X) / 2;
                    averageMouthLeftH = (averageMouthLeftH + mouthLeft[i].Y) / 2;
                    averageMouthRightH = (averageMouthRightH + mouthRight[i].Y) / 2;
                    averageMouthMidH = (averageMouthMidH + mouthMid[i].Y) / 2;
                }

                //Calculates the pixel mouth height, ignoring any operands which = 0
                if (mouthBottom[i].Y != 0 && mouthTop[i].Y != 0)
                {
                    pixelMouthHeight = (pixelMouthHeight + mouthBottom[i].Y - mouthTop[i].Y) / 2;
                }

                //Calculates distance between centre of right eye and right eyebrow
                if (rightEye[i].Y != 0 && rightEyeBrowMid[i].Y != 0)
                {
                    pixelRightEyeBrowDistance = (pixelRightEyeBrowDistance + rightEye[i].Y + (rightEye[i].Height / 2) - rightEyeBrowMid[i].Y) / 2;
                }

                //Calculates distance between centre of left eye and left eyebrow
                if (leftEye[i].Y != 0 && leftEyeBrowMid[i].Y != 0)
                {
                    pixelLeftEyeBrowDistance = (pixelLeftEyeBrowDistance + leftEye[i].Y + (leftEye[i].Height / 2) - leftEyeBrowMid[i].Y) / 2;
                }

                //Calculates distance between eyebrows
                if (leftEyeBrowRight[i].X != 0 && rightEyeBrowLeft[i].X != 0)
                {
                    pixelEyeBrowGap = (pixelEyeBrowGap + rightEyeBrowLeft[i].X - leftEyeBrowRight[i].X) / 2;
                }

                //Updates face variables
                averageFaceHeight = (averageFaceHeight + faceLocation[i].Height) / 2;
                averageFaceWidth = (averageFaceWidth + faceLocation[i].Width) / 2;
            }

            //Assigns the relative values by comparing the absolute pixel values to the face's dimensions
            //Relative values are needed as absolute pixel values will depend on how close the face is to the camera
            if (averageFaceHeight != 0)
            {
                relativeMouthHeight = (float)pixelMouthHeight / (float)averageFaceHeight;
                relativeRightEyeBrowDist = (float)pixelRightEyeBrowDistance / (float)averageFaceHeight;
                relativeLeftEyeBrowDist = (float)pixelLeftEyeBrowDistance / (float)averageFaceHeight;
            }

            if (averageFaceWidth != 0)
            {
                relativeMouthWidth = (float)pixelMouthWidth / (float)averageFaceWidth;
                relativeEyeBrowGap = (float)pixelEyeBrowGap / (float)averageFaceWidth;
            }

            //Sad expression (Left and right points BOTH need to be significantly lower than middle mouth point)
            if ((averageMouthMidH * 1.02) < averageMouthLeftH && (averageMouthMidH * 1.02) < averageMouthRightH && relativeMouthHeight < 0.23)
            {
                currentExpression("sad");
            }
            //Smile expression
            else if (relativeMouthWidth > 0.4 && relativeMouthHeight < 0.2)
            {
                currentExpression("happy");
            }
            //Surprise expression
            else if (relativeMouthWidth < 0.35 && relativeMouthHeight > 0.22)
            {
                currentExpression("surprise");
            }
            //Anger expression (a frown)
            else if (relativeEyeBrowGap < 0.2)
            {
                currentExpression("angry");
            }
            else
            {
                currentExpression("none");
            }

            debug = "Mouth Width: " + relativeMouthWidth.ToString("N2") + " Height: " + relativeMouthHeight.ToString("N2") + " R EyeBrow Dist: " + relativeRightEyeBrowDist.ToString("N2")
                + " L EyeBrow Dist: " + relativeLeftEyeBrowDist.ToString("N2") + " EyeBrow Gap: " + relativeEyeBrowGap.ToString("N2") + " TempExp:" + tempExpression + 
                "  AverageMouthMid: " + averageMouthMidH + " AverageMouthR: " + averageMouthRightH + " AverageMouthL: " + averageMouthLeftH;
        }

        /// <summary>
        /// Updates tempExpression variable, and checks to see if expression has been held for 20 frames before sending to AIBO
        /// using sendExpression() method. Avoids 'overloading' of AIBO control messages. Modifies: tempExpression, tempExpCounter
        /// </summary>
        /// <param name="exp">Expression</param>
        private void currentExpression(string exp)
        {
            //If passed expression is same as tempExpression, increment counter
            if (tempExpression == exp)
            {
                tempExpCounter++;
            }
            // if not, restart counter and reassign tempExpression to new expression
            else
            {
                tempExpCounter = 0;
                tempExpression = exp;
            }

            //If tempExpCounter reaches 20, send the expression to AIBO and update global expression property using sendExpression()
            if (tempExpCounter == 20)
            {
                sendExpression();
            }
        }

        /// <summary>
        /// Updates CurrentExpression class property and sends expression to AIBO if it has changed
        /// Modifies: _expression
        /// </summary>
        private void sendExpression()
        {
            //Checks to see if expression has changed; if so, updates _expression and sends command to AIBOConnection.QueueExpression
            if (_expression != tempExpression)
            {
                _expression = tempExpression;
                AIBOConnection.QueueExpression(_expression);
            }
        }
    }
}
