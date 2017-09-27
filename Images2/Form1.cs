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
            var dialogResult = openFileDialog1.ShowDialog();
            if (dialogResult != DialogResult.OK)
            {
                System.Environment.Exit(0);
                return;
            }
            try
            {
                input = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Refresh();
            }
            catch
            {
                MessageBox.Show("Выбрано плохое изображение!");
            }
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
                if (oldLocation == new Point(0, 0))
                {
                    throw new Exception();
                }
                subImage = subImage.Clone(rectangle, inputFormat);
            }
            catch
            {
                MessageBox.Show("Область выбери");
                return;
            }
            subImage.Save(@".\subImage.png");
            subImage.Dispose();
            try
            {
                Crypto.Insert(oldLocation, newLocation, pictureBox1, input).Save("Hidden.png");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
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

        private void button2_Click(object sender, EventArgs e)
        {
            var dialogResult = openFileDialog1.ShowDialog();
            if (dialogResult != DialogResult.OK)
            {
                return;
            }
            try
            {
                input = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Refresh();
            }
            catch
            {
                MessageBox.Show("Выбрано плохое изображение!");
                return;
            }
            try
            {
                Crypto.Extract(input, pictureBox1).Save("Extracted.png");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }

        }
    }
}

