using System.Drawing;

namespace Project_Info
{
    public class Pixel
    {
        private int _red;
        private int _blue;
        private int _green;

        public Pixel(int red, int blue, int green)
        {
            this._red = red;
            this._blue = blue;
            this._green = green;
        }

        public Pixel()
        {
            this._red = 0;
            this._blue = 0;
            this._green = 0;
        }

        public Pixel(Pixel pixel)
        {
            _red = pixel.Red;
            _green = pixel.Green;
            _blue = pixel.Blue;
        }

        public int Red
        {
            get => _red;
            set => _red = value;
        }

        public int Blue
        {
            get => _blue;
            set => _blue = value;
        }

        public int Green
        {
            get => _green;
            set => _green = value;
        }

        public string HexString
        {
            get
            {
                Color myColor = Color.FromArgb(_red, _green, _blue);
                string hex = myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2");
                return hex;
            }
        }
    }
}