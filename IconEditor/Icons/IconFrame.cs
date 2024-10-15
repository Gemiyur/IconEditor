using System.IO;

namespace IconEditor.Icons
{
    /// <summary>
    /// Класс кадра значка.
    /// </summary>
    public class IconFrame
    {
        public byte Width;
        public byte Height;
        public byte ColorCount = 0;
        public short Planes = 0;
        public short BitsPerPixel = 32;
        public int Bytes;
        public byte[] Image;

        public IconFrame(int width, int height, int bytes, byte[] image)
        {
            Width = (byte)width;
            Height = (byte)height;
            Bytes = bytes;
            Image = image;
        }

        public IconFrame(BinaryReader Reader)
        {
            Width = Reader.ReadByte();
            Height = Reader.ReadByte();
            ColorCount = Reader.ReadByte();
            Reader.ReadByte();
            Planes = Reader.ReadInt16();
            BitsPerPixel = Reader.ReadInt16();
            Bytes = Reader.ReadInt32();
            Reader.ReadInt32();
        }

        public void ReadImage(BinaryReader Reader) => Image = Reader.ReadBytes(Bytes);

        public void Write(BinaryWriter Writer)
        {
            Writer.Write(Width);
            Writer.Write(Height);
            Writer.Write(ColorCount);
            Writer.Write((byte)0);
            Writer.Write(Planes);
            Writer.Write(BitsPerPixel);
            Writer.Write(Bytes);
        }
    }
}
