using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Emgu.CV;

namespace FYP
{
    /// <summary>
    /// Contains code to deal with the contours struct of EmguCV
    /// </summary>
    static class Contours
    {

        /// <summary>
        /// Finds and returns the two longest contours which are 'valid' (Ie, they intersect the line y = frameWidth/2).
        /// Using code adapted from Rob Sollars' project.
        /// </summary>
        /// <param name="contours">All of the contours in the frame</param>
        /// <param name="frameWidth">Width of the frame</param>
        /// <param name="contour1">The largest contour</param>
        /// <param name="contour2">The second largest contour</param>
        public static void ValidContours(Contour<Point> contours, int frameWidth, out Contour<Point> contour1, out  Contour<Point> contour2)
        {
            //Declaring variables to store contours
            MemStorage stor1 = new MemStorage();
            MemStorage stor2 = new MemStorage();
            contour1 = new Contour<Point>(stor1);
            contour2 = new Contour<Point>(stor2);

            //Declares boolean to hold if contour is 'valid'
            bool valid;

            //Loops through each contour in contours; testing to see if it is valid (intersects the line y = frameWidth/2) and
            // larger than any of the elements in the array, reordering and inserting the contour in the array if so
            for (; contours != null; contours = contours.HNext)
            {
                //Tests and stores for contour being valid; saves computing value each time (verifyContour is only called once)
                valid = VerifyContour(contours, frameWidth);

                //If-ElseIf construct to see if contour is one of the 3 biggest 'valid' contours
                if (valid && contours.Total > contour1.Total)
                {
                    contour2 = contour1;
                    contour1 = contours;
                }
                else if (valid && contours.Total > contour2.Total)
                {
                    contour2 = contours;
                }
            }            

            //Empties contour positions in array if points would be null
            if (contour1.Total < 3)
            {
                contour1 = null;
            }

            if (contour2 != null && contour2.Total < 3)
            {
                contour2 = null;
            }

            //Disposes of memory storage
            stor1.Dispose();
            stor2.Dispose();            
        }

        /// <summary>
        /// Ensures that the contour intersects the line y = frameWidth/2; which indicates the contour could represent the element
        /// we are looking for.
        /// </summary>
        /// <param name="contour">Contour being tested</param>
        /// <param name="frameWidth">The width of the frame</param>
        /// <returns>True if contour intersects the mid point of the frame</returns>
        public static bool VerifyContour(Contour<Point> contour, int frameWidth)
        {
            Point leastX = new Point();
            Point greatestX = new Point();
            Point mid = new Point();

            ExtractPoints(contour, out leastX, out greatestX, out mid);

            //Declare boolean variable to return
            bool intersect = false;

            //Test if contour's left-most point is to the left of the frame mid point; and if the right-most point is on the right
            // if so, the contour intersects y = frameWidth/2
            if (leastX.X < frameWidth / 2 && greatestX.X > frameWidth / 2)
            {
                intersect = true;
            }

            return intersect;
        }

        /// <summary>
        /// Finds the start, mid and end points of a horizontal line implied by the contour.
        /// Code adapted from Rob Sollars' project.
        /// </summary>
        /// <param name="Contours">A list of contours containing the points</param>
        /// <param name="LeastX">The left most point</param>
        /// <param name="GreatestX">The right most point</param>
        /// <param name="MidX">The middle point</param>
        public static void ExtractPoints(Contour<Point> Contours, out Point LeastX, out Point GreatestX, out Point MidX)
        {
            LeastX = new Point(0,0);
            GreatestX = new Point(0, 0);
            MidX = new Point(0, 0);

            bool first = true;

            if (Contours != null)
            {
                foreach (Point Point in Contours)
                {
                    if (first)
                    {
                        LeastX = Point;
                        GreatestX = Point;

                        first = false;
                    }

                    if (Point.X < LeastX.X)
                    {
                        LeastX = Point;
                    }
                    if (Point.X > GreatestX.X)
                    {
                        GreatestX = Point;
                    }
                }

                MidX.X = (LeastX.X + GreatestX.X) / 2;
                MidX.Y = (LeastX.Y + GreatestX.Y) / 2;
            }
        }

        /// <summary>
        /// Finds the start, end, top and bottom points of a contour. Code adapted from Rob Sollars' project.
        /// </summary>
        /// <param name="Contours">A list of contours containing the points</param>
        /// <param name="LeastX">The left most point</param>
        /// <param name="GreatestX">The right most point</param>
        /// <param name="GreatestY">The top most point</param>
        /// <param name="LeastY">The bottom point</param>
        public static void ExtractPoints(Contour<Point> Contours, out Point LeastX, out Point GreatestX, out Point GreatestY, out Point LeastY)
        {
            LeastX = new Point(0,0);
            GreatestX = new Point(0, 0);
            GreatestY = new Point(0, 0);
            LeastY = new Point(0, 0);

            bool First = true;

            if (Contours != null)
            {
                foreach (Point Point in Contours)
                {
                    if (First)
                    {
                        LeastX = Point;
                        GreatestX = Point;
                        LeastY = Point;
                        GreatestY = Point;

                        First = false;
                    }

                    //Extracts left and right points
                    if (Point.X < LeastX.X)
                    {
                        LeastX = Point;
                    }
                    if (Point.X > GreatestX.X)
                    {
                        GreatestX = Point;
                    }
                    //Extracts top and bottom points
                    if (Point.Y < LeastY.Y)
                    {
                        LeastY = Point;
                    }
                    if (Point.Y > GreatestY.Y)
                    {
                        GreatestY = Point;
                    }
                }
            }
        }
    }
}
