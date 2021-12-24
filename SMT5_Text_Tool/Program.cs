using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMT5_Text_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                var dirpath = Path.GetDirectoryName(args[0]);
                var filename = Path.GetFileName(args[0]);
                var nonext = Path.GetFileNameWithoutExtension(args[0]);
                var ext = Path.GetExtension(args[0]);

                if(ext == ".uexp")
                {
                    var rd = new BinaryReader(File.OpenRead(args[0]));
                    if (!File.Exists(dirpath + "\\" + filename + "_bk"))
                    {
                        var cp = new BinaryWriter(File.Create(dirpath + "\\" + filename + "_bk"));
                        cp.Write(rd.ReadBytes((int)rd.BaseStream.Length));
                        cp.Close();
                        rd.BaseStream.Seek(0, SeekOrigin.Begin);
                    }

                    rd.BaseStream.Seek(0x21, SeekOrigin.Current);
                    int count = rd.ReadInt32();
                    rd.BaseStream.Seek(0x31, SeekOrigin.Current);

                    var wt = new StreamWriter(dirpath + "\\" + nonext + ".txt");

                    for(int i = 0; i< count; i++)
                    {
                        rd.BaseStream.Seek(0x91, SeekOrigin.Current);
                        int len = rd.ReadInt32();

                        if(len > 0)
                        {
                            string text = Encoding.UTF8.GetString(rd.ReadBytes(len)).Replace("\0", "[0]").Replace("\r", "[r]").Replace("\n", "[n]");
                            wt.WriteLine(text);
                        }
                        else
                        {
                            string text = Encoding.Unicode.GetString(rd.ReadBytes(len * -2)).Replace("\0", "[0]").Replace("\r", "[r]").Replace("\n", "[n]");
                            wt.WriteLine(text);
                        }
                        rd.BaseStream.Seek(0x140, SeekOrigin.Current);
                    }
                    wt.Close();
                }

                if(ext == ".txt")
                {
                    string[] lines = File.ReadAllLines(args[0]);
                    var rd = new BinaryReader(File.OpenRead(dirpath + "\\" + nonext + ".uexp_bk"));
                    var wt = new BinaryWriter(File.Create(dirpath + "\\" + nonext + ".uexp"));

                    wt.Write(rd.ReadBytes(0x56));

                    foreach(var line in lines)
                    {
                        wt.Write(rd.ReadBytes(0x4E));
                        var data = Encoding.Unicode.GetBytes(line.Replace("[0]", "\0").Replace("[r]", "\r").Replace("[n]", "\n"));
                        wt.Write((Int64)(0x36 + data.Length));
                        rd.ReadBytes(8);
                        wt.Write(rd.ReadBytes(0x3B));
                        wt.Write(data.Length / -2);
                        wt.Write(data);
                        int len = rd.ReadInt32();
                        if (len > 0)
                            rd.ReadBytes(len);
                        else
                            rd.ReadBytes(len * -2);
                        wt.Write(rd.ReadBytes(0x140));
                    }
                    wt.Write(rd.ReadBytes(0x10));

                    wt.BaseStream.Seek(0x10, SeekOrigin.Begin);
                    wt.Write((Int64)(wt.BaseStream.Length - 0x31));
                    wt.BaseStream.Seek(0x1D, SeekOrigin.Current);
                    wt.Write((Int64)(wt.BaseStream.Length - 0x66));

                    long allbytes = wt.BaseStream.Length;

                    wt.Close();

                    wt = new BinaryWriter(File.OpenWrite(dirpath + "\\" + nonext + ".uasset"));

                    wt.BaseStream.Seek(-92, SeekOrigin.End);
                    wt.Write((Int64)(allbytes - 4));
                    wt.Close();
                }
            }
        }
    }
}
