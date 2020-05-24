﻿using System;
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
        const int NUMBER_OF_FILTER_ELEMENTS = 9;

        string OriginalImagePath;
        byte[,] OriginalImageMatrix;
        double[,] DecodedImageMatrix;

        double[] LowAnalysis = { 0.026748757411, -0.016864118443, -0.078223266529, 0.266864118443, 0.602949018236, 0.266864118443, -0.078223266529, -0.016864118443, 0.026748757411 };
        double[] HighAnalysis = { 0.000000000000, 0.091271763114, -0.057543526229, -0.591271763114, 1.115087052457, -0.591271763114, -0.057543526229, 0.091271763114, 0.000000000000 };
        double[] LowSynthesis = { 0.000000000000, -0.091271763114, -0.057543526229, 0.591271763114, 1.115087052457, 0.591271763114, -0.057543526229, -0.091271763114, 0.000000000000 };
        double[] HighSynthesis = { 0.026748757411, 0.016864118443, -0.078223266529, -0.266864118443, 0.602949018236, -0.266864118443, -0.078223266529, 0.016864118443, 0.026748757411 };

        byte[] testArray = {1, 2, 3, 4, 5, 6, 7, 8, 9, 9, 9, 9, 9, 9, 3, 2, 7, 5, 2, 8, 2, 55, 2, 7, 3, 1, 6, 9, 1, 3, 2, 66 };
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
        int GetLength(int level)
        {
            level--;
            int divisionFactor = (int)Math.Pow(2, level);
            int length = IMAGE_HEIGHT / divisionFactor;
            return length;
        }
        void HorizontalAnalysis(int level)
        {
            int length = GetLength(level);
            for (int line = 0; line < IMAGE_HEIGHT; line++)
            {
                double[] low = AnalyzeLine(line, length, LowAnalysis);
                double[] high = AnalyzeLine(line, length, HighAnalysis);
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

        private double[] AnalyzeLine(int line, int length, double[] mask)
        {
            double[] buffer = new double[length];
            for (int i = 0; i < length; i++)
            {
                int pixelOffset = -4;
                double result = 0;
                for (int j = 0; j < NUMBER_OF_FILTER_ELEMENTS; j++)
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
                        pixelIndex = length - 1 - difference;
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
                double[] low = AnalyzeColumn(column, length, LowAnalysis);
                double[] high = AnalyzeColumn(column, length, HighAnalysis);
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

        private double[] AnalyzeColumn (int column, int length, double[] mask)
        {
            double[] buffer = new double[length];
            for (int i = 0; i < length; i++)
            {
                int pixelOffset = -4;
                double result = 0;
                for (int j = 0; j < NUMBER_OF_FILTER_ELEMENTS; j++)
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
                        pixelIndex = length - 1 - difference;
                    }

                    double pixelValue = DecodedImageMatrix[pixelIndex, column];
                    result += filterValue * pixelValue;
                    pixelOffset++;
                }
                buffer[i] = result;
            }
            return buffer;
        }

        void VerticalSynthesis(int level)
        {
            /* int length = GetLength(level); */
            int length = GetLength(level);
            for (int column = 0; column < IMAGE_HEIGHT; column++)
            {
                double[] lowVector = new double[length];
                for (int i = 0; i < length / 2; i++)
                {
                    lowVector[2 * i] = DecodedImageMatrix[i, column];
                    lowVector[2 * i + 1] = 0;
                }

                double[] highVector = new double[length];
                int start = 0;
                for (int i = length / 2; i < length; i++)
                {
                    highVector[start] = 0;
                    highVector[start + 1] = DecodedImageMatrix[i, column];
                    start += 2;
                }

                double[] low = SynthesizeColumn(lowVector, length, LowSynthesis);
                double[] high = SynthesizeColumn(highVector, length, HighSynthesis);
                for (int i = 0; i < length; i++)
                {
                    DecodedImageMatrix[i, column] = low[i] + high[i];
                }
            }

        }

        double[] SynthesizeColumn(double[] vector, int length, double[] mask)
        {
            double[] buffer = new double[length];
            for (int i = 0; i < length; i++)
            {
                int pixelOffset = -4;
                double result = 0;
                for (int j = 0; j < NUMBER_OF_FILTER_ELEMENTS; j++)
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
                        pixelIndex = length - 1 - difference;
                    }

                    double pixelValue = vector[pixelIndex];
                    result += filterValue * pixelValue;
                    pixelOffset++;
                }
                buffer[i] = result;
            }
            return buffer;
        }

        void HorizontalSynthesis(int level)
        {
            int length = GetLength(level);
            for (int line = 0; line < IMAGE_HEIGHT; line++)
            {
                double[] lowVector = new double[length];
                for (int i = 0; i < length / 2; i++)
                {
                    lowVector[2 * i] = DecodedImageMatrix[line, i];
                    lowVector[2 * i + 1] = 0;
                }

                double[] highVector = new double[length];
                int start = 0;
                for (int i = length / 2; i < length; i++)
                {
                    highVector[start] = 0;
                    highVector[start + 1] = DecodedImageMatrix[line, i];
                    start += 2;
                }

                double[] low = SynthesizeColumn(lowVector, length, LowSynthesis);
                double[] high = SynthesizeColumn(highVector, length, HighSynthesis);
                for (int i = 0; i < length; i++)
                {
                    DecodedImageMatrix[line, i] = low[i] + high[i];
                }
            }

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

        private void SyV5_Click(object sender, EventArgs e)
        {
            VerticalSynthesis(5);
            DrawImage();
        }

        private void SyH5_Click(object sender, EventArgs e)
        {
            HorizontalSynthesis(5);
            DrawImage();
        }

        private void SyV4_Click(object sender, EventArgs e)
        {
            VerticalSynthesis(4);
            DrawImage();
        }

        private void SyH4_Click(object sender, EventArgs e)
        {
            HorizontalSynthesis(4);
            DrawImage();
        }

        private void SyV3_Click(object sender, EventArgs e)
        {
            VerticalSynthesis(3);
            DrawImage();
        }

        private void SyH3_Click(object sender, EventArgs e)
        {
            HorizontalSynthesis(3);
            DrawImage();
        }

        private void SyV2_Click(object sender, EventArgs e)
        {
            VerticalSynthesis(2);
            DrawImage();
        }

        private void SyH2_Click(object sender, EventArgs e)
        {
            HorizontalSynthesis(2);
            DrawImage();
        }

        private void SyV1_Click(object sender, EventArgs e)
        {
            VerticalSynthesis(1);
            DrawImage();
        }

        private void SyH1_Click(object sender, EventArgs e)
        {
            HorizontalSynthesis(1);
            DrawImage();
        }
    }
}
