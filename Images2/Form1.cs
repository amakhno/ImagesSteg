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
            
            if (!Helpers.CheckSizes(oldLocation, newLocation, pictureBox1.Image.Width, pictureBox1.Image.Height, byteLenght.Length, encodedSubImageString.Length))
            {
                MessageBox.Show("Слишком большая область");
                return;
            }

            Color oldColor;
            Color newColor = Color.Black;
            int byteLenghtIndex = 0;
            int i = 0;
            for (int j = 0; j<input.Width; j++)
            {
                for (int k = 0; k < input.Height; k++)
                {
                    if (i == encodedSubImageString.Length)
                    {
                        break;
                    }
                    if (Helpers.isIndexInRectangle(j, k, oldLocation, newLocation))
                    {
                        k += Math.Abs(oldLocation.X - newLocation.X);
                        continue;
                    }

                    if (byteLenghtIndex < 4)
                    {
                        oldColor = input.GetPixel(j, k);
                        newColor = Helpers.GetNewColor(oldColor, byteLenght[byteLenghtIndex]);
                        input.SetPixel(j, k, newColor);
                        byteLenghtIndex++;
                    }
                    else
                    {
                        oldColor = input.GetPixel(j, k);
                        newColor = Helpers.GetNewColor(oldColor, encodedSubImageString[i]);
                        newColor = Color.Black;
                        input.SetPixel(j, k, newColor);
                        i++;
                    }                    
                }
                if (i == encodedSubImageString.Length)
                {
                    break;
                }
            }
            pb.FillRectangle(Brushes.Black, oldLocation.X, oldLocation.Y, newLocation.X - oldLocation.X, newLocation.Y - oldLocation.Y);
            //pictureBox1.Image.Save("output.png");
            Crypto.DecryptData(@".\subImage.png.out.jpg", @".\subImage.png.after.jpg", key, iV);            
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

