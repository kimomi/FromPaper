namespace PerlinNoise
{
    using BumpKit;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading;

    class Program
    {
        private static Color color1 = Color.Yellow;
        private static Color color2 = Color.Purple;

        private static Color color3 = Color.FromArgb(200, 200, 200);
        private static Color color4 = Color.FromArgb(50, 50, 50);

        static Color NormalPerLin(float x, float y)
        {
            var col = PerlinNoise.Noise01(x, y, 0);
            return PerlinNoise.Lerp(color1, color2, col);
        }

        static Color MarblePerLin(float x, float y, float z)
        {
            var t = PerlinNoise.Noise01(x, y, z);

            t = t * t;

            var a = 0.2f;
            var b = 0.5f;
            if (t > a)
            {
                var k = (1 - b) / (1.0f - a);
                t = t * k + 1 - k;
            }
            else
            {
                t = b / a * t;
            }
            return PerlinNoise.Lerp(color4, color3, t * t);
        }

        static void Main(string[] args)
        {
            using (FileStream fs = new FileStream("result.gif", FileMode.Create))
            {
                using (var encoder = new GifEncoder(fs))
                {
                    encoder.FrameDelay = System.TimeSpan.FromSeconds(1 / 60.0f);
                    var num = 0;
                    while (num < 200)
                    {
                        encoder.AddFrame(GetImage(num * 0.02f));
                        num++;
                    }
                }
            }
        }

        static Image GetImage(float movex)
        {
            var len = 1024;
            var image = new Bitmap(len, len);

            for (var i = 0; i < len; i++)
            {
                for (var j = 0; j < len; j++)
                {
                    var x = i / (float)len * 6;
                    var y = j / (float)len * 6;
                    image.SetPixel(i, j, MarblePerLin(x, y, movex));
                }
            }

            return image;
        }
    }
}
