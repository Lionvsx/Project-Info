using System;

namespace Project_Info
{
    public class Image
    {
        private string _type;
        private int _size;
        private int _offset;
        private int _height;
        private int _width;
        private int _bitRgb;
        private Pixel[,] _imageData;

        public Image()
        {
        }
        public string Type
        {
            get => _type;
            set => _type = value;
        }

        public int Size
        {
            get => _height * _width * 3 + _offset;
        }

        public int Offset
        {
            get => _offset;
            set => _offset = value;
        }

        public int Height
        {
            get => _height;
            set => _height = value;
        }

        public int Width
        {
            get => _width;
            set => _width = value;
        }

        public int BitRgb
        {
            get => _bitRgb;
            set => _bitRgb = value;
        }

        public Pixel[,] ImageData
        {
            get => _imageData;
            set => _imageData = value;
        }

        public Image(Image image)
        {
            _type = image.Type;
            _size = image.Size;
            _offset = image.Offset;
            _height = image.Height;
            _width = image.Width;
            _bitRgb = image.BitRgb;
            _imageData = image.ImageData;
        }

        public Image(string type, int size, int offset, int height, int width, int bitRgb, Pixel[,] image)
        {
            _type = type;
            _size = size;
            _offset = offset;
            _height = height;
            _width = width;
            _bitRgb = bitRgb;
            _imageData = image;
        }
        

        public void DisplayImage()
        {
            for (var i = 0; i < _height; i++)
            {
                for (var j = 0; j < _width; j++)
                {
                    Console.BackgroundColor = _imageData[i, j].HexString == "000000" ? ConsoleColor.Black : ConsoleColor.White;
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    
        public void Rotate90R()
        {
            var newImage = new Pixel[_imageData.GetLength(1), _imageData.GetLength(0)];
            for (var line = 0; line < _imageData.GetLength(0); line++)
            {
                for (var col = 0; col < _imageData.GetLength(1); col++)
                {
                    newImage[col, (newImage.GetLength(1) - 1) - line] = _imageData[line, col];
                }
            }

            _height = _imageData.GetLength(1);
            _width = _imageData.GetLength(0);
            _imageData = newImage;
        }
        
        public void Rotate90L()
        {
            var newImage = new Pixel[_imageData.GetLength(1), _imageData.GetLength(0)];
            for (var line = 0; line < _imageData.GetLength(0); line++)
            {
                for (var col = 0; col < _imageData.GetLength(1); col++)
                {
                    newImage[(newImage.GetLength(0) - 1) - col, line] = _imageData[line, col];
                }
            }
            
            _height = _imageData.GetLength(1);
            _width = _imageData.GetLength(0);
            _imageData = newImage;
        }
//add check if possible
        public void Maximize(int factor)
        {
            var newImage = new Pixel[_imageData.GetLength(0) * factor, _imageData.GetLength(1) * factor];
            for (var line = 0; line < newImage.GetLength(0); line++)
            {
                for (var col = 0; col < newImage.GetLength(1); col++)
                {
                    newImage[line, col] = new Pixel(_imageData[line / factor, col / factor]);
                }
            }
            _height *= factor;
            _width *= factor;
            _imageData = newImage;
        }
        //add check if possible
        public void Minimize(int factor)
        {
            var newImage = new Pixel[_imageData.GetLength(0) / factor, _imageData.GetLength(1) / factor];
            for (var line = 0; line < newImage.GetLength(0); line++)
            {
                for (var col = 0; col < newImage.GetLength(1); col++)
                {
                    newImage[line, col] = new Pixel(_imageData[line * factor, col * factor]);
                }
            }
            _height /= factor;
            _width /= factor;
            _imageData = newImage;
        }
        public void Mirror()
        {
            var newImage = new Pixel[_imageData.GetLength(0), _imageData.GetLength(1)];
            for (var line = 0; line < _imageData.GetLength(0); line++)
            {
                for (var col = 0; col < _imageData.GetLength(1); col++)
                {
                    newImage[line, newImage.GetLength(1) - 1 - col] = _imageData[line, col];
                }
            }
            _imageData = newImage;
        }

        public void ConvolutionFilter(double[,] kernel, double factor = 1.0)
        {
            var newImageData = new Pixel[_imageData.GetLength(0), _imageData.GetLength(1)];
            for (var line = 0; line < _imageData.GetLength(0); line++)
            {
                for (var col = 0; col < _imageData.GetLength(1); col++)
                {
                    var filteredPixel = new Pixel()
                    {
                        Red = 0,
                        Green = 0,
                        Blue = 0
                    };
                    for (var kernelLine = 0; kernelLine < kernel.GetLength(0); kernelLine++)
                    {
                        var imageLine = (line + (kernelLine - kernel.GetLength(0) / 2) + _imageData.GetLength(0)) %
                                        _imageData.GetLength(0);
                        for (var kernelCol = 0; kernelCol < kernel.GetLength(1); kernelCol++)
                        {
                            var imageCol = (col + (kernelCol - kernel.GetLength(1) / 2) + _imageData.GetLength(1)) %
                                           _imageData.GetLength(1);

                            filteredPixel.Red +=
                                (int) kernel[kernelLine, kernelCol] * _imageData[imageLine, imageCol].Red;
                            filteredPixel.Blue +=
                                (int) kernel[kernelLine, kernelCol] * _imageData[imageLine, imageCol].Blue;
                            filteredPixel.Green +=
                                (int) kernel[kernelLine, kernelCol] * _imageData[imageLine, imageCol].Green;
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
            _imageData = newImageData;
        }

        public void DoubleConvolutionFilter(double[,] kernelX, double[,] kernelY, double factor = 1)
        {
            var newImageData = new Pixel[_imageData.GetLength(0), _imageData.GetLength(1)];
            if (kernelX.GetLength(0) != kernelY.GetLength(0) || kernelX.GetLength(1) != kernelY.GetLength(1))
                throw new ArgumentException("KernelX and kernelY must be the same dimensions");
            for (var line = 0; line < _imageData.GetLength(0); line++)
            {
                for (var col = 0; col < _imageData.GetLength(1); col++)
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
                        var imageLine = (line + (kernelLine - kernelX.GetLength(0) / 2) + _imageData.GetLength(0)) %
                                        _imageData.GetLength(0);
                        for (var kernelCol = 0; kernelCol < kernelX.GetLength(1); kernelCol++)
                        {
                            var imageCol = (col + (kernelCol - kernelX.GetLength(1) / 2) + _imageData.GetLength(1)) %
                                           _imageData.GetLength(1);

                            filteredPixelX.Red +=
                                (int) kernelX[kernelLine, kernelCol] * _imageData[imageLine, imageCol].Red;
                            filteredPixelX.Blue +=
                                (int) kernelX[kernelLine, kernelCol] * _imageData[imageLine, imageCol].Blue;
                            filteredPixelX.Green +=
                                (int) kernelX[kernelLine, kernelCol] * _imageData[imageLine, imageCol].Green;
                            
                            filteredPixelY.Red +=
                                (int) kernelY[kernelLine, kernelCol] * _imageData[imageLine, imageCol].Red;
                            filteredPixelY.Blue +=
                                (int) kernelY[kernelLine, kernelCol] * _imageData[imageLine, imageCol].Blue;
                            filteredPixelY.Green +=
                                (int) kernelY[kernelLine, kernelCol] * _imageData[imageLine, imageCol].Green;
                        }
                    }
                    filteredPixelX.Red = (int) (filteredPixelX.Red * factor);
                    filteredPixelX.Blue = (int) (filteredPixelX.Blue * factor);
                    filteredPixelX.Green = (int) (filteredPixelX.Green * factor);
                    
                    filteredPixelY.Red = (int) (filteredPixelX.Red * factor);
                    filteredPixelY.Blue = (int) (filteredPixelX.Blue * factor);
                    filteredPixelY.Green = (int) (filteredPixelX.Green * factor);

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
            _imageData = newImageData;
        }
        
        public void ConvertToGrey()
        {
            for (var i = 0; i < _height; i++)
            {
                for (var j = 0; j < _width ; j++)
                {
                    //Apply conversion equation
                    var gray = (byte)(.21 * _imageData[i,j].Red + .71 * _imageData[i,j].Green + .071 * _imageData[i,j].Blue);

                    //Set the color of this pixel
                    _imageData[i, j].Red = gray;
                    _imageData[i, j].Green = gray;
                    _imageData[i, j].Blue = gray;
                    
                }
            }
        }
    }
}