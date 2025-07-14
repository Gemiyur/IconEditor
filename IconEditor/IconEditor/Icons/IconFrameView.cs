using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace IconEditor.Icons;

/// <summary>
/// Класс представления кадра значка.
/// </summary>
public class IconFrameView : INotifyPropertyChanged
{
    public string Name => $"{Size} x {Size} ({BitmapSize} x {BitmapSize})";

    public int Size { get; set; }

    public Bitmap Bitmap { get; set; }

    public int BitmapSize => Math.Max(Bitmap.Width, Bitmap.Height);
    
    public BitmapFrame BitmapFrame { get; set; }

    public IconFrameView(Bitmap bitmap)
    {
        Bitmap = bitmap;
        Size = App.NearestImageSize(BitmapSize);
        // Изменение размера для того, чтобы был не оригинальный формат, а MemoryBmp.
        Bitmap = GetResizedBitmap();
        UpdateBitmapFrame();
    }

    public Bitmap GetResizedBitmap()
    {
        int width;
        int height;
        if (Bitmap.Width == Bitmap.Height)
        {
            width = Size;
            height = Size;
        }
        else if (Bitmap.Width > Bitmap.Height)
        {
            width = Size;
            height = (int)((double)Bitmap.Height / Bitmap.Width * Size);
        }
        else
        {
            height = Size;
            width = (int)((double)Bitmap.Width / Bitmap.Height * Size);
        }
        return new Bitmap(Bitmap, width, height);
    }

    public void FlipHorizontal()
    {
        Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
        UpdateBitmapFrame();
    }

    public void FlipVertical()
    {
        Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        UpdateBitmapFrame();
    }

    public void RotateLeft()
    {
        Bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
        UpdateBitmapFrame();
    }

    public void RotateRight()
    {
        Bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        UpdateBitmapFrame();
    }

    public void ResizeBitmap(int size)
    {
        Bitmap = GetResizedBitmap();
    }

    public void UpdateBitmapFrame()
    {
        using (var stream = new MemoryStream())
        {
            Bitmap.Save(stream, ImageFormat.Png);
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BitmapFrame = decoder.Frames[0];
        }
        OnPropertyChanged("BitmapFrame");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string property = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
