using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Compression;
using System.IO;
using System.Security.Cryptography;

namespace Images2
{
    public partial class Form1 : Form
    {
        Point oldLocation = new Point();
        bool isDraw = false;
        Point newLocation = new Point();
        Graphics pb;
        Pen p = new Pen(Color.Black, 1);
        System.Drawing.Imaging.PixelFormat inputFormat;
        Bitmap input;

        public Form1()
        {
            InitializeComponent();
            input = new Bitmap(@".\image.png");
            pictureBox1.Image = input;
            inputFormat = input.PixelFormat;
            pb = pictureBox1.CreateGraphics();
            p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap subImage = ((Bitmap)pictureBox1.Image);
            Rectangle rectangle = Helpers.BuildRectangle(oldLocation, newLocation); 
            try
            {
                subImage = subImage.Clone(rectangle, inputFormat);
            }
            catch
            {
                MessageBox.Show("Область выбери");
                return;
            }
            subImage.Save(@".\subImage.png");
            subImage.Dispose();

            Compressor.Compress(@".\subImage.png");
            string pathToCompressImage = Helpers.FindSmallImage();

            Bitmap encoded = new Bitmap(pathToCompressImage);
            byte[] key = Crypto.LongToByteArray(int.MaxValue);
            byte[] iV = Crypto.LongToByteArray(40);
            Crypto.EncryptData(pathToCompressImage, @".\subImage.png.out.jpg", key, iV);

            byte[] encodedSubImageString = File.ReadAllBytes(@".\subImage.png.out.jpg");

            byte[] byteLenght = Crypto.IntToByteArray(encodedSubImageString.Length);

            byte[][] rect = Helpers.BuildRectangle(oldLocation, newLocation, true);

            if (!Helpers.CheckSizes(oldLocation, newLocation, pictureBox1.Image.Width, pictureBox1.Image.Height, byteLenght.Length, encodedSubImageString.Length))
            {
                MessageBox.Show("Слишком большая область");
                return;
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
            pb.Save();
            pictureBox1.Image.Save("out.png");

            //////////////Extract
            Point uploadedOldPos = new Point(0, 0);
            Point uploadedNewPos = new Point(0, 0);
            int xOld = 0, yOld = 0;
            var points = Helpers.ExtractRectangle(input, ref xOld, ref yOld);
            uploadedOldPos = points[0];
            uploadedNewPos = points[1];
            int extractSize = Helpers.ExtractSize(input, ref xOld, ref yOld);

            byte[] extractedImage = new byte[extractSize];
            int extractedIndex = 0;
            for (int x = xOld; x<input.Width; x++)
            {
                for (int y = yOld; y < input.Height; y++)
                {
                    if (extractedIndex >= extractSize)
                    {
                        break;
                    }

                    extractedImage[extractedIndex] = Helpers.ExtractByte(input, x, y);
                    if (extractedImage[extractedIndex] != encodedSubImageString[extractedIndex])
                    {
                        ;
                    }
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

            using (Graphics gr = Graphics.FromImage(input))
            {
                using (Bitmap smallImage = new Bitmap(@".\outSmallDecoded.png"))
                {
                    gr.DrawImage(smallImage, uploadedOldPos);
                }                    
            }
            pictureBox1.Refresh();     
        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraw)
            {
                pictureBox1.Refresh();
                pb.DrawRectangle(p, oldLocation.X, oldLocation.Y, e.X - oldLocation.X, e.Y - oldLocation.Y);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDraw = false;
            newLocation = e.Location;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Refresh();
            oldLocation = e.Location;
            isDraw = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            pictureBox1.Image.Save("output.png");
        }
    }
}

