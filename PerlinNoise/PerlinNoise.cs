﻿using System;
using System.Drawing;
using System.Numerics;

namespace PerlinNoise
{
    public static class PerlinNoise
    {
        /// <summary>
        /// Fast Hash Random
        /// </summary>
        private static readonly int[] perm = {
                      151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36,
                      103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0,
                      26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56,
                      87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
                      77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55,
                      46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132,
                      187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109,
                      198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126,
                      255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183,
                      170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43,
                      172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112,
                      104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162,
                      241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106,
                      157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205,
                      93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
                      151
        };


        public static float Lerp(float x, float y, float l)
        {
            return (1 - l) * x + l * y;
        }

        public static Color Lerp(Color x, Color y, float l)
        {
            return Color.FromArgb((int)Lerp(x.R, y.R, l), (int)Lerp(x.G, y.G, l), (int)Lerp(x.B, y.B, l));
        }

        static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float LinearFade(float t)
        {
            return t;
        }

        // dot product
        public static float Grad(int hash, float x, float y, float z)
        {
            switch (hash & 0xF)
            {
                case 0x0: return x + y;
                case 0x1: return -x + y;
                case 0x2: return x - y;
                case 0x3: return -x - y;
                case 0x4: return x + z;
                case 0x5: return -x + z;
                case 0x6: return x - z;
                case 0x7: return -x - z;
                case 0x8: return y + z;
                case 0x9: return -y + z;
                case 0xA: return y - z;
                case 0xB: return -y - z;
                case 0xC: return y + x;
                case 0xD: return -y + z;
                case 0xE: return y - x;
                case 0xF: return -y - z;
                default: return 0; // never happens
            }
        }

        public static float Noise(float x, float y, float z)
        {
            var X = (int)MathF.Floor(x) & 0xff;
            var Y = (int)MathF.Floor(y) & 0xff;
            var Z = (int)MathF.Floor(z) & 0xff;

            x -= (int)MathF.Floor(x);
            y -= (int)MathF.Floor(y);
            z -= (int)MathF.Floor(z);

            var u = Fade(x);
            var v = Fade(y);
            var w = Fade(z);

            var A = (perm[X] + Y) & 0xff;
            var B = (perm[X + 1] + Y) & 0xff;
            var AA = (perm[A] + Z) & 0xff;
            var BA = (perm[B] + Z) & 0xff;
            var AB = (perm[A + 1] + Z) & 0xff;
            var BB = (perm[B + 1] + Z) & 0xff;

            // HASH coordinate of 8 cube corners
            var AAA = perm[AA];
            var BAA = perm[BA];
            var ABA = perm[AB];
            var BBA = perm[BB];
            var AAB = perm[AA + 1];
            var BAB = perm[BA + 1];
            var ABB = perm[AB + 1];
            var BBB = perm[BB + 1];

            float x1, x2, y1, y2;
            x1 = Lerp(Grad(AAA, x, y, z), Grad(BAA, x - 1, y, z), u);
            x2 = Lerp(Grad(ABA, x, y - 1, z), Grad(BBA, x - 1, y - 1, z), u);
            y1 = Lerp(x1, x2, v);

            x1 = Lerp(Grad(AAB, x, y, z - 1), Grad(BAB, x - 1, y, z - 1), u);
            x2 = Lerp(Grad(ABB, x, y - 1, z - 1), Grad(BBB, x - 1, y - 1, z - 1), u);
            y2 = Lerp(x1, x2, v);
            return Lerp(y1, y2, w);
        }

        public static float Noise01(float x, float y, float z)
        { 
            return (Noise(x, y, z) + 1) / 2;
        }

        public static float GetNoise1(float x, float y)
        {
            var floorx = MathF.Floor(x);
            var floorxPlus1 = floorx + 1;
            var floory = MathF.Floor(y);
            var flooryPlus1 = floory + 1;

            // 纯随机
            float p0 = RandomValue();
            float p1 = RandomValue();
            float p2 = RandomValue();
            float p3 = RandomValue();
            
            var l1 = Lerp(p0, p1, x - floorx);
            var l2 = Lerp(p2, p3, x - floorx);
            return Lerp(l1, l2, y - floory);
        }
        
        public static float GetNoise2(float x, float y)
        {
            int floorx = (int)MathF.Floor(x);
            int floorxPlus1 = floorx + 1;
            int floory = (int)MathF.Floor(y);
            int flooryPlus1 = floory + 1;
            float p0 = Hash(floorx, floory);
            float p1 = Hash(floorxPlus1, floory);
            float p2 = Hash(floorx, flooryPlus1);
            float p3 = Hash(floorxPlus1, flooryPlus1);
            
            var l1 = Lerp(p0, p1, x - floorx);
            var l2 = Lerp(p2, p3, x - floorx);
            return Lerp(l1, l2, y - floory);
        }

        public static float GetNoise3(float x, float y)
        {
            int floorx = (int)MathF.Floor(x);
            int floorxPlus1 = floorx + 1;
            int floory = (int)MathF.Floor(y);
            int flooryPlus1 = floory + 1;

            float p0 = Hash(floorx, floory);
            float p1 = Hash(floorxPlus1, floory);
            float p2 = Hash(floorx, flooryPlus1);
            float p3 = Hash(floorxPlus1, flooryPlus1);

            var l1 = Lerp(p0, p1, Fade(x - floorx));
            var l2 = Lerp(p2, p3, Fade(x - floorx));
            return Lerp(l1, l2, Fade(y - floory));
        }
        
        public static float GetNoise4(float x, float y)
        {
            int floorx = (int)MathF.Floor(x);
            int floorxPlus1 = floorx + 1;
            int floory = (int)MathF.Floor(y);
            int flooryPlus1 = floory + 1;

            var p0 = HashGradient(floorx, floory);
            var p1 = HashGradient(floorxPlus1, floory);
            var p2 = HashGradient(floorx, flooryPlus1);
            var p3 = HashGradient(floorxPlus1, flooryPlus1);

            float p0Result = Dot(p0, (x - floorx, y - floory));
            float p1Result = Dot(p1, (x - floorxPlus1, y - floory));
            float p2Result = Dot(p2, (x - floorx, y - flooryPlus1));
            float p3Result = Dot(p3, (x - floorxPlus1, y - flooryPlus1));
            
            var l1 = Lerp(p0Result, p1Result, Fade(x - floorx));
            var l2 = Lerp(p2Result, p3Result, Fade(x - floorx));
            return (Lerp(l1, l2, Fade(y - floory)) + 1.0f) * 0.5f; // dot 得到的范围为 [-1, 1]
        }

        public static float Hash(float x, float y)
        {
            return Hash(Hash(x) + 57.31f * Hash(y));
        }

        public static float Hash(float x)
        {
            var a = MathF.Sin(x) * 43578.7263f;
            return a - MathF.Floor(a);
        }

        private static Random rand = new Random(1);
        public static float RandomValue()
        {
            return (float)rand.NextDouble();
        }

        public static float Dot((float, float) a, (float, float) b)
        { 
            return a.Item1 * b.Item2 + a.Item2 * b.Item1;
        }

        public static (float, float) HashGradient(float x, float y)
        {
            var angle = (float)(Hash(x, y) * 2f * Math.PI);
            return (MathF.Sin(angle), MathF.Cos(angle));
        }
    }
}
