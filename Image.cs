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
        private Pixel[,] _image;

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
            get => _image;
            set => _image = value;
        }

        public Image(Image im)
        {
            this._type = im.Type;
            this._size = im.Size;
            this._offset = im.Offset;
            this._height = im.Height;
            this._width = im.Width;
            _bitRgb = im.BitRgb;
            this._image = im.ImageData;
        }

        public Image(string type, int size, int offset, int height, int width, int bitRgb, Pixel[,] imge)
        {
            this._type = type;
            this._size = size;
            this._offset = offset;
            this._height = height;
            this._width = width;
            _bitRgb = bitRgb;
            this._image = imge;
        }

        public void DisplayImage()
        {
            for (var i = 0; i < _height; i++)
            {
                for (var j = 0; j < _width; j++)
                {
                    Console.BackgroundColor = _image[i, j].HexString == "000000" ? ConsoleColor.Black : ConsoleColor.White;
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    
        public void Rotate90R()
        {
            var newImage = new Pixel[_image.GetLength(1), _image.GetLength(0)];
            for (var line = 0; line < _image.GetLength(0); line++)
            {
                for (var col = 0; col < _image.GetLength(1); col++)
                {
                    newImage[col, (newImage.GetLength(1) - 1) - line] = _image[line, col];
                }
            }

            _height = _image.GetLength(1);
            _width = _image.GetLength(0);
            _image = newImage;
        }
        
        public void Rotate90L()
        {
            var newImage = new Pixel[_image.GetLength(1), _image.GetLength(0)];
            for (var line = 0; line < _image.GetLength(0); line++)
            {
                for (var col = 0; col < _image.GetLength(1); col++)
                {
                    newImage[(newImage.GetLength(0) - 1) - col, line] = _image[line, col];
                }
            }
            
            _height = _image.GetLength(1);
            _width = _image.GetLength(0);
            _image = newImage;
        }
//add check if possible
        public void Maximize(int factor)
        {
            var newImage = new Pixel[_image.GetLength(0) * factor, _image.GetLength(1) * factor];
            for (var line = 0; line < newImage.GetLength(0); line++)
            {
                for (var col = 0; col < newImage.GetLength(1); col++)
                {
                    newImage[line, col] = new Pixel(_image[line / factor, col / factor]);
                }
            }
            _height *= factor;
            _width *= factor;
            _image = newImage;
        }
        //add check if possible
        public void Minimize(int factor)
        {
            var newImage = new Pixel[_image.GetLength(0) / factor, _image.GetLength(1) / factor];
            for (var line = 0; line < newImage.GetLength(0); line++)
            {
                for (var col = 0; col < newImage.GetLength(1); col++)
                {
                    newImage[line, col] = new Pixel(_image[line * factor, col * factor]);
                }
            }
            _height /= factor;
            _width /= factor;
            _image = newImage;
        }
        public void Mirror()
        {
            var newImage = new Pixel[_image.GetLength(0), _image.GetLength(1)];
            for (var line = 0; line < _image.GetLength(0); line++)
            {
                for (var col = 0; col < _image.GetLength(1); col++)
                {
                    newImage[line, newImage.GetLength(1) - 1 - col] = _image[line, col];
                }
            }
            _image = newImage;
        }
    }
}