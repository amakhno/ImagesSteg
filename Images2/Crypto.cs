using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Images2
{
    static class Crypto
    {
        public static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public static void EncryptData(String inName, String outName, byte[] desKey, byte[] desIV)
        {
            //Create the file streams to handle the input and output files.
            FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);

            //Create variables to help with read and write.
            byte[] bin = new byte[100]; //This is intermediate storage for the encryption.
            long rdlen = 0;              //This is the total number of bytes written.
            long totlen = fin.Length;    //This is the total length of the input file.
            int len;                     //This is the number of bytes to be written at a time.

            DES des = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(fout, des.CreateEncryptor(desKey, desIV), CryptoStreamMode.Write);

            Console.WriteLine("Encrypting...");

            //Read from the input file, then encrypt and write to the output file.
            while (rdlen < totlen)
            {
                len = fin.Read(bin, 0, 100);
                encStream.Write(bin, 0, len);
                rdlen = rdlen + len;
                Console.WriteLine("{0} bytes processed", rdlen);
            }

            encStream.Close();
            fout.Close();
            fin.Close();
        }

        internal static Bitmap Extract(Bitmap input, PictureBox pictureBox1)
        {
            byte[] key = Crypto.LongToByteArray(int.MaxValue);
            byte[] iV = Crypto.LongToByteArray(40);

            Point uploadedOldPos = new Point(0, 0);
            Point uploadedNewPos = new Point(0, 0);
            int xOld = 0, yOld = 0;
            var points = Helpers.ExtractRectangle(input, ref xOld, ref yOld);
            uploadedOldPos = points[0];
            uploadedNewPos = points[1];
            if (!(Math.Abs(uploadedOldPos.X) < input.Width && Math.Abs(uploadedOldPos.Y) < input.Height && Math.Abs(uploadedNewPos.X) < input.Width && Math.Abs(uploadedNewPos.Y) < input.Height))
            {
                throw new Exception("Не смог разобрать содержимое");
            }
            int extractSize = Helpers.ExtractSize(input, ref xOld, ref yOld);

            byte[] extractedImage = new byte[extractSize];
            int extractedIndex = 0;
            for (int x = xOld; x < input.Width; x++)
            {
                for (int y = yOld; y < input.Height; y++)
                {
                    if (extractedIndex >= extractSize)
                    {
                        break;
                    }
                    extractedImage[extractedIndex] = Helpers.ExtractByte(input, x, y);
                    extractedIndex++;
                }
                if (extractedIndex >= extractSize)
                {
                    break;
                }
                yOld = 0;
            }

            File.WriteAllBytes("izzz.coded", extractedImage);
            Crypto.DecryptData(@".\izzz.coded", @".\outSmallDecoded.png", key, iV);
            File.Delete(@".\izzz.coded");
            using (Graphics gr = Graphics.FromImage(input))
            {
                using (Bitmap smallImage = new Bitmap(@".\outSmallDecoded.png"))
                {
                    gr.DrawImage(smallImage, uploadedOldPos);
                    pictureBox1.Image = input;
                }
                File.Delete(@".\outSmallDecoded.png");
            }
            pictureBox1.Refresh();
            return input;
        }

        internal static Bitmap Insert(Point oldLocation, Point newLocation, PictureBox pictureBox1, Bitmap input)
        {
            Compressor.Compress(@".\subImage.png");
            string pathToCompressImage = Helpers.FindSmallImage();

            byte[] key = Crypto.LongToByteArray(int.MaxValue);
            byte[] iV = Crypto.LongToByteArray(40);

            Crypto.EncryptData(pathToCompressImage, @".\subImage.png.out.jpg", key, iV);

            File.Delete(pathToCompressImage);

            byte[] encodedSubImageString = File.ReadAllBytes(@".\subImage.png.out.jpg");
            File.Delete(@".\subImage.png.out.jpg");
            byte[] byteLenght = Crypto.IntToByteArray(encodedSubImageString.Length);

            byte[][] rect = Helpers.BuildRectangle(oldLocation, newLocation, true);

            if (!Helpers.CheckSizes(oldLocation, newLocation, pictureBox1.Image.Width, pictureBox1.Image.Height, byteLenght.Length, encodedSubImageString.Length))
            {
                throw new Exception("Слишком большая область");
            }

            int rectIndexX = 0;
            int rectIndexY = 0;
            Color oldColor;
            Color newColor = Color.Black;
            int byteLenghtIndex = 0;
            int i = 0;
            for (int k = 0; k < input.Width; k++)
            {
                for (int j = 0; j < input.Height; j++)
                {
                    if (i == encodedSubImageString.Length)
                    {
                        break;
                    }
                    if (Helpers.isIndexInRectangle(k, j, oldLocation, newLocation))
                    {
                        k += Math.Abs(oldLocation.X - newLocation.X);
                        continue;
                    }

                    if (rectIndexX < 4 && rectIndexY < 4)
                    {
                        oldColor = input.GetPixel(k, j);
                        newColor = Helpers.GetNewColor(oldColor, rect[rectIndexY][rectIndexX]);
                        input.SetPixel(k, j, newColor);
                        if (rectIndexX == 3)
                        {
                            rectIndexY++;
                            rectIndexX = 0;
                            continue;
                        }
                        rectIndexX++;
                        continue;
                    }
                    else if (byteLenghtIndex < 4)
                    {
                        oldColor = input.GetPixel(k, j);
                        newColor = Helpers.GetNewColor(oldColor, byteLenght[byteLenghtIndex]);
                        input.SetPixel(k, j, newColor);
                        byteLenghtIndex++;
                        continue;
                    }
                    else
                    {
                        oldColor = input.GetPixel(k, j);
                        newColor = Helpers.GetNewColor(oldColor, encodedSubImageString[i]);
                        input.SetPixel(k, j, newColor);
                        i++;
                        continue;
                    }
                }
                if (i == encodedSubImageString.Length)
                {
                    break;
                }
            }
            using (Graphics g = Graphics.FromImage(input))
            {
                g.FillRectangle(Brushes.Black, oldLocation.X, oldLocation.Y, newLocation.X - oldLocation.X, newLocation.Y - oldLocation.Y);
                pictureBox1.Refresh();
            }
            return input;
        }

        public static void DecryptData(String inName, String outName, byte[] desKey, byte[] desIV)
        {
            //Create the file streams to handle the input and output files.
            FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);

            //Create variables to help with read and write.
            byte[] bin = new byte[100]; //This is intermediate storage for the encryption.
            long rdlen = 0;              //This is the total number of bytes written.
            long totlen = fin.Length;    //This is the total length of the input file.
            int len;                     //This is the number of bytes to be written at a time.

            DES des = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(fout, des.CreateDecryptor(desKey, desIV), CryptoStreamMode.Write);

            Console.WriteLine("Decrypting...");

            //Read from the input file, then encrypt and write to the output file.
            while (rdlen < totlen)
            {
                len = fin.Read(bin, 0, 100);
                encStream.Write(bin, 0, len);
                rdlen = rdlen + len;
                Console.WriteLine("{0} bytes processed", rdlen);
            }

            encStream.Close();
            fout.Close();
            fin.Close();
        }

        public static byte[] LongToByteArray(ulong intValue)
        {
            byte[] intBytes = BitConverter.GetBytes(intValue);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            byte[] result = intBytes;
            return result;
        }

        public static byte[] IntToByteArray(int intValue)
        {
            byte[] intBytes = BitConverter.GetBytes(intValue);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            byte[] result = intBytes;
            return result;
        }
    }
}
