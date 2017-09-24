using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Images2
{
    static class Helpers
    {
        public static Rectangle BuildRectangle(Point oldLocation, Point newLocation)
        {
            return new Rectangle(new Point(oldLocation.X > newLocation.X ? newLocation.X : oldLocation.X, oldLocation.Y > newLocation.Y ? newLocation.Y : oldLocation.Y),
                new Size(Math.Abs(oldLocation.X - newLocation.X), Math.Abs(oldLocation.Y - newLocation.Y)));
        }

        public static string FindSmallImage()
        {
            string findString = ".\\subImage.png.jpg";
            string[] filesname = Directory.GetFiles(@".\");
            if (filesname.Contains(findString))
            {
                return findString;
            }
            return ".\\subImage.png";
        }

        internal static bool CheckSizes(Point oldLocation, Point newLocation, int width, int height, int length1, int length2)
        {
            int size = Math.Abs(oldLocation.X - newLocation.X) * Math.Abs(oldLocation.Y - newLocation.Y) ;
            int imageSize = width * height - size;
            if (imageSize < length1 + length2)
            {
                return false;
            }
            return true;            
        }

        public static bool isIndexInRectangle(int i, int j, Point oldLocation, Point newLocation)
        {
            int biggerX = 0, smallerX = 0, biggerY = 0, smallerY = 0;
            if (oldLocation.X > newLocation.X)
            {
                biggerX = oldLocation.X;
                smallerX = newLocation.X;
            }
            else
            {
                biggerX = newLocation.X;
                smallerX = oldLocation.X;
            }
            if (oldLocation.Y > newLocation.Y)
            {
                biggerY = oldLocation.Y;
                smallerY = newLocation.Y;
            }
            else
            {
                biggerY = newLocation.Y;
                smallerY = oldLocation.Y;
            }

            if ((i > smallerX && i < biggerX) && (j > smallerY && j < biggerY))
            {
                return true;
            }
            return false;
        }

        internal static Color GetNewColor(Color oldColor, byte insert)
        {
            int oldR = oldColor.R;
            int oldG = oldColor.G;
            int oldB = oldColor.B;

            byte insertByteR = Convert.ToByte(Convert.ToByte(insert >> 6) & Convert.ToByte(0x3));
            byte insertByteG = Convert.ToByte(Convert.ToByte(insert >> 3) & Convert.ToByte(0x7));
            byte insertByteB = Convert.ToByte(Convert.ToByte(insert) & Convert.ToByte(0x7));

            int newR = (insertByteR) ^ (oldR & 0x3) | (oldR);
            int newG = (insertByteG) ^ (oldG & 0x7) | (oldG);
            int newB = (insertByteB) ^ (oldB & 0x7) | (oldB);

            Color result = Color.FromArgb(newR, newG, newB);

            return result;
        }

        public static string GetStringFromByte(byte a)
        {
            string outS = "";
            while (a != 0)
            {
                if ((a >> 7) == 0)
                {
                    outS += "0";
                }
                else
                {
                    outS += "1";
                }
                a = (byte) (a << 1);                    
            }            
            while (outS.Length != 8)
            {
                outS += "0";
            }
            return outS;
        }

        public static string GetStringFromByte(int a)
        {
            string outS = "";
            while (a != 0)
            {
                if ((a >> 7) == 0)
                {
                    outS += "0";
                }
                else
                {
                    outS += "1";
                }
                a = (byte)(a << 1);
            }
            while (outS.Length != 8)
            {
                outS += "0";
            }
            return outS;
        }
    }
}
