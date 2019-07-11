using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SerahToolkit_SharpGL.FF8_Core
{
    class PlayMovie
    {
        public struct MovieClip
        {
            public Resolutions[] Resolutions;
            public uint Frames;
        };

        public struct Resolutions
        {
            public uint Offset;
            public uint Size;
        }

        public MovieClip[] _mClips;
        public static bool bSuccess;
        public int nClips;

        public string path;
        public PlayMovie(string path)
        {
            this.path = path;
            _mClips = new MovieClip[256];
            for (int i = 0; i != 255; i++)
                _mClips[i].Resolutions = new Resolutions[2];
        }

        bool ValidVersion(bool biktype2,byte version)
        {
            if (!biktype2)
                return
                    version == 0x62 ||
                    version == 0x64 ||
                    version == 0x66 ||
                    version == 0x67 ||
                    version == 0x68 ||
                    version == 0x69;
            return
                version == 0x61 ||
                version == 0x64 ||
                version == 0x66 ||
                version == 0x67 ||
                version == 0x68 ||
                version == 0x69;
        }
        public void Read()
        {
            if (path == null)
                return;
            const int _3B_MASK = 0xFFFFFF;
            const int F8P = 0x503846;
            const int BIK = 0x4B4942;
            const int KB2 = 0X32424B; // BIK version 2.
            FileStream fs = new FileStream(path, FileMode.Open,FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            uint len = (uint)fs.Length;
            nClips = 0;
            while (fs.Position < len)
            {
                //START OF CAM
                uint header = br.ReadUInt32() & _3B_MASK; // are first 3 bytes F8P
                Debug.Assert(header == F8P);
                fs.Seek(2, SeekOrigin.Current);
                _mClips[nClips].Frames = br.ReadUInt16();
                fs.Seek((_mClips[nClips].Frames+1)*0x2C, SeekOrigin.Current);
                while (br.ReadByte() == 0xFF)
                {
                    fs.Seek(0x2B, SeekOrigin.Current);
                }
                fs.Seek(-1, SeekOrigin.Current);
                //START OF BIK 1
                byte[] tmp = br.ReadBytes(4);
                header = (uint)(tmp[2] << 16 | tmp[1] << 8 | tmp[0]);
                byte version = tmp[3];
                Debug.Assert(header == KB2 || header == BIK);
                Debug.Assert(ValidVersion(header == KB2, version));
                //header = br.ReadUInt32() & _3B_MASK;
                //if (header != BIK)
                //    return;

                _mClips[nClips].Resolutions[0].Offset = (uint)fs.Position-4;
                _mClips[nClips].Resolutions[0].Size = br.ReadUInt32();
                _mClips[nClips].Frames = br.ReadUInt32();
                _mClips[nClips].Resolutions[0].Size += 8;

                //START OF BIK 2
                //header = br.ReadUInt32() & _3B_MASK;
                fs.Seek(_mClips[nClips].Resolutions[0].Size -12, SeekOrigin.Current);
                tmp = br.ReadBytes(4);
                header = (uint)(tmp[2] << 16 | tmp[1] << 8 | tmp[0]);
                version = tmp[3];
                Debug.Assert(header == KB2 || header == BIK);
                Debug.Assert(ValidVersion(header == KB2, version));
                _mClips[nClips].Resolutions[1].Offset = (uint)fs.Position - 4;
                _mClips[nClips].Resolutions[1].Size = br.ReadUInt32();
                _mClips[nClips].Resolutions[1].Size += 8;

                //NEXT VIDEO
                fs.Seek(_mClips[nClips].Resolutions[1].Size - 8, SeekOrigin.Current);
                nClips++;
            }
            bSuccess = true;
            br.Dispose();
            fs.Dispose();
        }

        private static string BuildPath(byte MovieID)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"Movies/");
            sb.Append("disc");
            sb.Append(MovieID <= 30 ? "00" :
                 MovieID > 30 && MovieID <= 30 + 34 ? "01" :
                 MovieID > 30 + 34 && MovieID <= 64 + 32 ? "02" :
                 MovieID > 64 + 32 && MovieID <= 64 + 32 + 7 ? "04" : "01");
            sb.Append(@"_");
            string temp;
            if (MovieID > 9)
                temp = MovieID.ToString();
            else
                temp = "0" + MovieID.ToString();
            sb.Append(temp + @"h");

            return sb.ToString();
        }
    }
}
