using System;
using System.IO;
using System.Text;
using CNUSPACKER.contents;
using CNUSPACKER.fst;
using CNUSPACKER.utils;

namespace CNUSPACKER.packaging
{
    public class FST
    {
        private static readonly byte[] magicbytes = { 0x46, 0x53, 0x54, 0x00 };
        private const int unknown = 0x20;
        private int contentCount;

        public readonly Contents contents;
        public readonly FSTEntries fileEntries = new FSTEntries();

        private static readonly MemoryStream strings = new MemoryStream();

        public static int curEntryOffset;

        private byte[] alignment;

        public FST(Contents contents)
        {
            this.contents = contents;
        }

        public void Update()
        {
            strings.SetLength(0);
            curEntryOffset = 0;

            contents.ResetFileOffsets();
            fileEntries.Update();
            contents.Update(fileEntries);
            fileEntries.GetRootEntry().SetEntryCount(fileEntries.GetFSTEntryCount());

            contentCount = contents.GetContentCount();
        }

        public static int GetStringPosition()
        {
            return (int)strings.Position;
        }

        public static void AddString(string filename)
        {
            strings.Write(Encoding.ASCII.GetBytes(filename));
            strings.WriteByte(0x00);
        }

        public byte[] GetAsData()
        {
            MemoryStream buffer = new MemoryStream(GetDataSize());
            byte[] temp;

            buffer.Write(magicbytes);
            temp = BitConverter.GetBytes(unknown);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(contentCount);
            Array.Reverse(temp);
            buffer.Write(temp);
            buffer.Seek(20, SeekOrigin.Current);
            buffer.Write(contents.GetFSTContentHeaderAsData());
            buffer.Write(fileEntries.GetAsData());
            buffer.Write(strings.ToArray());
            buffer.Write(alignment);

            return buffer.GetBuffer();
        }

        public int GetDataSize()
        {
            int size = 0;
            size += magicbytes.Length;
            size += 0x04; // unknown
            size += 0x04; // contentCount
            size += 20; // padding
            size += contents.GetFSTContentHeaderDataSize();
            size += fileEntries.GetDataSize();
            size += (int)strings.Position;
            int newsize = (int)Utils.Align(size, 0x8000);
            alignment = new byte[newsize - size];

            return newsize;
        }
    }
}
