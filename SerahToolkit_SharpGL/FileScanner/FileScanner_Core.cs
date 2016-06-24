﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;

namespace SerahToolkit_SharpGL.FileScanner
{
    class FileScanner_Core
    {
        private string _path;
        private List<string> _listOfFiles;

        public FileScanner_Core(string path)
        {
            _path = path;
        }

        public void Start()
        {
            if (_path == null)
                throw new Exception("FileScanner not initialized or not path specified");

            _listOfFiles = new List<string>();
            _listOfFiles = Directory.EnumerateFiles(_path).ToList();
            
        }

        private void Check(string file)
        {
            var bytes = Header64(file);
            if (bytes == null)
                return;
            foreach (FileScanner_Headers fsh in Enum.GetValues(typeof(FileScanner_Headers)))
            {
                long? b64, b128 = null; //Nullable<T>
                var perform = BigInteger.Parse(fsh.ToString()).ToByteArray();
                if (fsh.ToString().Length > 8)
                    b128 = BitConverter.ToInt64(perform, 8);
                b64 = BitConverter.ToInt64(perform, 0);
                
                if(b128 != null)
                    if(bytes.Item1 == b64 && bytes.Item2 == b128)
                        Console.WriteLine($"{Path.GetFileName(file)} is {Enum.GetName(typeof(FileScanner_Headers),fsh)}");
                else
                        if(bytes.Item1 == b64)
                        Console.WriteLine($"{Path.GetFileName(file)} is {Enum.GetName(typeof(FileScanner_Headers), fsh)}");
            }
        }

        private Tuple<Int64, Int64> Header64(string file)
        {
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        return new Tuple<long, long>(br.ReadInt64(), br.ReadInt64());
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error with: {Path.GetFileName(file)}, {e.ToString()}");
                return null;
            }
        }
    }
}
