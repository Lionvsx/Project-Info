using System.Drawing;

namespace Project_Info
{
    public class Pixel
    {
        public Pixel(int red, int blue, int green)
        {
            this.Red = red;
            this.Blue = blue;
            this.Green = green;
        }

        public Pixel()
        {
            Red = 0;
            Blue = 0;
            Green = 0;
        }

        public Pixel(Pixel pixel)
        {
            Red = pixel.Red;
            Green = pixel.Green;
            Blue = pixel.Blue;
        }

        public int Red { get; set; }

        public int Blue { get; set; }

        public int Green { get; set; }

        public string HexString
        {
            get
            {
                Color myColor = Color.FromArgb(Red, Green, Blue);
                string hex = myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2");
                return hex;
            }
        }

        public bool IsBlack => Red == 0 && Green == 0 && Blue == 0;
    }
}