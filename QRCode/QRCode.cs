using System;
using System.Collections.Generic;
using System.Linq;
using Project_Info.Console_Display;
using STH1123.ReedSolomon;

namespace Project_Info.QRCode
{
    public class QRCode : Image
    {
        private int _maskPattern;
        private int _correctionLevel;
        private int _version;
        private readonly int _quietZoneWidth;
        private readonly int _moduleWidth;
        private int[] _mode;
        private static Dictionary<char, int> _alphanumericTable = new();
        private bool[,] _notFunctionModules;
        private bool[,] functionModulesMasksOptimization;
        private int _numberDataCodewords;
        private int _numberEcCodewords;
        private List<int> _wordEncodedData;
        private int[] _qrCodeData;
        private int _numberBlocksGroup1;
        private int _numberBlocksGroup2;
        private int _numberEcPerBlock;
        private int _numberDataPerBlockGrp2;
        private int _numberDataPerBlockGrp1;


        public QRCode(int version, int quietZoneWidth, int moduleWidth, int maskPattern, int correctionLevel, string message)
        {
            var borderSize = (8 * 2 + (4 * version + 1)) * moduleWidth + 2 * quietZoneWidth;
            ImageData = new Pixel[borderSize, borderSize];
            _notFunctionModules = new bool[borderSize, borderSize];
            _version = version;
            _quietZoneWidth = quietZoneWidth;
            _moduleWidth = moduleWidth;
            
            Height = borderSize;
            Width = borderSize;
            Offset = 54;
            BitRgb = 24;
            
            _maskPattern = maskPattern;
            _correctionLevel = correctionLevel;

            CreateEmptyQrCode();
            EncodeStringData(message);
            GetErrorData();
            DataEncoding();
            
            
            Functions.FillImageWhite(ImageData);
            AddMask();
        }

        public QRCode(string message, int correctionLevel =  1)
        {
            _correctionLevel = correctionLevel;
            _quietZoneWidth = 6;
            _moduleWidth = 3;
            
            GetVersionFromString(message);
            
            var borderSize = (8 * 2 + (4 * _version + 1)) * _moduleWidth + 2 * _quietZoneWidth;
            ImageData = new Pixel[borderSize, borderSize];
            _notFunctionModules = new bool[borderSize, borderSize];
            Height = borderSize;
            Width = borderSize;
            Offset = 54;
            BitRgb = 24;

            _maskPattern = 0;
            
            CreateBestMaskQRCode(message);
            
            AddFormatInformation();

            DataEncoding();
            Functions.FillImageWhite(ImageData);

            
            AddMask();
        }

        private QRCode(int version, int quietZoneWidth, int moduleWidth)
        {
            _version = version;
            _quietZoneWidth = quietZoneWidth;
            _moduleWidth = moduleWidth;
            
            var borderSize = (8 * 2 + (4 * _version + 1)) * _moduleWidth + 2 * _quietZoneWidth;
            ImageData = new Pixel[borderSize, borderSize];
            _notFunctionModules = new bool[borderSize, borderSize];
            Height = borderSize;
            Width = borderSize;
            Offset = 54;
            BitRgb = 24;
            
            CreateEmptyQrCode();
        }

        private void GetVersionFromString(string message)
        {
            var requiredBits = 17 + (message.Length % 2) * 6 + (message.Length / 2) * 11;
            var requiredBytes = requiredBits % 8 == 0 ? requiredBits / 8 : requiredBits / 8 + 1;
            GetCodeDataLength(requiredBytes);
        }

        private void CreateBestMaskQRCode(string message)
        {
            CreateFinderPatterns(0+_quietZoneWidth, 0+_quietZoneWidth);
            CreateSeparators(0 + _quietZoneWidth, 0 + _quietZoneWidth);
            CreateFinderPatterns(Height - _quietZoneWidth - 7 * _moduleWidth, 0 + _quietZoneWidth);
            CreateSeparators(Height - _quietZoneWidth - 8 * _moduleWidth, 0 + _quietZoneWidth);
            CreateFinderPatterns(0+_quietZoneWidth, 0 + Width - _quietZoneWidth - 7 * _moduleWidth);
            CreateSeparators(0+_quietZoneWidth, 0 + Width - _quietZoneWidth - 8 * _moduleWidth);

            var borderSize = (8 * 2 + (4 * _version + 1));
            var masksMatrix = new bool[borderSize, borderSize, 8];
            functionModulesMasksOptimization = new bool[borderSize, borderSize];
            
            CreateMaskFinderPatterns(0, 0, masksMatrix);
            CreateMasksSeparators(0, 0);
            CreateMaskFinderPatterns(borderSize - 7, 0, masksMatrix);
            CreateMasksSeparators(borderSize - 8, 0);
            CreateMaskFinderPatterns(0, borderSize - 7, masksMatrix);
            CreateMasksSeparators(0, borderSize - 8);
            
            CreateAlignmentPatternsWithMasks(masksMatrix);
            
            
            
            CreateTimingPatterns();
            CreateMasksTimingPatterns(masksMatrix, borderSize);
            
            AddDarkModule();
            AddMasksDarkModule(masksMatrix);
            
            AddVersionInformationWithMasks(masksMatrix);
            
            
            
            EncodeStringData(message);
            AddErrorData();
            
            AddMasksFormatInformation(masksMatrix);
            MaskDataEncoding(masksMatrix);
            ApplyAllMasksMatrix(masksMatrix);
            
            ConsoleFunctions.DisplayBoolQRCode(masksMatrix);
            Console.ReadKey();
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
            SetCodeDataLengthInfo();
            AddFormatInformation();
        }

        private int[] ByteEncodedData => Functions.ConvertBitArrayToByteArray(_wordEncodedData.ToArray());

        private void CreateFinderPatterns(int line, int col)
        {
            var spacing = (_moduleWidth - 1);
            for (int i = line; i < 7 * _moduleWidth + line; i++)
            {
                for (int j = col; j < 7* _moduleWidth + col; j++)
                {
                    if (j < col + _moduleWidth || i < line + _moduleWidth) ImageData[i, j] = new Pixel(0, 0, 0);
                    else if (j > col + 6 * _moduleWidth - 1 ||
                             i > line + 6 * _moduleWidth - 1) ImageData[i, j] = new Pixel(0, 0, 0);

                    else if (j >= col + 2 * _moduleWidth &&
                        i >= line + 2 * _moduleWidth &&
                        j <= col + 6 * _moduleWidth - 2 * _moduleWidth + spacing &&
                        i <= line + 6 * _moduleWidth - 2 * _moduleWidth + spacing) ImageData[i, j] = new Pixel(0, 0, 0);
                }
            }
        }
        
        private void CreateMaskFinderPatterns(int line, int col, bool[,,] masksMatrix)
        {
            for (var k = 0; k < 8; k++)
            {
                for (var i = line; i < 7 + line; i++)
                {
                    for (var j = col; j < 7 + col; j++)
                    {
                        if (j <= col || i <= line) masksMatrix[i, j, k] = true;
                        else if (j > col + 5 ||
                                 i > line + 5) masksMatrix[i, j, k] = true;
                        else if (j >= col + 2 &&
                                 i >= line + 2 &&
                                 j <= col + 4 &&
                                 i <= line + 4) masksMatrix[i, j, k] = true;
                    }
                }
            }
        }
        
        private void CreateMasksSeparators(int line, int col)
        {
            for (int i = line; i < 8 + line; i++)
            {
                for (int j = col; j < 8 + col; j++)
                {
                    functionModulesMasksOptimization[i, j] = true;
                }
            }
        }

        private void CreateSeparators(int line, int col)
        {
            for (int i = line; i < 8 * _moduleWidth + line; i++)
            {
                for (int j = col; j < 8 * _moduleWidth + col; j++)
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
        
        private void CreateMasksTimingPatterns(bool[,,] masksMatrix, int borderSize)
        {
            for (var k = 0; k < 9; k++)
            {
                for (var line = 7; line <= borderSize - 7; line++)
                {
                    if (k == 8)
                    {
                        functionModulesMasksOptimization[line, 6] = true;
                        continue;
                    }
                    if (line % 2 == 0) masksMatrix[line, 6, k] = true;
                }
            
                for (var col = 7; col <= borderSize - 7; col++)
                {
                    if (k == 8)
                    {
                        functionModulesMasksOptimization[6, col] = true;
                        continue;
                    }
                    if (col % 2 == 0) masksMatrix[6, col, k] = true;
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
        
        private void AddMasksDarkModule(bool[,,] masksMatrix)
        {
            var startingLine = (4 * _version + 9);
            functionModulesMasksOptimization[startingLine, 8] = true;
            for (var k = 0; k < 8; k++)
            {
                masksMatrix[startingLine, 8, k] = true;
            }
        }

        public void AddFormatInformation()
        {
            var formatBinary = GetFormatInfo();
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
                if (movingLine - 1 * _moduleWidth == (4 * _version + 9) * _moduleWidth + _quietZoneWidth &&
                    movingCol == 7 * _moduleWidth + _quietZoneWidth)
                {
                    movingLine = 8 * _moduleWidth + _quietZoneWidth;
                    movingCol = ImageData.GetLength(1) - _quietZoneWidth - 8 * _moduleWidth;
                    continue;
                }

                movingLine -= 1 * _moduleWidth;
                movingCol += 1 * _moduleWidth;
            }
        }
        
        public void AddMasksFormatInformation(bool[,,] masksMatrix)
        {
            var formatBinaryVector = GetFormatInfoVector();
            const int fixedLine = 8;
            const int fixedCol = 8;

            for (int k = 0; k < 8; k++)
            {
                var movingCol = 0;
                var movingLine = masksMatrix.GetLength(0) - 1;
                var formatBinary = formatBinaryVector[k];
                foreach (var bit in formatBinary)
                {
                    
                    if (masksMatrix[movingLine, fixedCol, k]) movingLine -= 1;
                    if (masksMatrix[fixedLine, movingCol, k]) movingCol += 1;

                    masksMatrix[movingLine, fixedCol, k] = bit == 1;
                    masksMatrix[fixedLine, movingCol, k] = bit == 1;
                    if (k == 0)
                    {
                        functionModulesMasksOptimization[movingLine, fixedCol] = true;
                        functionModulesMasksOptimization[fixedLine, movingCol] = true;
                    }
                    
                    if (movingLine - 1 == (4 * _version + 9) &&
                        movingCol == 7)
                    {
                        movingLine = 8;
                        movingCol = masksMatrix.GetLength(1) - 8;
                        continue;
                    }

                    movingLine -= 1;
                    movingCol += 1;
                }
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
        
        private void AddVersionInformationWithMasks(bool[,,] masksMatrix)
        {
            if (_version < 7) return;
            var bitArray = GetQRVersion();
            for (int i = 0; i < bitArray.Length; i++)
            {
                var newPixel = bitArray[i] == 1 ? new Pixel(0, 0, 0) : new Pixel(255, 255, 255);
                var line = (2 - i % 3) * _moduleWidth + ImageData.GetLength(0) - 11 * _moduleWidth - _quietZoneWidth;
                var col = (5 - i / 3) * _moduleWidth + _quietZoneWidth;

                var maskLine = (2 - i % 3) + masksMatrix.GetLength(0) - 11;
                var maskCol = (5 - i / 3);
                for (var l = line; l < _moduleWidth + line; l++)
                {
                    for (var c = col; c < _moduleWidth + col; c++)
                    {
                        ImageData[l, c] = newPixel;
                        ImageData[c, l] = newPixel;
                    }
                }
                for (int k = 0; k < 8; k++)
                {
                    if (k == 0)
                    {
                        functionModulesMasksOptimization[maskLine, maskCol] = true;
                        functionModulesMasksOptimization[maskCol, maskLine] = true;
                    }
                    masksMatrix[maskLine, maskCol, k] = bitArray[i] == 1;
                    masksMatrix[maskCol, maskLine, k] = bitArray[i] == 1;
                }
            }
        }

        private void CreateAlignmentPatterns()
        {
            var spacing = (_moduleWidth - 1);
            if (_version < 2) return;
            var coordinates = new List<int>(GetQRAlignmentCoordinates());
            
            var arrayOfCoordinates = Functions.DoubleIntCombinations(coordinates);
            foreach (var item in arrayOfCoordinates)
            {
                CreateAlignmentPattern(item[0]*_moduleWidth + _quietZoneWidth, item[1]*_moduleWidth + _quietZoneWidth);
                CreateAlignmentPattern(item[1]*_moduleWidth + _quietZoneWidth, item[0]*_moduleWidth + _quietZoneWidth);
                CreateAlignmentPattern(item[0]*_moduleWidth + _quietZoneWidth, item[0]*_moduleWidth + _quietZoneWidth);
                CreateAlignmentPattern(item[1]*_moduleWidth + _quietZoneWidth, item[1]*_moduleWidth + _quietZoneWidth);
            }
        }
        
        private void CreateAlignmentPatternsWithMasks(bool[,,] masksMatrix)
        {
            if (_version < 2) return;
            var coordinates = new List<int>(GetQRAlignmentCoordinates());
            
            var arrayOfCoordinates = Functions.DoubleIntCombinations(coordinates);
            foreach (var item in arrayOfCoordinates)
            {
                CreateAlignmentPattern(item[0]*_moduleWidth + _quietZoneWidth, item[1]*_moduleWidth + _quietZoneWidth);
                CreateAlignmentPattern(item[1]*_moduleWidth + _quietZoneWidth, item[0]*_moduleWidth + _quietZoneWidth);
                CreateAlignmentPattern(item[0]*_moduleWidth + _quietZoneWidth, item[0]*_moduleWidth + _quietZoneWidth);
                CreateAlignmentPattern(item[1]*_moduleWidth + _quietZoneWidth, item[1]*_moduleWidth + _quietZoneWidth);
                CreateMasksAlignmentPattern(item[0], item[1], masksMatrix);
                CreateMasksAlignmentPattern(item[1], item[0], masksMatrix);
                CreateMasksAlignmentPattern(item[0], item[0], masksMatrix);
                CreateMasksAlignmentPattern(item[1], item[1], masksMatrix);
            }
        }
        

        private void CreateMasksAlignmentPattern(int col, int line, bool[,,] masksMatrix)
        {
            if (masksMatrix[line, col, 0]) return;
            for (int k = 0; k < 8; k++)
            {
                for (int i = line - 2; i <= line + 2; i++)
                {
                    for (int j = col - 2; j <= col + 2; j++)
                    {
                        if (k == 0)
                        {
                            functionModulesMasksOptimization[i, j] = true;
                        }
                        var fakeLine = i - (line - 2);
                        var fakeCol = j - (col - 2);
                        masksMatrix[i, j, k] = fakeLine == 0 || fakeCol == 0 || fakeLine == 4 || fakeCol == 4 ||
                                               (fakeLine == 2 && fakeCol == 2);
                    }
                }
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
            var lines = Functions.ReadFile("../../../QRCode/qrSettings.txt").ToArray();
            var selectedLine = lines[_version-2];
            var coordinates = selectedLine.Split(";");
            int[] result = new int[coordinates.Length-1];
            for (var i = 1; i < coordinates.Length; i++)
            {
                result[i - 1] = Convert.ToInt32(coordinates[i]);
            }
            return result;
        }

        // ReSharper disable once InconsistentNaming
        private int[] GetQRVersion()
        {
            var lines = Functions.ReadFile("../../../QRCode/qrVersion.txt").ToArray();
            if (_version < 7) return Array.Empty<int>();
            var selectedLine = lines[_version-7];
            var coordinates = selectedLine.Split(";");
            int[] result = new int[coordinates[1].Length];
            for (var i = 0; i < coordinates[1].Length; i++)
            {
                result[i] = coordinates[1][i] - 48;
            }
            return result;
        }

        private void SetCodeDataLengthInfo()
        {
            var lines = Functions.ReadFile("../../../QRCode/qrCodeDataLength.txt").ToArray();
            var lineIndex = _correctionLevel switch
            {
                1 => 0,
                3 => 2,
                2 => 3,
                0 => 1,
                _ => throw new ArgumentOutOfRangeException()
            };

            var selectedLine = lines[(_version - 1) * 4 + lineIndex];
            
            var infos = selectedLine.Split(";");
            var result = new int[infos.Length-2];
            for (var i = 1; i < infos.Length-1; i++)
            {
                result[i - 1] = Convert.ToInt32(infos[i]);
            }

            _numberDataCodewords = result[0];
            _numberEcCodewords = result[1] * result[2] + result[1] * result[4];
            
            _numberBlocksGroup1 = result[2];
            _numberBlocksGroup2 = result[4];
            
            _numberEcPerBlock = result[1];
            _numberDataPerBlockGrp1 = result[3];
            _numberDataPerBlockGrp2 = result[5];

        }
        
        private void GetCodeDataLength(int requiredBytes)
        {
            var lines = Functions.ReadFile("../../../QRCode/qrCodeDataLength.txt").ToArray();
            var startIndex = _correctionLevel switch
            {
                1 => 0,
                3 => 2,
                2 => 3,
                0 => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(_correctionLevel))
            };
            
            for (var index = startIndex; index < lines.Length; index += 4)
            {
                var infos = lines[index].Split(";");
                if (index > 40 * 4) throw new ArgumentOutOfRangeException(nameof(requiredBytes));
                if (Convert.ToInt32(infos[1]) < requiredBytes) continue;
                
                _version = Convert.ToInt32(infos[0].Split("-")[0]);
                
                var result = new int[infos.Length-2];
                for (var i = 1; i < infos.Length-1; i++)
                {
                    result[i - 1] = Convert.ToInt32(infos[i]);
                }
                
                _numberDataCodewords = result[0];
                _numberEcCodewords = result[1] * result[2] + result[1] * result[4];
            
                _numberBlocksGroup1 = result[2];
                _numberBlocksGroup2 = result[4];
            
                _numberEcPerBlock = result[1];
                _numberDataPerBlockGrp1 = result[3];
                _numberDataPerBlockGrp2 = result[5];
                

                break;
            }
        }


        private void AddErrorData()
        {
            var field = new GenericGF(285, 256, 0);
            var rse = new ReedSolomonEncoder(field);
            var maxNumberDataPerBlock = Math.Max(_numberDataPerBlockGrp1, _numberDataPerBlockGrp2);
            
            var encodedData = new int[_numberBlocksGroup1 + _numberBlocksGroup2, maxNumberDataPerBlock];
            var errorEncodedData = new int[_numberBlocksGroup1 + _numberBlocksGroup2, _numberEcPerBlock];

            // Error Data Group 1
            for (int i = 0; i < _numberBlocksGroup1; i++)
            {
                var errorFields = _numberEcPerBlock;
                
                var dataBlock = ByteEncodedData[(i * _numberDataPerBlockGrp1)..((i + 1) * _numberDataPerBlockGrp1)];
                var zerosArray = Enumerable.Repeat(0, errorFields);

                var byteArray = dataBlock.Concat(zerosArray).ToArray();
                rse.Encode(byteArray, errorFields);
                
                for (var j = 0; j < maxNumberDataPerBlock; j++)
                {
                    encodedData[i, j] = j >= _numberDataPerBlockGrp1 ? -1 : byteArray[j];
                }
                for (var j = 0; j < _numberEcPerBlock; j++)
                {
                    errorEncodedData[i, j] = byteArray[j + _numberDataPerBlockGrp1];
                }
            }
            
            //Error data group 2
            for (int i = 0; i < _numberBlocksGroup2; i++)
            {
                var errorFields = _numberEcPerBlock;
                
                var dataBlock = ByteEncodedData[(i * _numberDataPerBlockGrp2 + _numberBlocksGroup1 * _numberDataPerBlockGrp1)..((i + 1) * _numberDataPerBlockGrp2 + _numberBlocksGroup1 * _numberDataPerBlockGrp1)];
                var zerosArray = Enumerable.Repeat(0, errorFields);

                var byteArray = dataBlock.Concat(zerosArray).ToArray();
                rse.Encode(byteArray, errorFields);
                
                for (var j = 0; j < maxNumberDataPerBlock; j++)
                {
                    encodedData[i + _numberBlocksGroup1, j] = j >= _numberDataPerBlockGrp2 ? -1 : byteArray[j];
                }
                for (var j = 0; j < _numberEcPerBlock; j++)
                {
                    errorEncodedData[i + _numberBlocksGroup1, j] = byteArray[j + _numberDataPerBlockGrp2];
                }
            }
            
            var qrFinalData = new List<int>();
            
            for (var i = 0; i < maxNumberDataPerBlock; i++)
            {
                for (var j = 0; j < _numberBlocksGroup1 + _numberBlocksGroup2; j++)
                {
                    if (encodedData[j, i] == -1) continue;
                    qrFinalData.Add(encodedData[j, i]);
                }
            }
            
            for (var i = 0; i < _numberEcPerBlock; i++)
            {
                for (var j = 0; j < _numberBlocksGroup1 + _numberBlocksGroup2; j++)
                {
                    qrFinalData.Add(errorEncodedData[j, i]);
                }
            }


            var lines = Functions.ReadFile("../../../QRCode/qrRemainderBits.txt").ToArray();
            var selectedLine = lines[_version - 1];
            var coordinates = selectedLine.Split(";");
            var remainder = Convert.ToInt32(coordinates[1]);
            
            var bitArray = Functions.ConvertByteArrayToBitArray(qrFinalData.ToArray()).ToList();
            for (var i = 0; i < remainder; i++)
            {
                bitArray.Add(0);
            }
            _qrCodeData = bitArray.ToArray();
        }
        
        private void GetErrorData()
        {
            var field = new GenericGF(285, 256, 0);
            var rse = new ReedSolomonEncoder(field);
            //Max byte value = 255 (OxFF)
            
            var errorFields = _numberEcCodewords;
            
            var zerosArray = Enumerable.Repeat((int) 0, errorFields);
            var byteArray = ByteEncodedData.Concat(zerosArray).ToArray();
            var intByteArray = Array.ConvertAll(byteArray, x => (int) x);
            rse.Encode(intByteArray, errorFields);
            
            // Add remainder bits if needed
            var lines = Functions.ReadFile("../../../QRCode/qrRemainderBits.txt").ToArray();
            var selectedLine = lines[_version - 1];
            var coordinates = selectedLine.Split(";");
            var remainder = Convert.ToInt32(coordinates[1]);
            
            var bitArray = Functions.ConvertByteArrayToBitArray(intByteArray).ToList();
            for (var i = 0; i < remainder; i++)
            {
                bitArray.Add(0);
            }

            _qrCodeData = bitArray.ToArray();
        }


        private int[] GetFormatInfoOld()
        {
            var maskBinary = Functions.ConvertIntToBinaryArray(_maskPattern);
            if (maskBinary.Length < 3) maskBinary = Functions.UnShift(maskBinary, 3);
            var correctionLevelBinary = Functions.ConvertIntToBinaryArray(_correctionLevel);
            if (correctionLevelBinary.Length < 2) correctionLevelBinary = Functions.UnShift(correctionLevelBinary, 2);
            return correctionLevelBinary.Concat(maskBinary).ToArray();
        }

        private int[] GetFormatInfo()
        {
            var lines = Functions.ReadFile("../../../QRCode/qrFormat.txt").ToArray();
            var lineIndex = _correctionLevel switch
            {
                1 => 0*8,
                3 => 2*8,
                2 => 3*8,
                0 => 1*8,
                _ => throw new ArgumentOutOfRangeException()
            };

            var selectedLine = lines[lineIndex + _maskPattern];
            
            var infos = selectedLine.Split(";");
            return infos[2].Select(a => a - '0').ToArray();;
        }
        
        private List<int[]> GetFormatInfoVector()
        {
            var outputList = new List<int[]>();
            var lines = Functions.ReadFile("../../../QRCode/qrFormat.txt").ToArray();
            var lineIndex = _correctionLevel switch
            {
                1 => 0*8,
                3 => 2*8,
                2 => 3*8,
                0 => 1*8,
                _ => throw new ArgumentOutOfRangeException()
            };

            var selectedLines = lines[lineIndex..(lineIndex + 8)];
            foreach (var line in selectedLines)
            {
                var infos = line.Split(";");
                outputList.Add(infos[2].Select(a => a - '0').ToArray());
            }
            return outputList;
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

        private void EncodeStringData(string word)
        {
            word = word.ToUpper();
            _mode = new[] {0, 0, 1, 0};
            var result = new List<int>(_mode);
            var wordLength = word.Length;

            var wordLengthBits = Functions.IntToDesiredLengthBit(wordLength,
                _version < 10 ? 9 : _version < 27 ? 11 : 13);
            result.AddRange(wordLengthBits);

            for (var i = 0; i < word.Length; i+=2)
            {
                if (i % 2 == 0 && i != word.Length - 1)
                {
                    _alphanumericTable.TryGetValue(word[i], out var highValue);
                    _alphanumericTable.TryGetValue(word[i + 1], out var lowValue);

                    result.AddRange(Functions.IntToDesiredLengthBit(highValue * 45 + lowValue, 11));
                }
                if (i == wordLength - 1 && wordLength % 2 == 1)
                {
                    _alphanumericTable.TryGetValue(word[i], out var value);
                    result.AddRange(Functions.IntToDesiredLengthBit(value, 6));
                }
            }
            
            
            //ADD Terminator
            var count = 0;
            while (result.Count < _numberDataCodewords * 8 && count < 4)
            {
                result.Add(0);
                count++;
            }
            //Add more 0 to make multiple of 8
            result = result.Count % 8 != 0
                ? Functions.Pad(result.ToArray(), result.Count + (8 - result.Count % 8)).ToList()
                : result;
            //Pad bytes if string still too short
            if (result.Count < _numberDataCodewords * 8)
            {
                var byte1 = Functions.IntToDesiredLengthBit(236, 8);
                var byte2 = Functions.IntToDesiredLengthBit(17, 8);

                var iterations = (_numberDataCodewords * 8 - result.Count);
                for (int i = 0; i < iterations/8; i++)
                {
                    result.AddRange(i % 2 == 0 ? byte1 : byte2);
                }
            }
            

            _wordEncodedData = result;
        }

        private void AddMask()
        {
            //Iterate through each pixel of the ImageData matrix
            for (int line = 0 + _quietZoneWidth; line < ImageData.GetLength(0); line+=_moduleWidth)
            {
                for (int col = 0 + _quietZoneWidth; col < ImageData.GetLength(1); col+=_moduleWidth)
                {
                    int fLine = (line - _quietZoneWidth)/_moduleWidth;
                    int fCol = (col - _quietZoneWidth)/_moduleWidth;
                    if (_notFunctionModules[line, col])
                    {
                        for (var i = line; i < line + _moduleWidth; i++)
                        {
                            for (var j = col; j < col + _moduleWidth; j++)
                            {
                                ImageData[i, j] = _maskPattern switch
                                {
                                    0 => (fLine + fCol) % 2 == 0
                                        ? Functions.InvertPixel(ImageData[i, j])
                                        : ImageData[i, j],
                                    1 => (fLine) % 2 == 0
                                        ? Functions.InvertPixel(ImageData[i, j])
                                        : ImageData[i, j],
                                    2 => (fCol) % 3 == 0
                                        ? Functions.InvertPixel(ImageData[i, j])
                                        : ImageData[i, j],
                                    3 => ((fLine + fCol) % 3 == 0)
                                        ? Functions.InvertPixel(ImageData[i, j])
                                        : ImageData[i, j],
                                    4 =>((fLine / 2 + fCol / 3) % 2 == 0)
                                        ? Functions.InvertPixel(ImageData[i, j])
                                        : ImageData[i, j],
                                    5 => ((fLine * fCol) % 2 + (fLine * fCol) % 3 == 0)
                                        ? Functions.InvertPixel(ImageData[i, j])
                                        : ImageData[i, j],
                                    6 => (((fLine * fCol) % 2 + (fLine * fCol) % 3) % 2 == 0)
                                        ? Functions.InvertPixel(ImageData[i, j])
                                        : ImageData[i, j],
                                    7 => (((fLine * fCol) % 3 + (fLine + fCol) % 2) % 2 == 0)
                                        ? Functions.InvertPixel(ImageData[i, j])
                                        : ImageData[i, j],
                                    _ => throw new ArgumentException("Mask pattern not found")
                                };
                            }
                        }
                    }
                }
            }
        }
        
        private void ApplyAllMasksMatrix(bool[,,] masksMatrix)
        {
            for (int line = 0; line < masksMatrix.GetLength(0); line += 1) 
            {
                for (int col = 0; col < masksMatrix.GetLength(1); col += 1) 
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if (functionModulesMasksOptimization[line, col]) continue;
                        
                        var condition = k switch
                        {
                            0 => (line + col) % 2 == 0,
                            1 => (line) % 2 == 0,
                            2 => (col) % 3 == 0,
                            3 => ((line + col) % 3 == 0),
                            4 => ((line / 2 + col / 3) % 2 == 0),
                            5 => ((line * col) % 2 + (line * col) % 3 == 0),
                            6 => (((line * col) % 2 + (line * col) % 3) % 2 == 0),
                            7 => (((line * col) % 3 + (line + col) % 2) % 2 == 0),
                            _ => throw new ArgumentException("Mask pattern not found")
                        };
                        
                        masksMatrix[line, col, k] = condition ? !masksMatrix[line, col, k] : masksMatrix[line, col, k];
                        
                        /*masksMatrix[line, col, k] = k switch
                        {
                            0 => (line + col) % 2 == 0
                                ? !masksMatrix[line, col, k]
                                : masksMatrix[line, col, k],
                            1 => (line) % 2 == 0
                                ? !masksMatrix[line, col, k]
                                : masksMatrix[line, col, k],
                            2 => (col) % 3 == 0
                                ? !masksMatrix[line, col, k]
                                : masksMatrix[line, col, k],
                            3 => ((line + col) % 3 == 0)
                                ? !masksMatrix[line, col, k]
                                : masksMatrix[line, col, k],
                            4 =>((line / 2 + col / 3) % 2 == 0)
                                ? !masksMatrix[line, col, k]
                                : masksMatrix[line, col, k],
                            5 => ((line * col) % 2 + (line * col) % 3 == 0)
                                ? !masksMatrix[line, col, k]
                                : masksMatrix[line, col, k],
                            6 => (((line * col) % 2 + (line * col) % 3) % 2 == 0)
                                ? !masksMatrix[line, col, k]
                                : masksMatrix[line, col, k],
                            7 => (((line * col) % 3 + (line + col) % 2) % 2 == 0)
                                ? !masksMatrix[line, col, k]
                                : masksMatrix[line, col, k],
                            _ => throw new ArgumentException("Mask pattern not found")
                        };*/
                    }
                    
                }
            }
        }
        
        
        private void MaskDataEncoding(bool[,,] masksMatrix)
        {
            var chain = _qrCodeData;
            var upp = true;
            var cpt = 0;
            var skip = false;
                for (var col = masksMatrix.GetLength(1) - 1; col > 0; col -=2)
                {
                    if (cpt >= chain.Length-1) break;
                    if (upp)
                    {
                        if (cpt >= chain.Length-1) break;
                        for (var line = masksMatrix.GetLength(0) - 1; line >= 0; line--)
                        {
                            if (cpt >= chain.Length-1) break;
                            
                            //SKIP TIMING PATTERNS
                            if (col <= 7 && skip == false)
                            {
                                col--;
                                skip = true;
                            }
                            
                            if (!functionModulesMasksOptimization[line, col]) 
                            {
                                if (chain[cpt] == 1)
                                {
                                    for (var k = 0; k < 8; k++)
                                    {
                                        masksMatrix[line, col, k] = true;
                                    }
                                }
                                cpt++;
                            }
                            
                            if (functionModulesMasksOptimization[line, col - 1]) continue;
                            if (chain[cpt] == 1)
                            {
                                for (var k = 0; k < 8; k++)
                                {
                                    masksMatrix[line, col - 1, k] = true;
                                }
                            }

                            cpt++;
                        }
                    }
                    else
                    {
                        if (cpt >= chain.Length-1) break;
                        for (var line = 0; line <= masksMatrix.GetLength(0) - 1; line++) 
                        {
                            if (cpt >= chain.Length-1) break;
                            //SKIP TIMING PATTERNS
                            if (col <= 7 && skip == false)
                            {
                                col --;
                                skip = true;
                            }
                            
                            if (!functionModulesMasksOptimization[line, col]) 
                            {
                                for (var k = 0; k < 8; k++)
                                {
                                    masksMatrix[line, col, k] = chain[cpt] == 1;
                                }
                                cpt++;
                            }

                            if (functionModulesMasksOptimization[line, col - 1]) continue;
                            for (var k = 0; k < 8; k++)
                            {
                                masksMatrix[line, col - 1, k] = chain[cpt] == 1;
                            }
                            cpt++;
                        }
                    }

                    upp = !upp;

                }
        }
        
        private void DataEncoding()
        {
            var chain = _qrCodeData;
            for (int k = 0; k < chain.Length; k++)
            {
                Console.Write(chain[k]);
                if ((k +1)%8 == 0) Console.Write(" ");
            } 
            var upp = true;
            var cpt = 0;
            var skip = false;
                for (var col = Width - 1-_quietZoneWidth; col >_quietZoneWidth; col -=2*_moduleWidth)
                {
                    if (cpt >= chain.Length-1) break;
                    if (upp)
                    {
                        if (cpt >= chain.Length-1) break;
                        for (var line = Height - 1-_quietZoneWidth; line >= _quietZoneWidth; line-=_moduleWidth)
                        {
                            if (cpt >= chain.Length-1) break;
                            if (col <= 7 * _moduleWidth + _quietZoneWidth && skip == false)
                            {
                                col -= _moduleWidth;
                                skip = true;
                            }
                            
                            if (ImageData[line, col] == null)
                            {

                                if (chain[cpt] == 0)
                                {
                                    for (var i = 0; i < _moduleWidth; i++)
                                    {
                                        for (var j = 0; j < _moduleWidth; j++)
                                        {
                                            ImageData[line-i, col-j] = new Pixel(255, 255, 255);
                                            _notFunctionModules[line-i, col-j] = true;
                                        }
                                    }
                                }

                                if (chain[cpt] == 1)
                                {
                                    for (var i = 0; i < _moduleWidth; i++)
                                    {
                                        for (var j = 0; j < _moduleWidth; j++)
                                        {
                                            ImageData[line-i, col-j] = new Pixel(0, 0, 0);
                                            _notFunctionModules[line-i, col-j] = true;
                                        }
                                    }
                                }
                                
                                cpt++;
                            }

                            if (ImageData[line, col - _moduleWidth] != null) continue;
                            if (chain[cpt] == 0)
                            {
                                for (var i = 0; i < _moduleWidth; i++)
                                {
                                    for (var j = 0; j < _moduleWidth; j++)
                                    {
                                        ImageData[line - i, col - _moduleWidth - j] = new Pixel(255, 255, 255);
                                        _notFunctionModules[line - i, col - _moduleWidth - j] = true;
                                    }
                                }

                            }

                            if (chain[cpt] == 1)
                            {
                                for (var i = 0; i < _moduleWidth; i++)
                                {
                                    for (var j = 0; j < _moduleWidth; j++)
                                    {
                                        ImageData[line - i, col - _moduleWidth - j] = new Pixel(0, 0, 0);
                                        _notFunctionModules[line - i, col - j - _moduleWidth] = true;
                                    }
                                }

                            }

                            cpt++;
                        }
                    }
                    else
                    {
                        if (cpt >= chain.Length-1) break;
                        for (var line = _quietZoneWidth; line <=Height-1-_quietZoneWidth; line+= _moduleWidth)
                        {
                            if (cpt >= chain.Length-1) break;
                            if (col <= 7 * _moduleWidth + _quietZoneWidth && skip == false)
                            {
                                col -= _moduleWidth;
                                skip = true;
                            }
                            if (ImageData[line,col] == null)
                            {
                                if (chain[cpt] == 0)
                                {
                                    for (var i = 0; i < _moduleWidth; i++)
                                    {
                                        for (var j = 0; j < _moduleWidth; j++)
                                        {
                                            ImageData[line+i, col-j] = new Pixel(255, 255, 255);
                                            _notFunctionModules[line+i, col-j] = true;
                                        }
                                    }
                                }
                                if (chain[cpt] == 1)
                                {
                                    for (var i = 0; i < _moduleWidth; i++)
                                    {
                                        for (var j = 0; j < _moduleWidth; j++)
                                        {
                                            ImageData[line+i, col-j] = new Pixel(0, 0, 0);
                                            _notFunctionModules[line+i, col-j] = true;
                                        }
                                    }
                                }
                                cpt++;
                            }

                            if (ImageData[line, col - _moduleWidth] != null) continue;
                            if (chain[cpt] == 0)
                            {
                                for (var i = 0; i < _moduleWidth; i++)
                                {
                                    for (var j = 0; j < _moduleWidth; j++)
                                    {
                                        ImageData[line + i, col - _moduleWidth - j] = new Pixel(255, 255, 255);
                                        _notFunctionModules[line + i, col - _moduleWidth - j] = true;
                                    }
                                }

                            }

                            if (chain[cpt] == 1)
                            {
                                for (var i = 0; i < _moduleWidth; i++)
                                {
                                    for (var j = 0; j < _moduleWidth; j++)
                                    {
                                        ImageData[line + i, col - _moduleWidth - j] = new Pixel(0, 0, 0);
                                        _notFunctionModules[line + i, col - _moduleWidth - j] = true;
                                    }
                                }

                            }

                            cpt++;
                        }
                    }

                    upp = !upp;

                }
                Console.WriteLine(" ");
        }
        
        
        public static void InitializeAlphaNumericTable()
        {
            var tableData = Functions.ReadFile("../../../QRCode/alphanumericTable.txt");
            foreach (var item in tableData)
            {
                var args = item.Split(';');
                _alphanumericTable.Add(Convert.ToChar(args[0]), Convert.ToInt32(args[1]));
            }
        }
        private static void AddMaskTemp(Pixel[,] im, int quietZoneWidth, int moduleWidth, int mask)
        {
            //Iterate through each pixel of the ImageData matrix
            for (int line = 0 + quietZoneWidth; line < im.GetLength(0); line+=moduleWidth)
            {
                for (int col = 0 + quietZoneWidth; col < im.GetLength(1); col+=moduleWidth)
                {
                    int fLine = (line - quietZoneWidth)/moduleWidth;
                    int fCol = (col - quietZoneWidth)/moduleWidth;
                    if (im[fLine, fCol] != null) ;
                    {
                        for (var i = line; i < line + moduleWidth; i++)
                        {
                            for (var j = col; j < col + moduleWidth; j++)
                            {
                                im[i, j] = mask switch
                                {
                                    0 => (fLine + fCol) % 2 == 0
                                        ? Functions.InvertPixel(im[i, j])
                                        : im[i, j],
                                    1 => (fLine) % 2 == 0
                                        ? Functions.InvertPixel(im[i, j])
                                        : im[i, j],
                                    2 => (fCol) % 3 == 0
                                        ? Functions.InvertPixel(im[i, j])
                                        : im[i, j],
                                    3 => ((fLine + fCol) % 3 == 0)
                                        ? Functions.InvertPixel(im[i, j])
                                        : im[i, j],
                                    4 =>((fLine / 2 + fCol / 3) % 2 == 0)
                                        ? Functions.InvertPixel(im[i, j])
                                        : im[i, j],
                                    5 => ((fLine * fCol) % 2 + (fLine * fCol) % 3 == 0)
                                        ? Functions.InvertPixel(im[i, j])
                                        : im[i, j],
                                    6 => (((fLine * fCol) % 2 + (fLine * fCol) % 3) % 2 == 0)
                                        ? Functions.InvertPixel(im[i, j])
                                        : im[i, j],
                                    7 => (((fLine * fCol) % 3 + (fLine + fCol) % 2) % 2 == 0)
                                        ? Functions.InvertPixel(im[i, j])
                                        : im[i, j],
                                    _ => throw new ArgumentException("Mask pattern not found")
                                };
                            }
                        }
                    }
                }
            }


        }
        public static string ReadQrCode(Image im)
        {
            var quietZoneWidth = 0;
            while (im.ImageData[quietZoneWidth, quietZoneWidth] != new Pixel(0, 0, 0))
            {
                quietZoneWidth++;
            }
            var QrWidth = im.Width - (2 * quietZoneWidth);
            var QrHeight = im.Height - (2 * quietZoneWidth);
            var moduleWidth = 0;
            while (im.ImageData[moduleWidth, moduleWidth] != new Pixel(255, 255, 255))
            {
                moduleWidth++;
            }
            var version = (QrWidth / (4 * moduleWidth)) - (17 / 4);
            var QrRead = new QRCode(version, quietZoneWidth, moduleWidth);
            var format = ExtractFormatInfo(im, QrRead);
            var correctionLevel = format[0];
            var mask = Convert.ToInt32(format[1]);
            QrRead._maskPattern = Convert.ToInt32(format[1]);
            var Data = new Pixel[QrRead.Height, QrRead.Width];
            for (var x = 0; x < QrRead.Width; x++)
            {
                for (var y = 0; y < QrRead.Width; y++)
                {
                    if (QrRead.ImageData[x, y] != im.ImageData[x + quietZoneWidth, y + quietZoneWidth])
                    {
                        Data[x, y] = im.ImageData[x + quietZoneWidth, y + quietZoneWidth];
                    }
                }
            }
            AddMaskTemp(Data, quietZoneWidth,moduleWidth,mask);
            var data = DataDecoding(Data, quietZoneWidth, moduleWidth);
            var decoData = DecodeStringData(data, correctionLevel, version);
            return null;

        }

        public static char[] ExtractFormatInfo(Image im, QRCode QrRead)
        {
            var fixedLine = 8 * QrRead._moduleWidth + QrRead._quietZoneWidth;
            var movingCol = 0 + QrRead._quietZoneWidth;
            int[] formatData = new int[15];
            for (var i = 0; i < formatData.Length; i++)
            {
                formatData[i] =
                im.ImageData[fixedLine, movingCol + i] == new Pixel(255, 255, 255)
                    ?  0
                    :  1;
                
                if (movingCol+i == 7 * QrRead._moduleWidth + QrRead._quietZoneWidth)
                {
                    movingCol = im.ImageData.GetLength(1) - QrRead._quietZoneWidth - 8 * QrRead._moduleWidth;
                    
                }
            }

            var ErMask = new char[2];
            var data = Convert.ToString(formatData);
            var lines = Functions.ReadFile("../../../QRCode/qrFormat.txt").ToArray();
            for (var i = 0; i < lines.Length; i++)
            {
                if (data == lines[i])
                {
                    ErMask[0] = lines[i][0];
                    ErMask[1] = lines[i][1];
                }
            }

            return ErMask;

        }

        public static int[] DecodeStringData(List<int> data, char correctionLevel, int version)
        {
            var lines = Functions.ReadFile("../../../QRCode/qrCodeDataLength.txt").ToArray();
            var startIndex = correctionLevel switch
            {
                'L' => 3,
                'M' => 2,
                'Q' => 1,
                'H' => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(_correctionLevel))
            };
            
            
            var infos = lines[version*4-1-startIndex].Split(";");
            var result = new int[infos.Length-2];
            for (var i = 1; i < infos.Length-1; i++)
            {
                result[i - 1] = Convert.ToInt32(infos[i]);
            }
            
            var numberDataCodewords = result[0];
            var numberEcCodewords = result[1] * result[2] + result[1] * result[4];
        
            var numberBlocksGroup1 = result[2];
            var numberBlocksGroup2 = result[4];
        
            var numberEcPerBlock = result[1];
            
            var numberDataPerBlockGrp1 = result[3];
            var numberDataPerBlockGrp2= result[5];
            
            var numberDataPerBlockArray = new[] {numberDataPerBlockGrp1, numberDataPerBlockGrp2};
            
            var maxNumberDataPerBlock = Math.Max(numberDataPerBlockGrp1, numberDataPerBlockGrp2);

            data = Functions.ConvertBitArrayToByteArray(data.ToArray()).ToList();
            
            var decodedDataMatrix = new int[numberBlocksGroup1 + numberBlocksGroup2, maxNumberDataPerBlock];

            var dataIndex = 0;
            for (var j = 0; j < decodedDataMatrix.GetLength(1); j++)
            {
                for (var i = 0; i < decodedDataMatrix.GetLength(0); i++)
                {
                    decodedDataMatrix[i, j] = data[dataIndex];
                    var currentNumberDataPerBlock = numberDataPerBlockArray[j / numberBlocksGroup1];
                    if (i >= currentNumberDataPerBlock)
                    {
                        decodedDataMatrix[i, j] = -1;
                        continue;
                    }
                    dataIndex++;
                }
            }


                /*
                for ( var j = 0; j < maxNumberDataPerBlock*8; j+=8)
                {
                    for (var i = 0;i<decodedData.GetLength(0); i++)
                    {
                        
                        for (var k = 0; k < 8; k++)
                        {
                            if (i* (maxNumberDataPerBlock * 8)+j + k >= numberDataCodewords*8) break;
                            decodedData[i, j+k] = data[i* (maxNumberDataPerBlock * 8)+j + k];
                        }
                    }
                }*/

            var message = new List<int>();
            for (var x = 0; x < decodedDataMatrix.GetLength(0); x++)
            {
                for (var y = 0; y < decodedDataMatrix.GetLength(1); y++)
                {
                    message.Add(decodedDataMatrix[x,y]);
                }
            }

            return message.ToArray();

        }
        public static List<int> DataDecoding(Pixel[,] data, int quietZoneWidth, int moduleWidth)
        {
            var upp = true;
            var chain = new List<int>();
            var skip = false;
                for (var col = data.GetLength(1) - 1-quietZoneWidth; col >quietZoneWidth; col -=2*moduleWidth)
                {
                    if (upp)
                    {
                        for (var line = data.GetLength(0) - 1-quietZoneWidth; line >= quietZoneWidth; line-=moduleWidth)
                        {
                            if (col <= 7 * moduleWidth + quietZoneWidth && skip == false)
                            {
                                col -= moduleWidth;
                                skip = true;
                            }

                            if (data[line, col] != null)
                            {
                                if (data[line, col] == new Pixel(255, 255, 255))
                                {
                                    chain.Add(0);
                                }
                                if (data[line, col] == new Pixel(0, 0, 0))
                                {
                                    chain.Add(1);
                                }
                            }

                            if (data[line, col-moduleWidth] != null)
                            {
                                if (data[line, col-moduleWidth] == new Pixel(255, 255, 255))
                                {
                                    chain.Add(0);
                                }
                                if (data[line, col-moduleWidth] == new Pixel(0, 0, 0))
                                {
                                    chain.Add(1);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (var line = quietZoneWidth; line <=data.GetLength(0)-1-quietZoneWidth; line+= moduleWidth)
                        {
                            if (col <= 7 * moduleWidth + quietZoneWidth && skip == false)
                            {
                                col -= moduleWidth;
                                skip = true;
                            }
                            if (data[line, col] != null)
                            {

                                if (data[line, col] == new Pixel(255, 255, 255))
                                {
                                    chain.Add(0);
                                }
                                if (data[line, col] == new Pixel(0, 0, 0))
                                {
                                    chain.Add(1);
                                }
                            }
                            if (data[line, col-moduleWidth] != null)
                            {
                                if (data[line, col-moduleWidth] == new Pixel(255, 255, 255))
                                {
                                    chain.Add(0);
                                }
                                if (data[line, col-moduleWidth] == new Pixel(0, 0, 0))
                                {
                                    chain.Add(1);
                                }
                            }
                        }
                    }
                    upp = !upp;
                }
                return chain;
        }
    }
}