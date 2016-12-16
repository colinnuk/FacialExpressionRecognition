using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FYP
{
    /// <summary>
    /// Class to handle the connection with the AIBO
    /// </summary>
    public partial class AIBOConnection : Form
    {
        //Declares TelnetConnection for AIBO; public so that other methods can access properties
        TelnetConnection Connection;

        //Defines each of the AIBO response programs (happy (x2), surprise, angry (x2), sad)
        #region AIBOPrograms

        private string happy = "EDIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_JOY.S_SOUNDLED.1\r\n" +
            "PLAY:ACTION:STAND\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:PALONE.AUTO.TAILH\r\n" +
            "WAIT:5000\r\n" +
            "PLAY:ACTION:PALONE.AUTO.TAILSTOP\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_JOY.S_SOUNDLED.5\r\n" +
            "PLAY:ACTION:SIT\r\n" +
            "END\r\n" +
            "RUN\r\n";

        private string happy2 = "EDIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_JOY.S_SOUNDLED.2\r\n" +
            "PLAY:ACTION:STAND\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:L_PALONE.M_DANCE.S_SPDANCE.stand\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:SIT\r\n" +
            "END\r\n" +
            "RUN\r\n";

        private string surprise = "EDIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_SURP.S_SOUNDLED.1\r\n" +
            "PLAY:ACTION:STAND\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:TURN:180\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:WALK:0:100\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:TURN:180\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_SURP.S_SOUNDLED.4\r\n" +
            "WAIT:500\r\n" +
            "PLAY:ACTION:SIT\r\n" +
            "END\r\n" +
            "RUN\r\n";

        private string angry = "EDIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_ANGRY.S_SOUNDLED.1\r\n" +
            "PLAY:ACTION:STAND\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:WALK:0:100\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:L_FDSP.M_DPLEAS.S_DPLEAS.stand\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:L_FDSP.M_COMFORT.S_BARK.stand\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:SIT\r\n" +
            "END\r\n" +
            "RUN\r\n";

        private string angry2 = "EDIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_FEAR.S_SOUNDLED.1\r\n" +
            "PLAY:ACTION:STAND\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:TURN:180\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:WALK:0:100\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:TURN:180\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:LIE\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_FEAR.S_SOUNDLED.5\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:SIT\r\n" +
            "END\r\n" +
            "RUN\r\n";

        private string sad = "EDIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_SAD.S_SOUNDLED.1\r\n" +
            "PLAY:ACTION:STAND\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:WALK:0:100\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:L_FDSP.M_DEPRES.S_DOWN\r\n" +
            "WAIT\r\n" +
            "PLAY:ACTION:L_EDSP.M_SAD.S_SOUNDLED.2\r\n" +
            "PLAY:ACTION:SIT\r\n" +
            "END\r\n" +
            "RUN\r\n";

        #endregion

        /// <summary>
        /// Constructor for AIBOConnection class, initialises Windows forms components 
        ///  and attempts connection to AIBO by calling connect() method
        /// </summary>
        public AIBOConnection()
        {
            //Initialises Windows Forms components for the program's window
            InitializeComponent();
            //Attempts connection
            connect();
        }

        /// <summary>
        /// Public method to queue the expressions being sent from Expression class.
        /// Calls the background worker thread so telnet commands can be sent to AIBO without
        /// slowing down UI thread.
        /// </summary>
        /// <param name="expression">Expression to queue</param>
        public void QueueExpression(string expression)
        {
            backgroundWorker.RunWorkerAsync(expression);
        }

        /// <summary>
        /// Checks if AIBO is ready to perform a new action (True - AIBO is ready; False - AIBO is not ready)
        /// </summary>
        /// <returns>True - AIBO is ready; False - AIBO is not ready</returns>
        private bool aiboReady()
        {
            //Initialises a bool to false that will be turned to true if the AIBO is ready
            bool ready = false;
            
            //Asks AIBO if it is ready
            Connection.WriteLine("PRINT:%d:Wait");
            System.Threading.Thread.Sleep(300); //Waits for response

            //Reads the output
            string tempOutput = Connection.Read();

            //Prints output to console (mainly for debugging)
            //telnetOutput.Text += Environment.NewLine + tempOutput;
            
            //Removes trailing whitespace, checks if the last character is a 0; if so then assigns 'true' to ready
            string temp = tempOutput.Substring(tempOutput.Length - 3, 1);
            if (temp == "0")
            {
                ready = true;
            }
            
            return ready;
        }

        /// <summary>
        /// Background Worker thread so that UI is not slowed down by Telnet communications
        /// with AIBO. Fires _RunWorkerCompleted event when finished.
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments (in this case, expression we are passing)</param>
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Casts the argument back to a string
            string expression = e.Argument as string;

            //New Random number generator
            Random random = new Random();
            double rnd = random.NextDouble();

            //If the AIBO is connected and ready, run the command
            if (Connection != null && Connection.IsConnected && aiboReady())
            {
                //Converts expression into AIBO program to be run, and sends it
                if (expression == "happy")
                {
                    //Sends either happy or happy2 program
                    if (rnd < 0.8)  //I want happy to be picked more often than happy2, so have specified this here
                    {
                        Connection.WriteLine(happy);
                    }
                    else
                    {
                        Connection.WriteLine(happy2);
                    }
                    System.Threading.Thread.Sleep(300); //Waits for response
                }
                else if (expression == "surprise")
                {
                    //Sends program
                    Connection.WriteLine(surprise);
                    System.Threading.Thread.Sleep(300); //Waits for response
                }
                else if (expression == "angry")
                {
                    //Sends either angry or angry2 program
                    if (rnd < 0.51)
                    {
                        Connection.WriteLine(angry);
                    }
                    else
                    {
                        Connection.WriteLine(angry2);
                    }
                    System.Threading.Thread.Sleep(300); //Waits for response
                }
                else if (expression == "sad")
                {
                    //Sends program
                    Connection.WriteLine(sad);
                    System.Threading.Thread.Sleep(300); //Waits for response
                }

                //Returns the AIBOs response
                e.Result = Connection.Read();
            }
            else
            {
                //Returns a connection failed notice
                e.Result = expression + " expression not sent to AIBO as there is no connection";
            }            
        }

        /// <summary>
        /// Called when the backgroundWorker's DoWork thread has completed.
        /// </summary>
        /// <param name="sender">Sending function</param>
        /// <param name="e">Result object from DoWork thread</param>
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Prints result to console
            string output = e.Result as string;
            telnetOutput.Text += Environment.NewLine + output;
        }

        /// <summary>
        /// Attempts to connect to the AIBO using the specified IP address from the form's text box
        /// </summary>
        private void connect()
        {
            //Initialises a new TelnetConnection which connects to AIBO
            try
            {
                Connection = new TelnetConnection(ipTextBox.Text, 21002);
            }
            catch (System.Net.Sockets.SocketException)
            {
                telnetOutput.Text = Environment.NewLine + "Could not connect to specified IP." + Environment.NewLine + "Please specify a valid AIBO IP address and try again.";
                return;
            }

            //Checks connection status and writes initial AIBO output to screen
            if (Connection.IsConnected)
            {
                System.Threading.Thread.Sleep(300); //Waits for response
                telnetOutput.Text = Connection.Read();
            }
            else
            {
                telnetOutput.Text = Environment.NewLine + "Could not connect to specified IP." + Environment.NewLine + "Please specify a valid AIBO IP address and try again.";
            }
        }
        
        /// <summary>
        /// Starts method to connect to specified IP
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">EventArgs</param>
        private void connectButton_Click(object sender, EventArgs e)
        {
            //Calls connect to connect to specified IP
            connect();
        }
        
    }
}
