namespace Project_Info
{
    public class Image
    {
        private string type;
        private int size;
        private int offset;
        private int height;
        private int width;
        private int bitRGB;
        private Pixel[,] imge;

        public string Type
        {
            get => type;
            set => type = value;
        }

        public int Size
        {
            get => size;
            set => size = value;
        }

        public int Offset
        {
            get => offset;
            set => offset = value;
        }

        public int Height
        {
            get => height;
            set => height = value;
        }

        public int Width
        {
            get => width;
            set => width = value;
        }

        public int BitRgb
        {
            get => bitRGB;
            set => bitRGB = value;
        }

        public Pixel[,] Imge
        {
            get => imge;
            set => imge = value;
        }

        public Image(string type, int size, int offset, int height, int width, int bitRgb, Pixel[,] imge)
        {
            this.type = type;
            this.size = size;
            this.offset = offset;
            this.height = height;
            this.width = width;
            bitRGB = bitRgb;
            this.imge = imge;
        }
    }
}