using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wavelet
{
    public partial class Form1 : Form
    {
        const int IMAGE_WIDTH = 512;
        const int IMAGE_HEIGHT = 512;
        const int HEADER_SIZE = 1078;

        string OriginalImagePath;
        byte[,] OriginalImageMatrix;
        double[,] DecodedImageMatrix;

        double[] LowAnalysis = { 0.026748757411, -0.016864118443, -0.078223266529, 0.266864118443, 0.602949018236, 0.266864118443, -0.078223266529, -0.016864118443, 0.026748757411 };
        double[] HighAnalysis = { 0.000000000000, 0.091271763114, -0.057543526229, -0.591271763114, 1.115087052457, -0.591271763114, -0.057543526229, 0.091271763114, 0.000000000000 };
        public Form1()
        {
            InitializeComponent();
            OriginalImagePath = null;
            OriginalImageMatrix = new byte[IMAGE_HEIGHT, IMAGE_WIDTH];
            DecodedImageMatrix = new double[IMAGE_HEIGHT, IMAGE_WIDTH];
        }

        private void CoderLoadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\School";
                openFileDialog.Filter = "BMP FIles (*.bmp)|*.BMP";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    OriginalImagePath = openFileDialog.FileName;
                    ReadOriginalImageBytes();
                    using (var bmpTemp = new Bitmap(OriginalImagePath))
                    {
                        OriginalImagePictureBox.Image = new Bitmap(bmpTemp);
                    }
                }
            }
        }

        void ReadOriginalImageBytes()
        {
            using (BinaryReader reader = new BinaryReader(File.Open(OriginalImagePath, FileMode.Open)))
            {
                for (int i = 0; i < HEADER_SIZE; i++)
                {
                    reader.ReadByte();
                }

                for (int i = IMAGE_HEIGHT - 1; i >= 0; i--)
                {
                    for (int j = 0; j < IMAGE_WIDTH; j++)
                    {
                        byte value = reader.ReadByte();
                        OriginalImageMatrix[i, j] = value;
                        DecodedImageMatrix[i, j] = value;
                    }
                }
            }
        }

        void HorizontalAnalysis(int level)
        {
            int length = GetLength(level);
            for (int line = 0; line < IMAGE_HEIGHT; line++)
            {
                double[] low = ProcessLine(line, length, LowAnalysis);
                double[] high = ProcessLine(line, length, HighAnalysis);
                for (int i = 0; i < length / 2; i++)
                {
                    DecodedImageMatrix[line, i] = low[2 * i];
                }
                int j = length / 2;
                for (int i = 0; i < length / 2; i++)
                {
                    DecodedImageMatrix[line, j] = high[2 * i + 1];
                    j++;
                }
            }
        }

        int GetLength(int level)
        {
            level--;
            int divisionFactor = (int)Math.Pow(2, level);
            int length = IMAGE_HEIGHT / divisionFactor;
            return length;
        }

        private double[] ProcessLine(int line, int length, double[] mask)
        {
            double[] buffer = new double[length];
            for (int i = 0; i < length; i++)
            {
                int pixelOffset = -4;
                double result = 0;
                for (int j = 0; j < 8; j++)
                {
                    double filterValue = mask[j];
                    int pixelIndex = i;
                    pixelIndex += pixelOffset;
                    if (i < 4)
                    {
                        pixelIndex = Math.Abs(pixelIndex);
                    }

                    if (pixelIndex > length - 1)
                    {
                        int difference = pixelIndex - (length - 1);
                        pixelIndex = length - difference;
                    }

                    double pixelValue = DecodedImageMatrix[line, pixelIndex];
                    result += filterValue * pixelValue;
                    pixelOffset++;
                }
                buffer[i] = result;
            }
            return buffer;
        }

        void VerticalAnalysis(int level)
        {
            int length = GetLength(level);
            for (int column = 0; column < IMAGE_HEIGHT; column++)
            {
                double[] low = ProcessColumn(column, length, LowAnalysis);
                double[] high = ProcessColumn(column, length, HighAnalysis);
                for (int i = 0; i < length / 2; i++)
                {
                    DecodedImageMatrix[i, column] = low[2 * i];
                }
                int j = length / 2;
                for (int i = 0; i < length / 2; i++)
                {
                    DecodedImageMatrix[j, column] = high[2 * i + 1];
                    j++;
                }
            }
        }

        private double[] ProcessColumn (int column, int length, double[] mask)
        {
            double[] buffer = new double[length];
            for (int i = 0; i < length; i++)
            {
                int pixelOffset = -4;
                double result = 0;
                for (int j = 0; j < 8; j++)
                {
                    double filterValue = mask[j];
                    int pixelIndex = i;
                    pixelIndex += pixelOffset;
                    if (i < 4)
                    {
                        pixelIndex = Math.Abs(pixelIndex);
                    }

                    if (pixelIndex > length - 1)
                    {
                        int difference = pixelIndex - (length - 1);
                        pixelIndex = length - difference;
                    }

                    double pixelValue = DecodedImageMatrix[pixelIndex, column];
                    result += filterValue * pixelValue;
                    pixelOffset++;
                }
                buffer[i] = result;
            }
            return buffer;
        }

        private void DrawImage()
        {
            Bitmap image = new Bitmap(IMAGE_HEIGHT, IMAGE_WIDTH);
            for (int i = 0; i < IMAGE_HEIGHT; i++)
            {
                for (int j = 0; j < IMAGE_WIDTH; j++)
                {
                    int color = (int)DecodedImageMatrix[i, j];
                    if (color < 0)
                    {
                        color = 0;
                    }

                    if (color > 255)
                    {
                        color = 255;
                    }
                    image.SetPixel(j, i, Color.FromArgb(color, color, color));
                }
            }
            DecodedImagePictureBox.Image = image;
        }

        private void AnH1_Click(object sender, EventArgs e)
        {
            HorizontalAnalysis(1);
            DrawImage();
        }

        private void AnV1_Click(object sender, EventArgs e)
        {
            VerticalAnalysis(1);
            DrawImage();
        }

        private void AnH2_Click(object sender, EventArgs e)
        {
            HorizontalAnalysis(2);
            DrawImage();
        }

        private void AnV2_Click(object sender, EventArgs e)
        {
            VerticalAnalysis(2);
            DrawImage();
        }

        private void AnH3_Click(object sender, EventArgs e)
        {
            HorizontalAnalysis(3);
            DrawImage();
        }

        private void AnV3_Click(object sender, EventArgs e)
        {
            VerticalAnalysis(3);
            DrawImage();
        }

        private void AnH4_Click(object sender, EventArgs e)
        {
            HorizontalAnalysis(4);
            DrawImage();
        }

        private void AnV4_Click(object sender, EventArgs e)
        {
            VerticalAnalysis(4);
            DrawImage();
        }

        private void AnH5_Click(object sender, EventArgs e)
        {
            HorizontalAnalysis(5);
            DrawImage();
        }

        private void AnV5_Click(object sender, EventArgs e)
        {
            VerticalAnalysis(5);
            DrawImage();
        }
    }
}
