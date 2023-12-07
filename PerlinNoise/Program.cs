using System;

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
            var coordImge = GetImage((x, y) => {
                if (MathF.Abs(x - MathF.Floor(x)) < 0.05f || MathF.Abs(y - MathF.Floor(y)) < 0.05f)
                {
                    return 0.0f;
                }
                else
                {
                    return 1.0f;
                }
            });
            coordImge.Save("coordImge.jpg");

            // 第一种噪声（随机噪声）
            var image = GetImage((x, y) => PerlinNoise.GetNoise1(x, y));
            image.Save("getnoise1.jpg");

            // 第二种噪声（连续随机噪声）
            var image2 = GetImage((x, y) => PerlinNoise.GetNoise2(x, y));
            image2.Save("getnoise2.jpg");

            // 第三种噪声（平滑连续随机噪声）
            var image3 = GetImage((x, y) => PerlinNoise.GetNoise3(x, y));
            image3.Save("getnoise3.jpg");

            // 第四种噪声（Perlin Noise伪）
            var image4 = GetImage((x, y) => PerlinNoise.GetNoise4(x, y));
            image4.Save("getnoise4.jpg");

            // 第五种噪声（Perlin Noise）
            var image5 = GetImage((x, y) => (PerlinNoise.Noise(x, y, 0)  + 1) / 2);
            image5.Save("getnoise5.jpg");

            // 做一个perlin noise 的动图
            WriteGif();
        }

        static void WriteGif()
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

        static Image GetImage(float z)
        {
            var len = 512;
            var image = new Bitmap(len, len);

            for (var i = 0; i < len; i++)
            {
                for (var j = 0; j < len; j++)
                {
                    var x = i / (float)len * 6;
                    var y = j / (float)len * 6;
                    image.SetPixel(i, j, MarblePerLin(x, y, z));
                }
            }

            return image;
        }
        
        static Image GetImage(Func<float, float, float> getNoiseFunc)
        {
            var len = 256;
            var image = new Bitmap(len, len);

            for (var i = 0; i < len; i++)
            {
                for (var j = 0; j < len; j++)
                {
                    var x = i / (float)len * 6 + 0.5f;
                    var y = j / (float)len * 6 + 0.5f;
                    image.SetPixel(i, j,  PerlinNoise.Lerp(Color.Black, Color.White, getNoiseFunc(x, y)));
                }
            }

            return image;
        }
    }
}
