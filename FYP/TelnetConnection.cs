// minimalistic telnet implementation
// conceived by Tom Janssens on 2007/06/06  for codeproject
//
// http://www.corebvba.be

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace FYP
{    
    //Holds values for telnet server option keywords
    enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }

    /// <summary>
    /// TelnetConnection class to provide telnet connection support for project.
    /// Programmed by:
    /// Tom Janssens on 2007/06/06  for codeproject
    /// http://www.corebvba.be
    /// </summary>
    class TelnetConnection
    {
        //Declares a tcpSocket
        TcpClient tcpSocket;

        int TimeOutMs = 100;  //Sets a connection timeout value

        /// <summary>
        /// Constructor for TelnetConnection, creates a TCP socket to the relevant host
        /// </summary>
        /// <param name="hostIP">IP Address of the host</param>
        /// <param name="port">Port number we want to connect to</param>
        public TelnetConnection(string hostIP, int port)
        {
            tcpSocket = new TcpClient(hostIP, port);
        }

        /// <summary>
        /// Returns state of TCP connection (true = connected; false = not connected)
        /// </summary>
        public bool IsConnected
        {
            get { return tcpSocket.Connected; }
        }

        /// <summary>
        /// Provides method for logging in with the given username and password
        /// </summary>
        /// <param name="username">The Username to log in with</param>
        /// <param name="password">The Password to log in with</param>
        /// <param name="loginTimeOutMs">The timeout before we assume connection has failed</param>
        /// <returns>The shell prompt</returns>
        public string Login(string username, string password, int loginTimeOutMs)
        {
            int oldTimeOutMs = TimeOutMs;
            TimeOutMs = loginTimeOutMs;
            string s = Read();
            if (!s.TrimEnd().EndsWith(":"))
               throw new Exception("Failed to connect : no login prompt");
            WriteLine(username);

            s += Read();
            if (!s.TrimEnd().EndsWith(":"))
                throw new Exception("Failed to connect : no password prompt");
            WriteLine(password);

            s += Read();
            TimeOutMs = oldTimeOutMs;
            return s;
        }

        /// <summary>
        /// Appends newline character and calls 'Write()' method to send string to host
        /// </summary>
        /// <param name="cmd">Command string to send</param>
        public void WriteLine(string cmd)
        {
            Write(cmd + "\n");
        }

        /// <summary>
        /// Sends string to host with the correct encoding
        /// </summary>
        /// <param name="cmd">Command string to send</param>
        public void Write(string cmd)
        {
            if (!tcpSocket.Connected) return;
            byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(cmd.Replace("\0xFF","\0xFF\0xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// Reads the server output
        /// </summary>
        /// <returns>String of the server output</returns>
        public string Read()
        {
            if (!tcpSocket.Connected) return null;
            StringBuilder sb=new StringBuilder();
            do
            {
                parseTelnet(sb);
                System.Threading.Thread.Sleep(TimeOutMs);
            } while (tcpSocket.Available > 0);
            return sb.ToString().Replace("\0", String.Empty);  //Replace removes null character from string
        }

        /// <summary>
        /// Parses the telnet output and does some Telnet Protocol handling of certain server options
        /// </summary>
        /// <param name="sb">Current StringBuilder object that contains output string from host</param>
        void parseTelnet(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1 :
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC: 
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO: 
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA )
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL:(byte)Verbs.DO); 
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT); 
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append( (char)input );
                        break;
                }
            }
        }
    }
}
