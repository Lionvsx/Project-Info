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
            get => _size;
            set => _size = value;
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


    }
}