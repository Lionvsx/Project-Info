using System;

namespace Project_Info
{
    public class QRCode : Image
    {
        private int _maskPattern;
        private int _correctionLevel;
        private int _version;
        private int _quietZoneWidth;
        private int _moduleWidth;


        public QRCode(int maskPattern, int correctionLevel, Pixel[,] imageData, int version)
        {
            _maskPattern = maskPattern;
            _correctionLevel = correctionLevel;
            ImageData = imageData;
            _version = version;
        }

        public QRCode(int version, int quietZoneWidth, int moduleWidth)
        {
            var borderSize = (8 * 2 + (4 * version + 1)) * moduleWidth + 2 * quietZoneWidth;
            ImageData = new Pixel[borderSize, borderSize];
            Functions.FillImageWhite(ImageData);
            _version = version;
            _quietZoneWidth = quietZoneWidth;
            _moduleWidth = moduleWidth;
            
            Height = borderSize;
            Width = borderSize;
            Offset = 54;
            BitRgb = 24;

            CreateEmptyQrCode();
            Console.WriteLine(borderSize);
        }

        public void CreateEmptyQrCode()
        {
            CreateFinderPatterns(0+_quietZoneWidth, 0+_quietZoneWidth);
            CreateFinderPatterns(Height - _quietZoneWidth - 7 * _moduleWidth, 0 + _quietZoneWidth);
            CreateFinderPatterns(0+_quietZoneWidth, 0 + Width - _quietZoneWidth - 7 * _moduleWidth);
            CreateTimingPatterns();
            AddDarkModule();
        }

        public void CreateFinderPatterns(int line, int col)
        {
            var spacing = (_moduleWidth - 1);
            for (int i = line; i < 7 * _moduleWidth + line; i++)
            {
                for (int j = col; j < 7* _moduleWidth + col; j++)
                {
                    if (j < col + _moduleWidth || i < line + _moduleWidth) ImageData[i, j] = new Pixel(0, 0, 0);
                    if (j > col + 6 * _moduleWidth -1 ||
                        i > line + 6 * _moduleWidth -1) ImageData[i, j] = new Pixel(0, 0, 0);
                    
                    if (j >= col + 2 * _moduleWidth &&
                        i >= line + 2 * _moduleWidth &&
                        j <= col + 6 * _moduleWidth - 2 * _moduleWidth + spacing &&
                        i <= line + 6 * _moduleWidth - 2 * _moduleWidth + spacing) ImageData[i, j] = new Pixel(0, 0, 0);
                }
            }
        }

        public void CreateTimingPatterns()
        {
            for (int line = 7 * _moduleWidth; line <= Height - 7 * _moduleWidth; line++)
            {
                for (int col = 6*_moduleWidth; col < _moduleWidth * 7; col++)
                {
                    if (line / _moduleWidth % 2 == 0) ImageData[line, col] = new Pixel(0, 0, 0);
                }
            }
            
            for (int col = 7 * _moduleWidth; col <= Height - 7 * _moduleWidth; col++)
            {
                for (int line = 6*_moduleWidth; line < _moduleWidth * 7; line++)
                {
                    if (col / _moduleWidth % 2 == 0) ImageData[line, col] = new Pixel(0, 0, 0);
                }
            }
        }

        public void AddDarkModule()
        {
            var startingLine = Height - 7 * _moduleWidth;
            for (int col = 8*_moduleWidth; col < _moduleWidth * 9; col++)
            {
                for (int line = startingLine; line < _moduleWidth + startingLine; line++)
                {
                    ImageData[line, col] = new Pixel(0, 0, 0);
                }
            }
        }
    }
}