﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


namespace SerahToolkit_SharpGL
{
    internal partial class Parser : Form
    {
        private string[] _file ;
        private List<float> _x;
        private List<float> _y;
        private List<float> _z;   
        private List<double> _u;
        private List<double> _uPixel; 
        private List<double> _v;
        private List<double> _vPixel;
        private List<ushort> _a;
        private List<int> _at;
        private List<int> _bt;
        private List<int> _ct; 
        private List<ushort> _b;
        private List<ushort> _c;   

        private readonly int _height;
        private readonly int _width;

        private readonly byte[] _start = {0x01, 0x00, 0x01, 0x00};
        private byte[] _verticesCount;
        private byte[] _trianglesCount;
        private List<byte[]> _vertices;
        private List<byte[]> _polygon;
        private int[] _page; 

        private readonly Dictionary<decimal, byte[]> _clut = new Dictionary<decimal, byte[]>
        {
            { 0,new byte[] {0x00, 0x3C} },
            { 1,new byte[] {0x40, 0x3C} },
            { 2,new byte[] {0x80, 0x3C} },
            { 3,new byte[] {0xC0, 0x3C} },
            { 4,new byte[] {0x00, 0x3D} },
            { 5,new byte[] {0x40, 0x3D} },
            { 6,new byte[] {0x80, 0x3D} },
            { 7,new byte[] {0xC0, 0x3D} },
            { 8,new byte[] {0x00, 0x3E} },
            { 9,new byte[] {0x40, 0x3E} },
            { 10,new byte[] {0x80, 0x3E} },
            { 11,new byte[] {0xC0, 0x3E} },
            { 12,new byte[] {0x00, 0x3F} },
            { 13,new byte[] {0x40, 0x3F} },
            { 14,new byte[] {0x80, 0x3F} },
            { 15,new byte[] {0xC0, 0x3F} },
        };

        public Parser(int segment, int height, int width)
        {
            InitializeComponent();
            Text = segment.ToString();
            _height = height;
            _width = width;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {Filter = "Wavefront OBJ file .obj|*.obj"};
            if (ofd.ShowDialog() != DialogResult.OK) return;
            _file = File.ReadAllLines(ofd.FileName);
            richTextBox1.Clear();
            richTextBox1.AppendText("Opened: " + ofd.FileName);
            Process();
        }

        private void Process()
        {
            _x = new List<float>(); _y = new List<float>(); _z = new List<float>();
            _u = new List<double>(); _v = new List<double>();
            _uPixel = new List<double>(); _vPixel = new List<double>();
            _a = new List<ushort>(); _b = new List<ushort>(); _c = new List<ushort>();
            _at = new List<int>(); _bt = new List<int>(); _ct = new List<int>();
            foreach (var s in _file)
            {
                if (s.StartsWith("v "))
                {
                    string[] temp = s.Replace(".", ",").Split(' ');
                    _x.Add(float.Parse(temp[1])); _y.Add(float.Parse(temp[2]));
                    _z.Add(float.Parse(temp[3]));
                }
                else if (s.StartsWith("vt "))
                {
                    string[] temp = s.Replace(".", ",").Split(' ');
                    _u.Add(double.Parse(temp[1])); _v.Add(double.Parse(temp[2]));
                }
                else if (s.StartsWith("f "))
                {
                    string[] temp = s.Split(' '); // F[0] 1/1[1] 2/2[2] 3/3[3] 4/4[UNUSED]
                    string[] aa = temp[1].Split('/'); // A/T
                    _a.Add(ushort.Parse(aa[0]));
                    _at.Add(int.Parse(aa[1]));
                    string[] bb = temp[2].Split('/'); // B/T
                    _b.Add(ushort.Parse(bb[0]));
                    _bt.Add(int.Parse(bb[1]));
                    string[] cc = temp[3].Split('/'); // C/T
                    _c.Add(ushort.Parse(cc[0]));
                    _ct.Add(int.Parse(cc[1]));
                }
            }
            _vertices = new List<byte[]>();
            ushort vertCount = Convert.ToUInt16(_x.Count);
            _verticesCount = new byte[2];
            _verticesCount = BitConverter.GetBytes(vertCount);
            richTextBox1.AppendText(Environment.NewLine + "Vertices: " + vertCount);
            richTextBox1.AppendText(Environment.NewLine + "VT: " + _u.Count);
            richTextBox1.AppendText(Environment.NewLine + "Triangles: " + _c.Count);

            for (int i = 0; i != _x.Count; i++)
            {
                _x[i] = _x[i] * 2000.0f; _y[i] = _y[i] * 2000.0f;
                _z[i] = _z[i] * 2000.0f;
                double d = Math.Round(_x[i]); short xs = short.Parse(d.ToString(CultureInfo.InvariantCulture));
                d = Math.Round(_y[i]); short ys = short.Parse(d.ToString(CultureInfo.InvariantCulture));
                d = Math.Round(_z[i]); short zs = short.Parse(d.ToString(CultureInfo.InvariantCulture));
                byte[] vertex = new byte[6];
                Buffer.BlockCopy(BitConverter.GetBytes(xs), 0, vertex, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(ys), 0, vertex, 2, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(zs), 0, vertex, 4, 2);
                _vertices.Add(vertex);
            }

            richTextBox1.AppendText(Environment.NewLine + "Vertices calculated and converted to FF8 format");
            _page = new int[_u.Count];
            for (int i = 0; i != _u.Count; i++)
            {
                _uPixel.Add(Math.Round(((_u[i]*100.0d)*_height)/100.0d));
                _vPixel.Add(Math.Round(((_v[i]*100.0d)*_width)/100.0d));
                
                while (true)
                {
                    if (_uPixel[i] <= 128.0d)
                        break;

                    _uPixel[i] -= 128.0d;
                    _page[i]++; 
                }
            }
            richTextBox1.AppendText(Environment.NewLine + "TPaging calculated and parsed");
            richTextBox1.AppendText(Environment.NewLine + "Mapping was calculated and parsed");
            _polygon = new List<byte[]>();
            ushort tempTrianglesCount = Convert.ToUInt16(_a.Count);
            _trianglesCount = BitConverter.GetBytes(tempTrianglesCount);
            const byte pagePreOperand = 0xB0;

            for (int i = 0; i != tempTrianglesCount; i++)
            {
                byte[] triangle = new byte[20];
                Buffer.BlockCopy(BitConverter.GetBytes(_a[i]-1),0,triangle,0,2); //A
                Buffer.BlockCopy(BitConverter.GetBytes(_b[i]-1), 0, triangle, 2, 2); //B
                Buffer.BlockCopy(BitConverter.GetBytes(_c[i]-1), 0, triangle, 4, 2); //C
                Buffer.BlockCopy(BitConverter.GetBytes(_u[_at[i]-1]),0,triangle,6,1); //U1
                Buffer.BlockCopy(BitConverter.GetBytes(_v[_at[i]-1]), 0, triangle, 7, 1); //V1
                Buffer.BlockCopy(BitConverter.GetBytes(_u[_bt[i]-1]), 0, triangle, 8, 1); //U2
                Buffer.BlockCopy(BitConverter.GetBytes(_v[_bt[i]-1]), 0, triangle, 9, 1); //V2
                Buffer.BlockCopy(_clut[numericUpDown1.Value],0,triangle,10,2); //CLUTid
                Buffer.BlockCopy(BitConverter.GetBytes(_u[_ct[i]-1]), 0, triangle, 12, 1); //U3
                Buffer.BlockCopy(BitConverter.GetBytes(_v[_ct[i]-1]), 0, triangle, 13, 1); //V3
                byte[] tempTp = BitConverter.GetBytes(_page[_at[i] - 1]);
                var a = tempTp[0] | pagePreOperand; //BITwise 0xB0 OR 0x0? = 0xB?
                triangle[14] = Convert.ToByte(a & 0xFF); // TPage Bitwised with 0xB0 (UNKNOWN)
                //Buffer.BlockCopy(ARRAY[here], 0, triangle, 14, 1); //TPAGE !!!!
                //PASS bHide = 0, and triangle[15] is NULL (00);
                triangle[16] = 0x80; triangle[17] = 0x80; triangle[18] = 0x80; // R G B
                triangle[19] = 0x2c; //PSOne GPU
                _polygon.Add(triangle);
            }
            richTextBox1.AppendText(Environment.NewLine + "Triangle data forged succesfully");
            richTextBox1.AppendText(Environment.NewLine + "Preparing to save segment...");
            SaveFileDialog sfd = new SaveFileDialog {Filter = "Segment file used to replace *.xBIN|*.xBIN"};
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Append);
                fs.Write(_start, 0, _start.Length);
                fs.Write(_verticesCount, 0, _verticesCount.Length);
                foreach (byte[] b in _vertices)
                    fs.Write(b, 0, b.Length);
                fs.Write(CalculatePadding(4), 0, CalculatePadding(4).Length);
                fs.Write(_trianglesCount, 0, 2);
                fs.Write(new byte[6], 0, 2);
                foreach (var variable in _polygon)
                    fs.Write(variable, 0, 20);
                richTextBox1.AppendText("Succesfully saved " + fs.Length + " bytes.");
                fs.Close();
            }
            else
                richTextBox1.AppendText("Save segment failed");
        }

        private static byte[] CalculatePadding(int globalOffset)
        {
            switch (globalOffset%4)
            {
                case 0:
                    return new byte[2];
                case 1:
                    return new byte[3];
                    
                case 2:
                    return new byte[4];
                    
                case 3:
                    return new byte[5];
                    
                case 4:
                    return new byte[6];
                    
                default:
                    return new byte[2];
            }
        }
    }
}
