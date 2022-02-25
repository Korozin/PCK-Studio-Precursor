using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MinecraftUSkinEditor
{
    public class PCK
    {

        public class MineFile
        {
            public int filesize;
            public int type;
            public string name;
            public byte[] data;
            public List<object[]> entries = new List<object[]>();
        }

        public Dictionary<int,string> types = new Dictionary<int, string>();
        public Dictionary<string, int> typeCodes = new Dictionary<string, int>();
        public List<MineFile> mineFiles = new List<MineFile>();

        public PCK()
        {

        }

        public PCK(string filename)
        {
            Read(File.ReadAllBytes(filename));
        }

        private static byte[] endianReverseUnicode(byte[] str)
        {
            byte[] newStr = new byte[str.Length];
            for(int i = 0; i < str.Length; i += 2)
            {
                newStr[i] = str[i + 1];
                newStr[i + 1] = str[i];
            }
            return newStr;
        }

        public static string readMineString(FileData f)
        {
            int length = f.readInt() * 2;
            return Encoding.Unicode.GetString(endianReverseUnicode(f.readBytes(length)));
        }

        public void Read(byte[] data)
        {
            FileData f = new FileData(data);
            f.Endian = Endianness.Big;

            int versionNumber = f.readInt();
            int entryTypeCount = f.readInt();

            for (int i = 0; i < entryTypeCount; i++)
            {
                int unk = f.readInt();
                string temp = readMineString(f);
                types.Add(unk, temp);
                typeCodes.Add(temp, unk);
                f.skip(4);
            }

            int itemCount = f.readInt();

            for (int i = 0; i < itemCount; i++)
            {
                MineFile mf = new MineFile();
                mf.filesize = f.readInt();
                mf.type = f.readInt();
                int length = f.readInt() * 2;
                mf.name = Encoding.Unicode.GetString(endianReverseUnicode(f.readBytes(length)));
                f.skip(4);
                mineFiles.Add(mf);
            }

            foreach(MineFile mf in mineFiles)
            {
                int entryCount = f.readInt();
                for(int i = 0; i < entryCount; i++)
                {
                    object[] temp = new object[2];
                    int entryTypeInt = f.readInt();
                    temp[0] = types[entryTypeInt]; //Entry type
                    temp[1] = readMineString(f);  //Entry data
                    f.skip(4);
                    mf.entries.Add(temp);
                }
                mf.data = f.readBytes(mf.filesize);    
            }
        }

        private static void writeMinecraftString(FileOutput f, string str)
        {
            byte[] d = Encoding.Unicode.GetBytes(str);
            f.writeInt(d.Length / 2);
            f.writeBytes(endianReverseUnicode(d));
            f.writeInt(0);
        }

        public byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeInt(3);
            f.writeInt(types.Count);
            foreach (int type in types.Keys)
            {
                f.writeInt(type);
                writeMinecraftString(f, types[type]);
            }

            f.writeInt(mineFiles.Count);
            foreach (MineFile mf in mineFiles)
            {
                f.writeInt(mf.data.Length);
                f.writeInt(mf.type);
                writeMinecraftString(f, mf.name);
            }

            foreach (MineFile mf in mineFiles)
            {
                f.writeInt(mf.entries.Count);
                foreach (object[] entry in mf.entries)
                {
                    f.writeInt(typeCodes[(string)entry[0]]);
                    writeMinecraftString(f, (string)entry[1]);
                }

                f.writeBytes(mf.data);
            }

            return f.getBytes();
        }
    }
}
