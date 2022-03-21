using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_Info
{
    public class QRCode : Image
    {
        private int _maskPattern;
        private int _correctionLevel;
        private readonly int _version;
        private readonly int _quietZoneWidth;
        private readonly int _moduleWidth;
        private int[] _mode;
        private static Dictionary<char, int> _alphanumericTable;


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

        private void CreateEmptyQrCode()
        {
            CreateFinderPatterns(0+_quietZoneWidth, 0+_quietZoneWidth);
            CreateSeparators(0 + _quietZoneWidth, 0 + _quietZoneWidth);
            CreateFinderPatterns(Height - _quietZoneWidth - 7 * _moduleWidth, 0 + _quietZoneWidth);
            CreateSeparators(Height - _quietZoneWidth - 8 * _moduleWidth, 0 + _quietZoneWidth);
            CreateFinderPatterns(0+_quietZoneWidth, 0 + Width - _quietZoneWidth - 7 * _moduleWidth);
            CreateSeparators(0+_quietZoneWidth, 0 + Width - _quietZoneWidth - 8 * _moduleWidth);
            CreateAlignmentPatterns();
            CreateTimingPatterns();
            AddDarkModule();
            AddVersionInformation();
            _maskPattern = 4;
            _correctionLevel = 1;
            AddFormatInformation();

            Functions.FillImageWhite(ImageData);
        }

        private void CreateFinderPatterns(int line, int col)
        {
            var spacing = (_moduleWidth - 1);
            for (int i = line; i < 7 * _moduleWidth + line; i++)
            {
                for (int j = col; j < 7* _moduleWidth + col; j++)
                {
                    if (j < col + _moduleWidth || i < line + _moduleWidth) ImageData[i, j] = new Pixel(0, 0, 0);
                    else if (j > col + 6 * _moduleWidth -1 ||
                        i > line + 6 * _moduleWidth -1) ImageData[i, j] = new Pixel(0, 0, 0);

                    else if (j >= col + 2 * _moduleWidth &&
                        i >= line + 2 * _moduleWidth &&
                        j <= col + 6 * _moduleWidth - 2 * _moduleWidth + spacing &&
                        i <= line + 6 * _moduleWidth - 2 * _moduleWidth + spacing) ImageData[i, j] = new Pixel(0, 0, 0);
                }
            }
        }

        private void CreateSeparators(int line, int col)
        {
            for (int i = line; i < 8 * _moduleWidth + line; i++)
            {
                for (int j = col; j < 8* _moduleWidth + col; j++)
                {
                    ImageData[i, j] ??= new Pixel(255, 255, 255);
                }
            }
        }

        private void CreateTimingPatterns()
        {
            for (int line = 7 * _moduleWidth + _quietZoneWidth; line <= Height - 7 * _moduleWidth - _quietZoneWidth; line++)
            {
                for (int col = 6*_moduleWidth + _quietZoneWidth; col < _moduleWidth * 7 + _quietZoneWidth; col++)
                {
                    if ((line - _quietZoneWidth) / _moduleWidth % 2 == 0) ImageData[line, col] = new Pixel(0, 0, 0);
                    else ImageData[line, col] = new Pixel(255, 255, 255);
                }
            }
            
            for (int col = 7 * _moduleWidth + _quietZoneWidth; col <= Height - 7 * _moduleWidth - _quietZoneWidth; col++)
            {
                for (int line = 6*_moduleWidth + _quietZoneWidth; line < _moduleWidth * 7 + _quietZoneWidth; line++)
                {
                    if ((col - _quietZoneWidth) / _moduleWidth % 2 == 0) ImageData[line, col] = new Pixel(0, 0, 0);
                    else ImageData[line, col] = new Pixel(255, 255, 255);
                }
            }
        }

        private void AddDarkModule()
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
            var formatBinary = EncodeFormatInfo(GetFormatInfo());
            var fixedLine = 8 * _moduleWidth + _quietZoneWidth;
            var fixedCol = 8 * _moduleWidth + _quietZoneWidth;

            var movingCol = 0 + _quietZoneWidth;
            var movingLine = ImageData.GetLength(0) -1 - _quietZoneWidth - (_moduleWidth - 1);
            foreach (var bit in formatBinary)
            {
                if (ImageData[movingLine, fixedCol] != null) movingLine -= 1 * _moduleWidth;
                if (ImageData[fixedLine, movingCol] != null) movingCol += 1 * _moduleWidth;
                
                for (var l = 0; l < _moduleWidth; l++)
                {
                    for (var c = 0; c < _moduleWidth; c++)
                    {
                        ImageData[fixedLine + l, movingCol + c] =
                            bit == 0 ? new Pixel(255, 255, 255) : new Pixel(0, 0, 0);
                        ImageData[movingLine + l, fixedCol + c] =
                            bit == 0 ? new Pixel(255, 255, 255) : new Pixel(0, 0, 0);
                    }
                }
                if (movingLine-1*_moduleWidth == (4 * _version + 9) * _moduleWidth + _quietZoneWidth)
                {
                    movingLine = 9 * _moduleWidth + _quietZoneWidth;
                    continue;
                }
                if (movingCol == 8 * _moduleWidth + _quietZoneWidth)
                {
                    movingCol = ImageData.GetLength(1) - _quietZoneWidth - 8 * _moduleWidth;
                    continue;
                }
                movingLine -= 1 * _moduleWidth;
                movingCol += 1 * _moduleWidth;
            }
        }


        private void AddVersionInformation()
        {
            if (_version < 7) return;
            var bitArray = GetQRVersion();
            for (int i = 0; i < bitArray.Length; i++)
            {
                var newPixel = bitArray[i] == 1 ? new Pixel(0, 0, 0) : new Pixel(255, 255, 255);
                var line = (2 - i % 3) * _moduleWidth + ImageData.GetLength(0) - 11 * _moduleWidth - _quietZoneWidth;
                var col = (5 - i / 3) * _moduleWidth + _quietZoneWidth;
                for (var l = line; l < _moduleWidth + line; l++)
                {
                    for (var c = col; c < _moduleWidth + col; c++)
                    {
                        ImageData[l, c] = newPixel;
                        ImageData[c, l] = newPixel;
                    }
                }
            }
        }

        private void CreateAlignmentPatterns()
        {
            var spacing = (_moduleWidth - 1);
            if (_version < 2) return;
            var coordinates = new List<int>(GetQRAlignmentCoordinates());
            
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

        private int[] GetQRAlignmentCoordinates()
        {
            var lines = Functions.ReadFile("../../../qrSettings.txt").ToArray();
            var selectedLine = lines[_version-2];
            var coordinates = selectedLine.Split(";");
            int[] result = new int[coordinates.Length-1];
            for (var i = 1; i < coordinates.Length; i++)
            {
                result[i - 1] = Convert.ToInt32(coordinates[i])*_moduleWidth + _quietZoneWidth;
            }
            return result;
        }

        private int[] GetQRVersion()
        {
            var lines = Functions.ReadFile("../../../qrVersion.txt").ToArray();
            if (_version < 7) return Array.Empty<int>();
            var selectedLine = lines[_version-7];
            var coordinates = selectedLine.Split(";");
            int[] result = new int[coordinates[1].Length];
            for (var i = 0; i < coordinates[1].Length; i++)
            {
                result[i] = (int) coordinates[1][i] - 48;
            }
            return result;
        }

        public int[] GetFormatInfo()
        {
            var maskBinary = Functions.ConvertIntToBinaryArray(_maskPattern);
            if (maskBinary.Length < 3) maskBinary = Functions.UnShift(maskBinary, 2);
            var correctionLevelBinary = Functions.ConvertIntToBinaryArray(_correctionLevel);
            if (correctionLevelBinary.Length < 2) correctionLevelBinary = Functions.UnShift(correctionLevelBinary, 2);
            return correctionLevelBinary.Concat(maskBinary).ToArray();
        }

        private static int[] EncodeFormatInfo(int[] format)
        {
            var newFormat = Functions.TrimAndPad(format, 14);

            var polynomial = new[] {1, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1};
            var newPoly = Functions.TrimAndPad(polynomial, 14);
            var division = Functions.XOR(newFormat, newPoly);
            division = Functions.Trim(division);
            while (division.Length > 10)
            {
                newPoly = Functions.TrimAndPad(polynomial, division.Length);
                division = Functions.XOR(division, newPoly);
                division = Functions.Trim(division);
            }
            if (division.Length < 10) division = Functions.Pad(division, 10);

            var mask = new[] {1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0};
            
            return Functions.XOR(mask, format.Concat(division).ToArray());
        }

        private int[] EncodeStringData(string word)
        {
            word = word.ToUpper();
            _mode = new[] {0, 0, 1, 0};
            var result = new List<int>(_mode);
            var wordLength = word.Length;

            var wordLengthBits = Functions.UnShift(Functions.ConvertIntToBinaryArray(wordLength),
                _version < 10 ? 9 : _version < 27 ? 11 : 13);
            result.AddRange(wordLengthBits);
            
            foreach (var character in word)
            {
            }

            return result.ToArray();
        }

        public static void InitializeAlphaNumericTable()
        {
            var tableData = Functions.ReadFile("../../../alphanumericTable.txt");
            foreach (var item in tableData)
            {
                var args = item.Split(';');
                _alphanumericTable.Add(Convert.ToChar(args[1]), Convert.ToInt32(args[0]));
            }
        }
    }
}