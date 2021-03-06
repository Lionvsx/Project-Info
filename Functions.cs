using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Math;


namespace Project_Info
{
    public static class Functions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Image ReadImage(string path)
        {
            var image = new Image();
            
            byte[] myfile = File.ReadAllBytes(path);
            //myfile est un vecteur composé d'octets représentant les métadonnées et les données de l'image
           
            //Métadonnées du fichier
            byte[] sign = {myfile[0],myfile[1]};
            var test = new byte[]{66, 77};
            if (sign[0] != test[0]) throw new ArgumentOutOfRangeException();
            if (sign[1] != test[1]) throw new ArgumentOutOfRangeException();

            byte[] fsize = {myfile[2], myfile[3], myfile[4], myfile[5]};
            byte[] off = {myfile[10], myfile[11], myfile[12], myfile[13]};
            //Métadonnées de l'image
            byte[] wid = {myfile[18], myfile[19], myfile[20], myfile[21]};
            byte[] hei = {myfile[22], myfile[23], myfile[24], myfile[25]};
            byte[] bpp = {myfile[28], myfile[29]};

            image.Height = ConvertToInt(hei);
            image.Width = ConvertToInt(wid);
            image.Offset = ConvertToInt(off);
            
            image.BitRgb = ConvertToInt(bpp);
            

            var imageData = new Pixel[image.Height,image.Width];
            
            //L'image elle-même
            var line = image.Height-1;
            var emptyBytes = (4 - (image.Width * 3 % 4))%4;
            for (var i = 54; i < image.Size; i = i + (image.Width*3 + emptyBytes))
            {
                var col = 0;
                for (var j = i; j < i + image.Width*3; j++)
                {
                    switch (col % 3)
                    {
                        case 0:
                            imageData[line, col / 3] = new Pixel
                            {
                                Blue = myfile[j]
                            };
                            break;
                        case 1:
                            imageData[line, col / 3].Red = myfile[j];
                            break;
                        case 2:
                            imageData[line, col / 3].Green = myfile[j];
                            break;
                    }
                    ++col;
                }

                --line;
            }

            image.ImageData = imageData;
            return image;

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="im"></param>
        /// <returns></returns>
        public static void WriteImage(Image im, string path)
        {
            var file = new List<byte>();
            const byte vide = byte.MinValue;
            const int b = 66;
            const int m = 77;
            var type = new List<byte>(){b, m};
            var size1 = new List<byte>(){vide};
            var size3 = new List<byte>(){vide, vide, vide};
            var size4 = new List<byte>(){vide, vide, vide,vide};

            file.AddRange(type);
            for (var i = 0; i < ConvertToEndian(im.Size,4).Length; i++)
            {
                file.Add(ConvertToEndian(im.Size,4)[i]);
            }
            
            file.AddRange(size4);
            
            for (var i = 0; i < ConvertToEndian(im.Offset, 4).Length; i++)
            {
                file.Add(ConvertToEndian(im.Offset, 4)[i]);
            }
            file.Add(40);
            file.AddRange(size3);
            
            for (var i = 0; i < ConvertToEndian(im.Width,4).Length; i++)
            {
                file.Add(ConvertToEndian(im.Width,4)[i]);
            }
            
            for (var i = 0; i < ConvertToEndian(im.Height,4).Length; i++)
            {
                file.Add(ConvertToEndian(im.Height,4)[i]);
            }
            file.Add(1);
            file.AddRange(size1);
            for (var i = 0; i < ConvertToEndian(im.BitRgb, 2).Length; i++)
            {
                file.Add(ConvertToEndian(im.BitRgb, 2)[i]);
            }
            for (var i = 0; i < 2; i++)
            {
                file.AddRange(size4);
            }
            file.Add(196);
            file.Add(4);
            for (var i = 0; i < 2; i++)
            {
                file.AddRange(size1);
            }
            file.Add(196);
            file.Add(4);
            for (var i = 0; i < 2; i++)
            {
                file.AddRange(size1);
            }
            for (var i = 0; i < 2; i++)
            {
                file.AddRange(size4);
            }
            
            for (var i = im.ImageData.GetLength(0) - 1; i >= 0; i--)
            {
                for (var j = 0; j < im.ImageData.GetLength(1); j++)
                {
                    file.Add(Convert.ToByte(Abs(im.ImageData[i,j].Blue)));
                    file.Add(Convert.ToByte(Abs(im.ImageData[i,j].Red)));
                    file.Add(Convert.ToByte(Abs(im.ImageData[i,j].Green)));

                }

                for (var k = 0; k < im.ImageData.GetLength(1)%4; k++)
                {
                    file.Add(vide);
                }
                
            }
            
            File.WriteAllBytes(path, file.ToArray());
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int ConvertToInt(IEnumerable<byte> data)
        {
            var result = 0;
            var enumerable = data.ToList();
            for (var i = 0; i < enumerable.Count; i++)
            {
                result = (int) (result + enumerable[i] * Pow(256, i));
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] ConvertToEndian(int data, int size)
        {
            var endian = new byte[size];
            for (var i = size - 1; i >= 0; i--)
            {
                endian[i] = (byte) (data / Pow(256,i ));
                data -= endian[i] * (int) (Pow(256, i));
            }
            return endian;
        }

        public static int[] ConvertBitArrayToByteArray(int[] data)
        {
            var result = new int[data.Length / 8];
            var intByte = 0;
            for (int i = 0; i < data.Length; i++)
            {

                intByte += (int) (Pow(2, Abs(i % 8 - 7)) * data[i]);
                if (i != 0 && (i + 1) % 8 == 0)
                {
                    result[i / 8] = intByte;
                    intByte = 0;
                }
            }
            return result;
        }
        
        public static int[] ConvertByteArrayToBitArray(int[] data)
        {
            var result = new int[data.Length * 8];
            for (int i = 0; i < result.Length; i++)
            {
                var division =  (int) (data[i / 8] / Pow(2, Abs(i % 8 - 7)));
                result[i] = division;
                data[i / 8] -= (int) (Pow(2, Abs(i % 8 - 7)) * division);
            }
            return result;
        }
        public static void DisplayBmp(byte[] myfile)
        {
            
            //Métadonnées du fichier
            Console.WriteLine("\n Header \n");
            for (int i = 0; i < 14; i++)
                Console.Write(myfile[i] + " ");
            //Métadonnées de l'image
            Console.WriteLine("\n HEADER INFO \n");
            for (int i = 14; i< 54; i++)
                Console.Write(myfile[i] + " ");
            //L'image elle-même
            Console.WriteLine("\n IMAGE \n");
            for (int i = 54; i < myfile.Length; i = i + 60)
            {
                for (int j = i; j < i + 60; j++)
                {
                    Console.Write("{0:D3} ",myfile[j]);

                }
                Console.WriteLine();
            }

            File.WriteAllBytes("./Images/Sortie.bmp", myfile);
        }

        public static Image Fractal(Image im)
        {
            var nbIteration =2000;
            var xmin = -2.1;
            var xmax = 0.6;
            var ymin = -1.2;
            var ymax = 1.2;
            for (var y = 0; y < im.ImageData.GetLength(0); y++)
            {
                for (var x = 0; x < im.ImageData.GetLength(1); x++)
                {
                    var cx = (x * (xmax - xmin) / im.ImageData.GetLength(1) + xmin);
                    var cy = (y * (ymax - ymin) / im.ImageData.GetLength(0) + ymin);
                    var xn = 0.0;
                    var yn = 0.0;
                    var n = 0;
                    while ((xn * xn + yn * yn) < 4 && n < nbIteration)
                    {
                        var tempx = xn;
                        var tempy = yn;
                        xn = tempx * tempx - tempy * tempy + cx;
                        yn = 2 * tempx * tempy + cy;
                        n++;
                    }

                    if (n == nbIteration)
                    {
                        im.ImageData[y, x] = new Pixel(255, 184, 53);
                        
                    }
                    else
                    {
                        im.ImageData[y, x] = new Pixel(0, 0, 0);
                    }
                } 
            }

            return im;
        }

        public static Image Histograme(Image im)
        { 
            var coefwidth = (im.Width/256)+1;
            var newHeight = im.Height;
            var newWidth = 256*coefwidth;
            var histo = new Image(im.Type, im.Offset, newHeight, newWidth, im.BitRgb,
                CreateBlackImage(im.Height, 256 * coefwidth));
           
            
            var rgbColor = new [] {new int[256], new int[256], new int[256]};
            for (var i = 0; i < im.Height; i++)
            {
                for (var j = 0; j < im.Width; j++)
                {
                    rgbColor[0][ im.ImageData[i, j].Red]++;                            // Red
                    rgbColor[1][ im.ImageData[i, j].Green]++;
                    rgbColor[2][ im.ImageData[i, j].Blue]++;
                }
            }
            
            for (var k = 0; k < histo.Width; k+=coefwidth)
            {
                
                for (var rep = 0; rep < coefwidth; rep++)
                {
                    for (var l = 0; l < rgbColor[0][k / coefwidth]/10; l++)
                    {
                        histo.ImageData[histo.Height-1-l, k+rep].Red = 255;
                    }

                    for (var m = 0; m < rgbColor[1][k / coefwidth]/10; m++)
                    {
                        histo.ImageData[histo.Height-1-m, k+rep].Green = 255;
                    }

                    for (var n = 0; n < rgbColor[2][k /coefwidth]/10; n++)
                    {
                        histo.ImageData[histo.Height-1-n, k+rep].Blue = 255;
                    }
                }
            }
            
            return histo;
        }

        public static Pixel[,] CreateWhiteImage(int height, int width)
        {
            var imageData = new Pixel[height, width];
            for (int i = 0; i < imageData.GetLength(0); i++)
            {
                for (int j = 0; j < imageData.GetLength(1); j++)
                {
                    imageData[i, j] = new Pixel(255, 255, 255);
                }
            }
            return imageData;
        }
        public static Pixel[,] CreateBlackImage(int height, int width)
        {
            var imageData = new Pixel[height, width];
           
            for (int i = 0; i < imageData.GetLength(0); i++)
            {
                for (int j = 0; j < imageData.GetLength(1); j++)
                {
                    
                    imageData[i, j] = new Pixel(0, 0, 0);
                }
            }
            return imageData;
        }
        
        public static IEnumerable<string> ReadFile(string path)
        {
            var lines = new Stack<string>();
            try
            {
                using var sr = new StreamReader(path);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Push(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                throw new IOException();
                
            }

            return lines.ToArray().Reverse();
        }
        
        public static bool CheckArrayEquality<T>(IEnumerable<T> first, IEnumerable<T> second) {
            return first.SequenceEqual(second);
        }


        public static void WriteFile(IEnumerable<string> lines, string path)
        {
            try
            {
                using var sw = new StreamWriter(path);
                foreach (var line in lines)
                {
                    sw.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occured while trying to write file");
                Console.WriteLine(e.Message);
                throw new IOException();
            }
        }
        
        public static void FillImageWhite(Pixel[,] imageData)
        {
            for (int i = 0; i < imageData.GetLength(0); i++)
            {
                for (int j = 0; j < imageData.GetLength(1); j++)
                {
                    imageData[i, j] ??= new Pixel(255, 255, 255);
                }
            }
        }
        
        public static void FillImageRed(Pixel[,] imageData)
        {
            for (int i = 0; i < imageData.GetLength(0); i++)
            {
                for (int j = 0; j < imageData.GetLength(1); j++)
                {
                    imageData[i, j] ??= new Pixel(0, 255, 0);
                }
            }
        }
        public static IEnumerable<T[]> Combinations<T>(IEnumerable<T> source) {
            if (null == source)
                throw new ArgumentNullException(nameof(source));

            var data = source.ToArray();

            return Enumerable
                .Range(0, 1 << (data.Length))
                .Select(index => data
                    .Where((v, i) => (index & (1 << i)) != 0)
                    .ToArray());
        }

        public static int[] XOR(int[] x, int[] y)
        {
            var result = new int[x.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = x[i] == 1 && y[i] == 1 ? 0 : x[i] != y[i] ? 1 : 0;
            }
            return result;
        }

        public static int[] ConvertIntToBinaryArray(int value)
        {
            var binaryString = Convert.ToString(value, 2);
            var result = new int[binaryString.Length];
            for (var index = 0; index < binaryString.Length; index++)
            {
                result[index] = binaryString[index] - 48;
            }
            return result;
        }

        public static int[] TrimAndPad(int[] array, int desiredLength)
        {
            while (array[0] == 0) array = array.Skip(1).ToArray();
            var zerosArray = Enumerable.Repeat(0, desiredLength - array.Length);
            return array.Concat(zerosArray).ToArray();
        }


        public static int[] Pad(int[] array, int desiredLength)
        {
            var zerosArray = Enumerable.Repeat(0, desiredLength - array.Length);
            return array.Concat(zerosArray).ToArray();
        }
        
        public static int[] UnShift(int[] array, int desiredLength)
        {
            var zerosArray = Enumerable.Repeat(0, desiredLength - array.Length);
            return zerosArray.Concat(array).ToArray();
        }
        
        public static int[] Trim(int[] array)
        {
            while (array[0] == 0) array = array.Skip(1).ToArray();
            return array;
        }

        public static IEnumerable<int[]> DoubleIntCombinations(IEnumerable<int> source)
        {
            var combinations = Combinations(source);
            return combinations.Where(v => v.Length == 2).ToArray();
        }

        public static int[] GetImageRotationOffset(Pixel[,] imageData, double radians)
        {
            var xOffsets = new List<int>() {0};
            var yOffsets = new List<int>() {0};

            var lines = new int[] {0, -imageData.GetLength(0), -imageData.GetLength(0)};
            var columns = new int[] { imageData.GetLength(1), imageData.GetLength(1), 0 };

            for (int i = 0; i < 3; i++)
            {
                var newLineDouble = Round(columns[i] * Sin(radians) + lines[i] * Cos(radians));
                var newColDouble = Round(columns[i] * Cos(radians) - lines[i] * Sin(radians));
                
                if (newLineDouble < 0) yOffsets.Add((int) Abs(newLineDouble + 1));
                if (newColDouble < 0) xOffsets.Add((int) Abs(newColDouble + 1));
            }

            return new int[] {yOffsets.Max(), xOffsets.Max()};
        }

        public static Image Hide(Image bigImage, Image smallImage)
        {
            var coef = 0;
            if (bigImage.Width / smallImage.Width > bigImage.Height / smallImage.Height)
            {
                coef = bigImage.Width / smallImage.Width; 
            }
            else
            {
                coef = bigImage.Height / smallImage.Height;
            }
            
            bigImage.ImageData[bigImage.ImageData.GetLength(0) - 1, bigImage.ImageData.GetLength(1) - 1].Red =
                (0xF0 & (byte) (bigImage
                    .ImageData[bigImage.ImageData.GetLength(0) - 1, bigImage.ImageData.GetLength(1) - 1].Red)) |
                (byte) coef;
            
            
            for (var x = 0; x < bigImage.Height; x+=coef)
            {
                for (var y = 0; y < bigImage.Width; y+=coef)
                {
                    bigImage.ImageData[x, y].Red = (0xF0&(byte)bigImage.ImageData[x, y].Red)| ((0xF0&(byte)(smallImage.ImageData[x/coef, y/coef].Red))>>4);
                    bigImage.ImageData[x, y].Green = (0xF0&(byte)bigImage.ImageData[x, y].Green)| ((0xF0&(byte)(smallImage.ImageData[x/coef, y/coef].Green))>>4);
                    bigImage.ImageData[x, y].Blue = (0xF0&(byte)bigImage.ImageData[x, y].Blue)| ((0xF0&(byte)(smallImage.ImageData[x/coef, y/coef].Blue))>>4);
                }
            }
            return bigImage;
        }

        public static int[] IntToDesiredLengthBit(int value, int desiredLength)
        {
            return UnShift(ConvertIntToBinaryArray(value), desiredLength);
        }

        public static Image Found(Image image)
        {
            var coef = 0x0F & image.ImageData[image.ImageData.GetLength(0) - 1, image.ImageData.GetLength(1)-1].Red;
            
            var newimdata = CreateBlackImage(image.Height / (coef), image.Width / (coef));
            
            for (var x = 0; x < newimdata.GetLength(0); x++)
            {
                for (var y = 0; y < newimdata.GetLength(1); y++)
                {
                    newimdata[x, y].Red = (0x0F & (byte)(image.ImageData[x*(coef) , y*(coef) ].Red))<<4;
                    newimdata[x, y].Green = (0x0F & (byte)(image.ImageData[x*(coef) , y*(coef) ].Green))<<4;
                    newimdata[x, y].Blue = (0x0F & (byte)(image.ImageData[x*(coef), y*(coef) ].Blue))<<4;
                }
            }
            var newImage = new Image(image.Type,image.Offset,newimdata.GetLength(0),newimdata.GetLength(1), image.BitRgb, newimdata);
            return newImage;
        }

        public static Pixel InvertPixel(Pixel pixel)
        {
            return pixel.IsBlack ? new Pixel(255, 255, 255) : new Pixel(0, 0, 0);
        }
    }
}