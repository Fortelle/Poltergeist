using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Poltergeist.Common.Utilities.Images;

public class PixelData
{
    public byte[] Source;
    public int Width;
    public int Height;

    public PixelData(byte[] source, int stroke)
    {
        Source = source;
        Width = stroke;
        Height = source.Length / 4 / stroke;
    }

    public PixelData(byte[] source, int width, int height)
    {
        Source = source;
        Width = width;
        Height = height;
    }

    public int GetOffset(int x, int y)
    {
        return (y * Width + x) * 4;
    }

    public Color GetColor(int x, int y)
    {
        var i = GetOffset(x, y);
        var color = Color.FromArgb(Source[i + 3], Source[i + 2], Source[i + 1], Source[i + 0]);
        return color;
    }

    public IEnumerable<int> Iterator(Rectangle rect)
    {
        for (var y = rect.Top; y < rect.Bottom; y++)
        {
            for (var x = rect.Left; x < rect.Right; x++)
            {
                yield return (y * rect.Width + x) * 4;
            }
        }
    }

    public IEnumerable<int> Iterator()
    {
        for (var i = 0; i < Source.Length; i += 4)
        {
            yield return i;
        }
    }

    public IEnumerable<TResult> ColorIterator<TResult>(Func<byte, byte, byte, byte, TResult> selector)
    {
        for (var i = 0; i < Source.Length; i += 4)
        {
            yield return selector(Source[i + 2], Source[i + 1], Source[i], Source[i + 3]);
        }
    }

    public IEnumerable<TResult> ColorIterator<TResult>(Func<byte, byte, byte, TResult> selector)
    {
        for (var i = 0; i < Source.Length; i += 4)
        {
            yield return selector(Source[i + 2], Source[i + 1], Source[i]);
        }
    }

    public void ColorIterator(Action<byte, byte, byte> selector)
    {
        for (var i = 0; i < Source.Length; i += 4)
        {
            selector(Source[i + 2], Source[i + 1], Source[i]);
        }
    }

    public void ColorIterator(Action<byte, byte, byte, byte> selector)
    {
        for (var i = 0; i < Source.Length; i += 4)
        {
            selector(Source[i + 2], Source[i + 1], Source[i], Source[i + 3]);
        }
    }

    public static PixelData FromBitmap(Bitmap bmp)
    {
        var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
        var byteCount = bitmapData.Stride * bmp.Height;
        var pixels = new byte[byteCount];
        Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);
        bmp.UnlockBits(bitmapData);

        var data = new PixelData(pixels, bmp.Width, bmp.Height);
        return data;
    }

    public Bitmap ToBitmap()
    {
        var bmp = new Bitmap(Width, Height);
        var bitmapData = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
        var byteCount = bitmapData.Stride * bitmapData.Height;
        Marshal.Copy(Source, 0, bitmapData.Scan0, byteCount);
        bmp.UnlockBits(bitmapData);
        return bmp;
    }

    public void Binarize(Func<byte, byte, byte, bool> func)
    {
        foreach (var i in Iterator())
        {
            var value = func(Source[i + 2], Source[i + 1], Source[i]) ? (byte)0 : (byte)255;
            Source[i] = value;
            Source[i + 1] = value;
            Source[i + 2] = value;
        }
    }

    public void Grayscale(Func<byte, byte, byte, byte> func)
    {
        foreach (var i in Iterator())
        {
            var value = func(Source[i + 2], Source[i + 1], Source[i]);
            Source[i] = value;
            Source[i + 1] = value;
            Source[i + 2] = value;
        }
    }

}
