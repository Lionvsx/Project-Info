namespace Project_Info
{
    public class QRCode : Image
    {
        private int _maskPattern;
        private int _correctionLevel;
        private int _version;


        public QRCode(int maskPattern, int correctionLevel, Pixel[,] imageData, int version)
        {
            _maskPattern = maskPattern;
            _correctionLevel = correctionLevel;
            ImageData = imageData;
            _version = version;
        }

        public QRCode()
        {
            
        }
    }
}