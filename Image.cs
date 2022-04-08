using System;
using System.Drawing.Drawing2D;

namespace Project_Info
{
    public class Image
    {
        public Image()
        {
        }
        
        
        public string Type { get; set; }

        public int Size => Height * Width * 3 + Offset;

        public int Offset { get; set; }

        public int Height { get; set; }

        //public int Height => ImageData.GetLength(0);

        public int Width { get; set; }

        public int BitRgb { get; set; }

        public Pixel[,] ImageData { get; set; }

        public Image(Image image)
        {
            Type = image.Type;
            Offset = image.Offset;
            Height = image.Height;
            Width = image.Width;
            BitRgb = image.BitRgb;
            ImageData = image.ImageData;
        }

        public Image(string type, int offset, int height, int width, int bitRgb, Pixel[,] image)
        {
            Type = type;
            Offset = offset;
            Height = height;
            Width = width;
            BitRgb = bitRgb;
            ImageData = image;
        }
        

        public void DisplayImage()
        {
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    Console.BackgroundColor = ImageData[i, j].HexString == "000000" ? ConsoleColor.Black : ConsoleColor.White;
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    
        public void Rotate90R()
        {
            var newImage = new Pixel[ImageData.GetLength(1), ImageData.GetLength(0)];
            for (var line = 0; line < ImageData.GetLength(0); line++)
            {
                for (var col = 0; col < ImageData.GetLength(1); col++)
                {
                    newImage[col, (newImage.GetLength(1) - 1) - line] = ImageData[line, col];
                }
            }

            Height = ImageData.GetLength(1);
            Width = ImageData.GetLength(0);
            ImageData = newImage;
        }
        
        public void Rotate90L()
        {
            var newImage = new Pixel[ImageData.GetLength(1), ImageData.GetLength(0)];
            for (var line = 0; line < ImageData.GetLength(0); line++)
            {
                for (var col = 0; col < ImageData.GetLength(1); col++)
                {
                    newImage[(newImage.GetLength(0) - 1) - col, line] = ImageData[line, col];
                }
            }
            
            Height = ImageData.GetLength(1);
            Width = ImageData.GetLength(0);
            ImageData = newImage;
        }
//add check if possible
        public void Maximize(double factor)
        {
            
                var newImage = new Pixel[(int) (ImageData.GetLength(0) * factor), (int) (ImageData.GetLength(1) * factor)];
                for (var line = 0; line < newImage.GetLength(0); line++)
                {
                    for (var col = 0; col < newImage.GetLength(1); col++)
                    {
                        newImage[line, col] = new Pixel(ImageData[(int) (line / factor), (int) (col / factor)]);
                    }
                }

                Height = (int) (ImageData.GetLength(0) * factor);
                Width = (int) (ImageData.GetLength(1) * factor);
                ImageData = newImage;
            
        }
        //add check if possible
        public void Minimize(double factor)
        {
            
                var newImage = new Pixel[(int) (ImageData.GetLength(0) / factor), (int) (ImageData.GetLength(1) / factor)];
                for (var line = 0; line < newImage.GetLength(0); line++)
                {
                    for (var col = 0; col < newImage.GetLength(1); col++)
                    {
                        newImage[line, col] = new Pixel(ImageData[(int) (line * factor), (int) (col * factor)]);
                    }
                }

                Height =  (int) (ImageData.GetLength(0)/factor);
                Width = (int) (ImageData.GetLength(1)/factor);
                ImageData = newImage;
        }
        public void Mirror()
        {
            var newImage = new Pixel[ImageData.GetLength(0), ImageData.GetLength(1)];
            for (var line = 0; line < ImageData.GetLength(0); line++)
            {
                for (var col = 0; col < ImageData.GetLength(1); col++)
                {
                    newImage[line, newImage.GetLength(1) - 1 - col] = ImageData[line, col];
                }
            }
            ImageData = newImage;
        }
        public void ConvolutionFilter(double[,] kernel, double factor = 1.0)
        {
            var newImageData = new Pixel[ImageData.GetLength(0), ImageData.GetLength(1)];
            for (var line = 0; line < ImageData.GetLength(0); line++)
            {
                for (var col = 0; col < ImageData.GetLength(1); col++)
                {
                    var filteredPixel = new Pixel()
                    {
                        Red = 0,
                        Green = 0,
                        Blue = 0
                    };
                    for (var kernelLine = 0; kernelLine < kernel.GetLength(0); kernelLine++)
                    {
                        var imageLine = (line + (kernelLine - kernel.GetLength(0) / 2) + ImageData.GetLength(0)) %
                                        ImageData.GetLength(0);
                        for (var kernelCol = 0; kernelCol < kernel.GetLength(1); kernelCol++)
                        {
                            var imageCol = (col + (kernelCol - kernel.GetLength(1) / 2) + ImageData.GetLength(1)) %
                                           ImageData.GetLength(1);

                            filteredPixel.Red +=
                                (int) kernel[kernelLine, kernelCol] * ImageData[imageLine, imageCol].Red;
                            filteredPixel.Blue +=
                                (int) kernel[kernelLine, kernelCol] * ImageData[imageLine, imageCol].Blue;
                            filteredPixel.Green +=
                                (int) kernel[kernelLine, kernelCol] * ImageData[imageLine, imageCol].Green;
                        }
                    }
                    filteredPixel.Red = (int) Math.Abs(filteredPixel.Red * factor);
                    filteredPixel.Blue = (int) Math.Abs(filteredPixel.Blue * factor);
                    filteredPixel.Green = (int) Math.Abs(filteredPixel.Green * factor);

                    newImageData[line, col] = new Pixel()
                    {
                        Red = filteredPixel.Red > 255 ? 255 : filteredPixel.Red,
                        Blue = filteredPixel.Blue > 255 ? 255 : filteredPixel.Blue,
                        Green = filteredPixel.Green > 255 ? 255 : filteredPixel.Green
                    };
                }
            }
            ImageData = newImageData;
        }

        public void DoubleConvolutionFilter(double[,] kernelX, double[,] kernelY, double factor = 1)
        {
            var newImageData = new Pixel[ImageData.GetLength(0), ImageData.GetLength(1)];
            if (kernelX.GetLength(0) != kernelY.GetLength(0) || kernelX.GetLength(1) != kernelY.GetLength(1))
                throw new ArgumentException("KernelX and kernelY must be the same dimensions");
            for (var line = 0; line < ImageData.GetLength(0); line++)
            {
                for (var col = 0; col < ImageData.GetLength(1); col++)
                {
                    var filteredPixelX = new Pixel()
                    {
                        Red = 0,
                        Green = 0,
                        Blue = 0
                    };
                    var filteredPixelY = new Pixel()
                    {
                        Red = 0,
                        Green = 0,
                        Blue = 0
                    };
                    for (var kernelLine = 0; kernelLine < kernelX.GetLength(0); kernelLine++)
                    {
                        var imageLine = (line + (kernelLine - kernelX.GetLength(0) / 2) + ImageData.GetLength(0)) %
                                        ImageData.GetLength(0);
                        for (var kernelCol = 0; kernelCol < kernelX.GetLength(1); kernelCol++)
                        {
                            var imageCol = (col + (kernelCol - kernelX.GetLength(1) / 2) + ImageData.GetLength(1)) %
                                           ImageData.GetLength(1);

                            filteredPixelX.Red +=
                                (int) kernelX[kernelLine, kernelCol] * ImageData[imageLine, imageCol].Red;
                            filteredPixelX.Blue +=
                                (int) kernelX[kernelLine, kernelCol] * ImageData[imageLine, imageCol].Blue;
                            filteredPixelX.Green +=
                                (int) kernelX[kernelLine, kernelCol] * ImageData[imageLine, imageCol].Green;
                            
                            filteredPixelY.Red +=
                                (int) kernelY[kernelLine, kernelCol] * ImageData[imageLine, imageCol].Red;
                            filteredPixelY.Blue +=
                                (int) kernelY[kernelLine, kernelCol] * ImageData[imageLine, imageCol].Blue;
                            filteredPixelY.Green +=
                                (int) kernelY[kernelLine, kernelCol] * ImageData[imageLine, imageCol].Green;
                        }
                    }
                    filteredPixelX.Red = (int) (filteredPixelX.Red * factor);
                    filteredPixelX.Blue = (int) (filteredPixelX.Blue * factor);
                    filteredPixelX.Green = (int) (filteredPixelX.Green * factor);
                    
                    filteredPixelY.Red = (int) (filteredPixelY.Red * factor);
                    filteredPixelY.Blue = (int) (filteredPixelY.Blue * factor);
                    filteredPixelY.Green = (int) (filteredPixelY.Green * factor);

                    var redValue = (int) Math.Sqrt(filteredPixelX.Red * filteredPixelX.Red +
                                                   filteredPixelY.Red * filteredPixelY.Red);
                    var greenValue = (int) Math.Sqrt(filteredPixelX.Blue * filteredPixelX.Blue +
                                                     filteredPixelY.Blue * filteredPixelY.Blue);
                    var blueValue = (int) Math.Sqrt(filteredPixelX.Blue * filteredPixelX.Blue +
                                              filteredPixelY.Blue * filteredPixelY.Blue);

                    newImageData[line, col] = new Pixel()
                    {
                        Red = redValue > 255 ? 255 : redValue,
                        Blue = blueValue > 255 ? 255 : blueValue,
                        Green = greenValue > 255 ? 255 : greenValue
                    };
                }
            }
            ImageData = newImageData;
        }
        
        public void ConvertToGrey()
        {
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width ; j++)
                {
                    //Apply conversion equation
                    var gray = (byte)(.21 * ImageData[i,j].Red + .71 * ImageData[i,j].Green + .071 * ImageData[i,j].Blue);

                    //Set the color of this pixel
                    ImageData[i, j].Red = gray;
                    ImageData[i, j].Green = gray;
                    ImageData[i, j].Blue = gray;
                }
            }
        }

        public void RotateAngle(double radians)
        {
            int newWidth = (int) Math.Abs(Math.Round(Math.Cos(radians) * Width + Math.Sin(radians) * Height));
            int newHeight = (int) Math.Abs(Math.Round(Math.Sin(radians) * Width + Math.Cos(radians) * Height));
            var newImageData = new Pixel[newHeight, newWidth];

            var offsets = Functions.GetImageRotationOffset(ImageData, radians);

            var offsetY = offsets[0];
            var offsetX = offsets[1];

            for (var line = 0; line < ImageData.GetLength(0); line++)
            {
                var ghostLine = - line;
                for (var col = 0; col < ImageData.GetLength(1); col++)
                {
                    var newLineDouble = Math.Abs(col * Math.Sin(radians) + ghostLine * Math.Cos(radians) + offsetY - newHeight + 1);
                    var newColDouble = Math.Abs(col * Math.Cos(radians) - ghostLine * Math.Sin(radians) + offsetX);
                    var newLine = Math.Round(newLineDouble);
                    var newCol = Math.Round(newColDouble);
                    if (Math.Abs(newLineDouble - newLine) < Math.Pow(10, -5) && Math.Abs(newColDouble - newCol) < Math.Pow(10, -5))
                    {
                        newImageData[(int) newLine, (int) newCol] = ImageData[line, col];
                    }
                    else
                    {
                        var newLineMax = (int) Math.Ceiling(newLineDouble);
                        var newLineMin = (int) Math.Floor(newLineDouble);
                        var newColMax = (int) Math.Ceiling(newColDouble);
                        var newColMin = (int) Math.Floor(newColDouble);

                        
                            if (newLineMax < newHeight && newColMax < newWidth) newImageData[newLineMax, newColMax] ??= ImageData[line, col];
                            if (newLineMin >= 0 && newColMax < newWidth) newImageData[newLineMin, newColMax] ??= ImageData[line, col];
                            if (newLineMax < newHeight && newColMin >= 0) newImageData[newLineMax, newColMin] ??= ImageData[line, col];
                            if (newLineMin >= 0 && newColMin >= 0) newImageData[newLineMin, newColMin] ??= ImageData[line, col];
                        
                    }
                    //Console.WriteLine($"({ghostLine},{col}) ==> ({newLine},{newCol})");
                }
            }
            Functions.FillImageWhite(newImageData);
            Height = newHeight;
            Width = newWidth;
            ImageData = newImageData;
        }
    }
}