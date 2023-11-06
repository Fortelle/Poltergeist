using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Poltergeist.Common.Utilities.Images;

public class BitmapUtil
{
    public static Bitmap Crop(Bitmap bmpSrc, Rectangle rectSrc)
    {
        var bmp = new Bitmap(rectSrc.Width, rectSrc.Height, PixelFormat.Format32bppRgb);
        using var gra = Graphics.FromImage(bmp);
        gra.DrawImage(bmpSrc, new Rectangle(0, 0, rectSrc.Width, rectSrc.Height), rectSrc, GraphicsUnit.Pixel);
        return bmp;
    }


    public static Bitmap Resize(Bitmap oldImage, Size newSize)
    {
        var newBmp = new Bitmap(newSize.Width, newSize.Height);
        using var gra = Graphics.FromImage(newBmp);
        //gra.InterpolationMode = InterpolationMode.High;
        //gra.CompositingQuality = CompositingQuality.HighQuality;
        gra.InterpolationMode = InterpolationMode.NearestNeighbor;
        gra.PixelOffsetMode = PixelOffsetMode.Half;
        gra.SmoothingMode = SmoothingMode.AntiAlias;

        gra.DrawImage(oldImage, new Rectangle(default, newSize));
        return newBmp;
    }


    public static Bitmap Rotate(Bitmap bmp, float angle, Rectangle? crop = null)
    {
        int x, y, w, h;
        if (crop == null)
        {
            x = 0;
            y = 0;
            w = bmp.Width;
            h = bmp.Height;
        }
        else
        {
            x = -crop.Value.X;
            y = -crop.Value.Y;
            w = crop.Value.Width;
            h = crop.Value.Height;
        }
        var rotatedImage = new Bitmap(w, h);
        using (var g = Graphics.FromImage(rotatedImage))
        {
            g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
            g.RotateTransform(angle);
            g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
            g.DrawImage(bmp, new Point(x, y));
        }
        return rotatedImage;
    }



    public static Bitmap PureColor(Size size, Color color)
    {
        var bmp = new Bitmap(size.Width, size.Height);
        using var gra = Graphics.FromImage(bmp);
        using var bru = new SolidBrush(color);
        var rect = new Rectangle(0, 0, size.Width, size.Height);
        gra.FillRectangle(bru, rect);
        return bmp;
    }

}
