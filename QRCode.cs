using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_Info
{
    public class QRCode : Image
    {
        private int _maskPattern;
        private int _correctionLevel;
        private int _version;
        private int _quietZoneWidth;
        private int _moduleWidth;


        public QRCode(int version, int quietZoneWidth, int moduleWidth)
        {
            var borderSize = (8 * 2 + (4 * version + 1)) * moduleWidth + 2 * quietZoneWidth;
            ImageData = new Pixel[borderSize, borderSize];
            _version = version;
            _quietZoneWidth = quietZoneWidth;
            _moduleWidth = moduleWidth;
            
            Height = borderSize;
            Width = borderSize;
            Offset = 54;
            BitRgb = 24;

            CreateEmptyQrCode();
        }

        public void CreateEmptyQrCode()
        {
            
            CreateFinderPatterns(0+_quietZoneWidth, 0+_quietZoneWidth);
            CreateFinderPatterns(Height - _quietZoneWidth - 7 * _moduleWidth, 0 + _quietZoneWidth);
            CreateFinderPatterns(0+_quietZoneWidth, 0 + Width - _quietZoneWidth - 7 * _moduleWidth);
            CreateAlignmentPatterns();
            CreateTimingPatterns();
            AddDarkModule();

            Functions.FillImageWhite(ImageData);
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
            for (int line = 7 * _moduleWidth + _quietZoneWidth; line <= Height - 7 * _moduleWidth - _quietZoneWidth; line++)
            {
                for (int col = 6*_moduleWidth + _quietZoneWidth; col < _moduleWidth * 7 + _quietZoneWidth; col++)
                {
                    if ((line - _quietZoneWidth) / _moduleWidth % 2 == 0) ImageData[line, col] = new Pixel(0, 0, 0);
                }
            }
            
            for (int col = 7 * _moduleWidth + _quietZoneWidth; col <= Height - 7 * _moduleWidth - _quietZoneWidth; col++)
            {
                for (int line = 6*_moduleWidth + _quietZoneWidth; line < _moduleWidth * 7 + _quietZoneWidth; line++)
                {
                    if ((col - _quietZoneWidth) / _moduleWidth % 2 == 0) ImageData[line, col] = new Pixel(0, 0, 0);
                }
            }
        }

        public void AddDarkModule()
        {
            var startingLine = (4 * _version + 9)*_moduleWidth +_quietZoneWidth;
            for (int col = 8*_moduleWidth + _quietZoneWidth; col < _moduleWidth * 9 + _quietZoneWidth; col++)
            {
                for (int line = startingLine; line < _moduleWidth + startingLine; line++)
                {
                    ImageData[line, col] = new Pixel(0, 0, 0);
                }
            }
        }

        public void AddFormatInformation()
        {
            
        }

        public void CreateAlignmentPatterns()
        {
            var spacing = (_moduleWidth - 1);
            if (_version < 2) return;
            var coordinates = new List<int>(GetQRAlignmentCoordinates());
            //var coordinates = new List<int>() { _quietZoneWidth + 6 * _moduleWidth, Width - _quietZoneWidth - 6 * _moduleWidth - 1 - spacing};
            // if (_version is > 6 and <= 13) coordinates.Add(Width/2 - _moduleWidth/2);
            // if (_version > 13 )
            // {
            //     var space = (Width - 2 * _moduleWidth * 7 - 2*_quietZoneWidth)/ _moduleWidth / 3;
            //     coordinates.AddRange(new[] {(space + 7)*_moduleWidth + _quietZoneWidth, (space * 2 + 7 + 1)*_moduleWidth + _quietZoneWidth});
            // }


            var arrayOfCoordinates = Functions.DoubleIntCombinations<int[]>(coordinates);
            foreach (var item in arrayOfCoordinates)
            {
                CreateAlignmentPattern(item[0], item[1]);
                CreateAlignmentPattern(item[1], item[0]);
                CreateAlignmentPattern(item[0], item[0]);
                CreateAlignmentPattern(item[1], item[1]);
            }
        }
        
        public void CreateAlignmentPattern(int col, int line)
        {
            var spacing = (_moduleWidth - 1);
            if (ImageData[line, col] != null) return;
            for (int i = line - 2 * _moduleWidth; i <= line + 2 * _moduleWidth + spacing; i++)
            {
                for (int j = col - 2 * _moduleWidth; j <= col + 2 * _moduleWidth + spacing; j++)
                {
                    var fakeLine = (i - (line - 2 * _moduleWidth)) / _moduleWidth;
                    var fakeCol = (j - (col - 2 * _moduleWidth)) / _moduleWidth;

                    ImageData[i, j] = fakeLine == 0 || fakeCol == 0 || fakeLine == 4 || fakeCol == 4
                        ?
                        new Pixel(0, 0, 0)
                        : fakeLine == 2 && fakeCol == 2
                            ? new Pixel(0, 0, 0)
                            : new Pixel(255, 255, 255);
                }
            }
        }

        public int[] GetQRAlignmentCoordinates()
        {
            var lines = Functions.ReadFile("../../../qrSettings.txt").ToArray();
            var selectedLine = lines[_version-2];
            var coordinates = selectedLine.Split(";");
            Console.WriteLine(coordinates);
            int[] result = new int[coordinates.Length-1];
            for (var i = 1; i < coordinates.Length; i++)
            {
                result[i - 1] = Convert.ToInt32(coordinates[i])*_moduleWidth + _quietZoneWidth;
            }
            return result;
        }
    }
}