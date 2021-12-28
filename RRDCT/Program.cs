using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;


namespace RRDCT
{
    class Program
    {
        static int DCTN = 3;
        static float MIN_F = 100f; 

        static void Main(string[] args)
        {
            var image = Bitmap.FromFile("originalPic.jpg") as Bitmap;

            // matrix
            var newImage = GetDCTImage(image);
            newImage.Save("newDenoisePic.jpg", ImageFormat.Jpeg);
        }

        static Image GetDCTImage(Bitmap image)
        {
            var grayMatrix = Image2Matrix(image);
            var split = SplitImage(grayMatrix);

            for (int i = 0; i < split.GetLength(1); i++)
            {
                for (int j = 0; j < split.GetLength(2); j++)
                {
                    split[0, i, j] = DCT(split[0, i, j]);
                    split[1, i, j] = DCT(split[1, i, j]);
                    split[2, i, j] = DCT(split[2, i, j]);
                    for (int x = 0; x < split[0, i, j].RowCount; x++)
                    {
                        for (int y = 0; y < split[0, i, j].ColumnCount; y++)
                        {
                            split[0, i, j][x, y] = split[0, i, j][x, y] < MIN_F ? 0 : split[0, i, j][x, y];
                            split[1, i, j][x, y] = split[1, i, j][x, y] < MIN_F ? 0 : split[1, i, j][x, y];
                            split[2, i, j][x, y] = split[2, i, j][x, y] < MIN_F ? 0 : split[2, i, j][x, y];
                        }
                    }
                }
            }

            for (int i = 0; i < split.GetLength(1); i++)
            {
                for (int j = 0; j < split.GetLength(2); j++)
                {
                    split[0, i, j] = IDCT(split[0, i, j]);
                    split[1, i, j] = IDCT(split[1, i, j]);
                    split[2, i, j] = IDCT(split[2, i, j]);
                }
            }

            var combineResult = CombineImage(split);
            return Matrix2Image(combineResult);
        }

        static Matrix<float>[] Image2Matrix(Bitmap image)
        {
            Matrix<float>[] result = new Matrix<float>[3];
            result[0] = Matrix<float>.Build.Dense(image.Width, image.Height);
            result[1] = Matrix<float>.Build.Dense(image.Width, image.Height);
            result[2] = Matrix<float>.Build.Dense(image.Width, image.Height);
            Matrix<float> basis = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1.0f / MathF.Sqrt(3f), 1.0f / MathF.Sqrt(2f), 1.0f / MathF.Sqrt(6f) },
                { 1.0f / MathF.Sqrt(3f), 0.0f, -2 / MathF.Sqrt(6f) },
                { 1.0f / MathF.Sqrt(3f), -1.0f / MathF.Sqrt(2f), 1.0f / MathF.Sqrt(6f) },
            });
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    result[0][i, j] = image.GetPixel(i, j).R;
                    result[1][i, j] = image.GetPixel(i, j).G;
                    result[2][i, j] = image.GetPixel(i, j).B;
                }
            }

            return result;
        }

        static Bitmap Matrix2Image(Matrix<float>[] input)
        {
            var image = new Bitmap(input[0].RowCount, input[0].ColumnCount);
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    image.SetPixel(i, j, Color.FromArgb(Math.Clamp((int)input[0][i, j], 0, 255), Math.Clamp((int)input[1][i, j], 0, 255), Math.Clamp((int)input[2][i, j],0,255)));
                }
            }

            return image;
        }

        /// <summary>
        /// 把图片切成N*N的图像
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static Matrix<float>[,,] SplitImage(Matrix<float>[] input)
        {
            int N = DCTN;
            var floatArray = new float[N, N];
            int x = input[0].RowCount + 2 * (N - 1);
            int y = input[0].ColumnCount + 2 * (N - 1);
            Matrix<float>[,,] result = new Matrix<float>[3,x,y];
            for (int i = 0; i < result.GetLength(1); i++)
            {
                for (int j = 0; j < result.GetLength(2); j++)
                {
                    for (int arrayI = 0; arrayI < N; arrayI++)
                    {
                        for (int arrayJ = 0; arrayJ < N; arrayJ++)
                        {
                            int indexX = i - N + 1 + arrayI;
                            int indexY = j - N + 1 + arrayJ;
                            floatArray[arrayI, arrayJ] = (indexX >= 0 && indexY >= 0 && indexX < input[0].RowCount && indexY < input[0].ColumnCount) ? input[0][indexX, indexY] : 0;
                            result[0, i, j] = Matrix<float>.Build.SparseOfArray(floatArray);
                        }
                    }
                    for (int arrayI = 0; arrayI < N; arrayI++)
                    {
                        for (int arrayJ = 0; arrayJ < N; arrayJ++)
                        {
                            int indexX = i - N + 1 + arrayI;
                            int indexY = j - N + 1 + arrayJ;
                            floatArray[arrayI, arrayJ] = (indexX >= 0 && indexY >= 0 && indexX < input[1].RowCount && indexY < input[1].ColumnCount) ? input[1][indexX, indexY] : 0;
                            result[1, i, j] = Matrix<float>.Build.SparseOfArray(floatArray);
                        }
                    }
                    for (int arrayI = 0; arrayI < N; arrayI++)
                    {
                        for (int arrayJ = 0; arrayJ < N; arrayJ++)
                        {
                            int indexX = i - N + 1 + arrayI;
                            int indexY = j - N + 1 + arrayJ;
                            floatArray[arrayI, arrayJ] = (indexX >= 0 && indexY >= 0 && indexX < input[2].RowCount && indexY < input[2].ColumnCount) ? input[2][indexX, indexY] : 0;
                            result[2, i, j] = Matrix<float>.Build.SparseOfArray(floatArray);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 把图片切成N*N的图像
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static Matrix<float>[] CombineImage(Matrix<float>[,,] input)
        {
            int N = DCTN;
            Matrix<float>[] result = new Matrix<float>[3];
            result[0] = Matrix<float>.Build.Dense(input.GetLength(1) -  2 * (N - 1), input.GetLength(2) - 2 * (N - 1));
            result[1] = Matrix<float>.Build.Dense(input.GetLength(1) - 2 * (N - 1), input.GetLength(2) - 2 * (N - 1));
            result[2] = Matrix<float>.Build.Dense(input.GetLength(1) - 2 * (N - 1), input.GetLength(2) - 2 * (N - 1));
            for (int i = 0; i < result[0].RowCount; i++)
            {
                for (int j = 0; j < result[0].ColumnCount; j++)
                {
                    float realNum = 0;
                    for (int arrayI = 0; arrayI < N; arrayI++)
                    {
                        for (int arrayJ = 0; arrayJ < N; arrayJ++)
                        {
                            realNum += input[0, i + arrayI, j + arrayJ][N - 1 - arrayI, N - 1 - arrayJ] / (N * N);
                        }
                    }

                    result[0][i, j] = realNum;

                    realNum = 0;
                    for (int arrayI = 0; arrayI < N; arrayI++)
                    {
                        for (int arrayJ = 0; arrayJ < N; arrayJ++)
                        {
                            realNum += input[1, i + arrayI, j + arrayJ][N - 1 - arrayI, N - 1 - arrayJ] / (N * N);
                        }
                    }

                    result[1][i, j] = realNum;

                    realNum = 0;
                    for (int arrayI = 0; arrayI < N; arrayI++)
                    {
                        for (int arrayJ = 0; arrayJ < N; arrayJ++)
                        {
                            realNum += input[2, i + arrayI, j + arrayJ][N - 1 - arrayI, N - 1 - arrayJ] / (N * N);
                        }
                    }

                    result[2][i, j] = realNum;
                }
            }

            return result;
        }

        /// <summary>
        /// DCT 运算, 输入为 N * N 块
        /// </summary>
        /// <returns></returns>
        static Matrix<float> DCT(Matrix<float> input)
        {
            int N = DCTN;
            Matrix<float> A, IA;
            if (DCTA.ContainsKey(N))
            {
                A = DCTA[N];
                IA = DCTIA[N];
            }
            else
            {
                A = Matrix<float>.Build.Dense(N, N);
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        var c = i == 0 ? MathF.Sqrt(1.0f / N) : MathF.Sqrt(2.0f / N);
                        A[i, j] = c * MathF.Cos((j + 0.5f) * MathF.PI / N * i);
                    }
                }
                IA = A.Inverse();
                DCTA[N] = A;
                DCTIA[N] = IA;
            }


            return A * input * IA;
        }

        static Dictionary<int, Matrix<float>> DCTA = new Dictionary<int, Matrix<float>>();
        static Dictionary<int, Matrix<float>> DCTIA = new Dictionary<int, Matrix<float>>();

        static Matrix<float> IDCT(Matrix<float> input)
        {
            int N = DCTN;
            Matrix<float> A, IA;
            if (DCTA.ContainsKey(N))
            {
                A = DCTA[N];
                IA = DCTIA[N];
            }
            else
            {
                A = Matrix<float>.Build.Dense(N, N);
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        var c = i == 0 ? MathF.Sqrt(1 / N) : MathF.Sqrt(2 / N);
                        A[i, j] = c * MathF.Cos((j + 0.5f) * MathF.PI / N * i);
                    }
                }
                IA = A.Inverse();
                DCTA[N] = A;
                DCTIA[N] = IA;
            }

            return IA * input * A;
        }
    }
}
