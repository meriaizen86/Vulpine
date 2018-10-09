using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Vulpine.Utils
{
    public class ImageBuilder : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Image Image => Bitmap;
        BitmapData BitmapData = null;
        public bool CanEdit => BitmapData != null;

        void _Constructor(Bitmap bmp, Rectangle rect)
        {
            Bitmap = bmp;
            BitmapData = Bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        }

        public ImageBuilder(Bitmap bmp)
        {
            _Constructor(bmp, new Rectangle(Point.Empty, bmp.Size));
        }

        public ImageBuilder(Bitmap bmp, Rectangle rect)
        {
            _Constructor(bmp, rect);
        }

        public ImageBuilder(Image image)
        {
            _Constructor(new Bitmap(image), new Rectangle(Point.Empty, image.Size));
        }

        public ImageBuilder(Image image, Rectangle rect)
        {
            _Constructor(new Bitmap(image), rect);
        }

        ~ImageBuilder()
        {
            Dispose();
        }

        public Color GetPixel(Point pos)
        {
            return GetPixel(pos.X, pos.Y);
        }

        public Color GetPixel(int x, int y)
        {
            if (BitmapData == null)
                throw new InvalidOperationException("ImageBuilder object is disposed or applied");

            if (x < 0 | x >= Bitmap.Width)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 | y >= Bitmap.Height)
                throw new ArgumentOutOfRangeException("y");
            unsafe
            {
                var ptr = (int*)(BitmapData.Scan0 + (y * BitmapData.Width + x) * 4);
                return Color.FromArgb(
                    *ptr
                );
            }
        }

        public void SetPixel(Point pos, Color color)
        {
            SetPixel(pos.X, pos.Y, color);
        }

        public void SetPixel(int x, int y, Color color)
        {
            if (BitmapData == null)
                throw new InvalidOperationException("ImageBuilder object is disposed or applied");

            if (x < 0 | x >= Bitmap.Width)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 | y >= Bitmap.Height)
                throw new ArgumentOutOfRangeException("y");
            unsafe
            {
                var ptr = (int*)(BitmapData.Scan0 + (y * BitmapData.Width + x) * 4);
                *ptr = color.ToArgb();
            }
        }

        public void SetData(Color[] colors)
        {
            if (BitmapData == null)
                throw new InvalidOperationException("ImageBuilder object is disposed or applied");

            unsafe
            {
                var ptr = (int*)BitmapData.Scan0;
                var count = Math.Min(Bitmap.Width * BitmapData.Height, colors.Length);
                for (var i = 0; i < count; i++)
                {
                    *ptr = colors[i].ToArgb();
                    ptr++;
                }
            }
        }

        public void SetData(byte[] data)
        {
            if (BitmapData == null)
                throw new InvalidOperationException("ImageBuilder object is disposed or applied");

            unsafe
            {
                var ptr = (byte*)BitmapData.Scan0;
                var count = Math.Min(Bitmap.Width * BitmapData.Height * 4, data.Length);
                for (var i = 0; i < count; i++)
                {
                    *(ptr++) = data[i];
                }
            }
        }

        public void SetData(IntPtr data, long size)
        {
            if (BitmapData == null)
                throw new InvalidOperationException("ImageBuilder object is disposed or applied");

            unsafe
            {
                var ptr = (byte*)BitmapData.Scan0;
                var src = (byte*)data;
                var count = Math.Min((long)Bitmap.Width * BitmapData.Height * 4L, size);
                for (var i = 0L; i < count; i++)
                {
                    *(ptr++) = *(src++);
                }
            }
        }

        public byte[] GetData()
        {
            if (BitmapData == null)
                throw new InvalidOperationException("ImageBuilder object is disposed or applied");

            unsafe
            {
                var ptr = (byte*)BitmapData.Scan0;
                var count = Bitmap.Width * BitmapData.Height * 4;
                var data = new byte[count];
                for (var i = 0; i < count; i++)
                {
                    data[i] = *(ptr++);
                }
                return data;
            }
        }

        public void Finish()
        {
            if (BitmapData == null)
                return;

            Bitmap.UnlockBits(BitmapData);
            BitmapData = null;
        }

        public void Dispose()
        {
            if (BitmapData == null)
                return;

            GC.SuppressFinalize(this);
            Finish();
        }

        public static implicit operator Bitmap(ImageBuilder self)
        {
            return self.Bitmap;
        }

        public static implicit operator Image(ImageBuilder self)
        {
            return self.Image;
        }
    }
}
