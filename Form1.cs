﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Win32;

namespace WaveAnalyzer
{
    public partial class Form1 : Form
    {
        [DllImport("PleaseWork.dll")]
        public static extern int Function1(int x, int y);
        [DllImport("PleaseWork.dll")]
        public static extern int Function2(int x, int y);
        //need to marshal or something to get hinstance and pstr
        [DllImport("Record.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int DllMain(IntPtr hinstanceorsomeshit);
        [DllImport("Volume.dll")]
        public static extern void changeVolume(double[] amplitudes, double change);
        private string filePath;
        private double[] globalFreq;
        //private double[] globalAmp;
        private double[] copy;
        private double xstart;
        private double xend;
        private WavReader globalWavHdr = new WavReader();
        public Form1()
        {
            InitializeComponent();
            Trace.WriteLine(Function1(1, 2));
            Trace.WriteLine(Function2(1, 2));
            //double[] s = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14};
            //double[] fw = {1, 0.2, 0.2, 0, 1, 0.1, 0.1, 1};
            //double[] samples = Fourier.convolve(s, fw);
            //for (int i = 0; i < samples.Length; i++)
            //{
            //    Trace.WriteLine(i + " " + samples[i]);
            //}
            Trace.WriteLine("Filter Test:");
            double srate = 1000;
            double fcut = 300;
            int size = 16;
            double[] myfilter = Fourier.lowPassFilter(size, fcut, srate);
            Fourier.printSamplesTrace(myfilter);
            myfilter = Fourier.highPassFilter(size, fcut, srate);
            Fourier.printSamplesTrace(myfilter);
            //IntPtr hwnd = Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]);
            //IntPtr hwnd = Marshal.GetHINSTANCE(typeof(Form).Module);
        }

        public double[] readingWave(String fileName)
        {
            byte[] byteArray;
            BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(fileName));
            globalWavHdr.Empty();
            globalWavHdr.ChunkID = reader.ReadInt32();
            globalWavHdr.ChunkSize = reader.ReadInt32();
            globalWavHdr.Format = reader.ReadInt32();
            globalWavHdr.SubChunk1ID = reader.ReadInt32();
            globalWavHdr.SubChunk1Size = reader.ReadInt32();
            globalWavHdr.AudioFormat = reader.ReadUInt16();
            globalWavHdr.NumChannels = reader.ReadUInt16();
            globalWavHdr.SampleRate = reader.ReadUInt32();
            globalWavHdr.ByteRate = reader.ReadUInt32();
            globalWavHdr.BlockAlign = reader.ReadUInt16();
            globalWavHdr.BitsPerSample = reader.ReadUInt16();
            globalWavHdr.SubChunk2ID = reader.ReadInt32();
            globalWavHdr.SubChunk2Size = reader.ReadInt32();
            byteArray = reader.ReadBytes((int)globalWavHdr.SubChunk2Size);
            short[] shortArray = new short[globalWavHdr.SubChunk2Size / globalWavHdr.BlockAlign];
            double[] outputArray;
            for (int i = 0; i < globalWavHdr.SubChunk2Size / globalWavHdr.BlockAlign; i++)
            {
                shortArray[i] = BitConverter.ToInt16(byteArray, i * globalWavHdr.BlockAlign);
            }
            outputArray = shortArray.Select(x => (double)(x)).ToArray();
            reader.Close();
            return outputArray;
        }

        public void OpenFile(string fileName)
        {
            filePath = fileName;
            this.Text = fileName;
            globalFreq = readingWave(filePath);
            //Complex[] complexNumbers = Fourier.DFT(globalFreq, 1000);
            //globalAmp = Fourier.getAmplitudes(complexNumbers);
            plotFreqWaveChart(globalFreq);
        }

        public void plotFreqWaveChart(double[] array)
        {
            chart1.Series["Original"].Points.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = Double.NaN;
            chart1.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            chart1.ChartAreas[0].AxisX.ScaleView.Size = array.Length / 100;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
            for (int i = 0; i < array.Length; i++)
            {
                chart1.Series["Original"].Points.AddXY(i, array[i]);
            }
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
        }

        private void File_Click(object sender, EventArgs e)
        {
            //Basically the menu where you select files
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //add filter
            openFileDialog.Filter = "WAV File (*.wav)|*.wav|All files (*.*)|*.*";
            //check if file open
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenFile(openFileDialog.FileName);
            }
            
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            xstart = chart1.ChartAreas[0].CursorX.SelectionStart;
            xend = chart1.ChartAreas[0].CursorX.SelectionEnd;
            Trace.WriteLine(xstart + "\n" + xend);
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            chart1.Series["Original"].Points.Clear();
        }

        private void Save_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IntPtr hwnd = Marshal.GetHINSTANCE(typeof(Form1).Module);
            DllMain(hwnd);
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            copy = new double[(int) xend - (int) xstart + 1];
            int nums = 0;
            for (int i = (int) xstart; i <= (int) xend; i++)
            {
                copy[nums] = globalFreq[i];
                nums++;
            }
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            for (int i = copy.Length - 1; i > 0; i--)
            {
                chart1.Series["Original"].Points.InsertXY((int)xstart, copy[i]);
            }
        }

        private void Cut_Click(object sender, EventArgs e)
        {
            Copy_Click(sender, e);
            for (int i = (int) xstart; i <= (int) xend; i++)
            {
                chart1.Series["Original"].Points.RemoveAt(i);
            }
        }
    }
}
