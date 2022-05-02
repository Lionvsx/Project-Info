using System;
using System.Collections.Generic;
using System.Linq;
using Project_Info.Console_Display;
using STH1123.ReedSolomon;

namespace Project_Info.QRCode
{
    public class QRCode : Image
    {
        private int _correctionLevel;
        private int[] _mode;
        private bool[,,] _masksMatrix;
        private List<int> _wordEncodedData;


        public QRCode(int version, int quietZoneWidth, int moduleWidth, int maskPattern, int correctionLevel, string message)
        {
            var borderSize = (8 * 2 + (4 * version + 1)) * moduleWidth + 2 * quietZoneWidth;
            ImageData = new Pixel[borderSize, borderSize];
            NotFunctionModules = new bool[borderSize, borderSize];
            Version = version;
            QuietZoneWidth = quietZoneWidth;
            ModuleWidth = moduleWidth;
            
            Height = borderSize;
            Width = borderSize;
            Offset = 54;
            BitRgb = 24;
            
            MaskPattern = maskPattern;
            _correctionLevel = correctionLevel;

            CreateEmptyQrCode();
            EncodeStringData(message);
            GetErrorData();
            DataEncoding();
            InitFunctionModulesMatrix();
            
            
            Functions.FillImageWhite(ImageData);
            AddMask();
        }

        public QRCode(string message, int correctionLevel =  1)
        {
            _correctionLevel = correctionLevel;
            QuietZoneWidth = 15;
            ModuleWidth = 10;
            
            GetVersionFromString(message);
            
            var borderSize = (8 * 2 + (4 * Version + 1)) * ModuleWidth + 2 * QuietZoneWidth;
            ImageData = new Pixel[borderSize, borderSize];
            NotFunctionModules = new bool[borderSize, borderSize];
            Height = borderSize;
            Width = borderSize;
            Offset = 54;
            BitRgb = 24;

            CreateBestMaskQRCode(message);
            
            AddFormatInformation();

            DataEncoding();
            Functions.FillImageWhite(ImageData);
            AddMask();
        }

        private QRCode(int version, int quietZoneWidth, int moduleWidth)
        {
            Version = version;
            QuietZoneWidth = quietZoneWidth;
            ModuleWidth = moduleWidth;
            
            var borderSize = (8 * 2 + (4 * Version + 1)) * ModuleWidth + 2 * QuietZoneWidth;
            ImageData = new Pixel[borderSize, borderSize];
            NotFunctionModules = new bool[borderSize, borderSize];
            Height = borderSize;
            Width = borderSize;
            Offset = 54;
            BitRgb = 24;
            
            CreateEmptyQrCode();
        }

        protected QRCode()
        {
        }

        public bool[,] IsFunctionModule { get; set; }

        public int MaskPattern { get; set; }

        public int CorrectionLevel
        {
            get => _correctionLevel;
            set => _correctionLevel = value;
        }

        public List<int> WordEncodedData
        {
            get => _wordEncodedData;
            set => _wordEncodedData = value;
        }

        public int Version { get; set; }

        public int QuietZoneWidth { get; set; }

        public int ModuleWidth { get; set; }

        public static Dictionary<char, int> AlphanumericTable { get; set; } = new();

        public bool[,] NotFunctionModules { get; set; }

        public int NumberDataCodewords { get; set; }

        public int NumberEcCodewords { get; set; }

        public int[] QRCodeData { get; set; }

        public int NumberBlocksGroup1 { get; set; }

        public int NumberBlocksGroup2 { get; set; }

        public int NumberEcPerBlock { get; set; }

        public int NumberDataPerBlockGrp2 { get; set; }

        public int NumberDataPerBlockGrp1 { get; set; }


        private void GetVersionFromString(string message)
        {
            var requiredBits = 17 + (message.Length % 2) * 6 + (message.Length / 2) * 11;
            var requiredBytes = requiredBits % 8 == 0 ? requiredBits / 8 : requiredBits / 8 + 1;
            GetCodeDataLength(requiredBytes);
        }

        private void InitFindersAndSeparators()
        {
            CreateFinderPatterns(0+QuietZoneWidth, 0+QuietZoneWidth);
            CreateSeparators(0 + QuietZoneWidth, 0 + QuietZoneWidth);
            CreateFinderPatterns(Height - QuietZoneWidth - 7 * ModuleWidth, 0 + QuietZoneWidth);
            CreateSeparators(Height - QuietZoneWidth - 8 * ModuleWidth, 0 + QuietZoneWidth);
            CreateFinderPatterns(0+QuietZoneWidth, 0 + Width - QuietZoneWidth - 7 * ModuleWidth);
            CreateSeparators(0+QuietZoneWidth, 0 + Width - QuietZoneWidth - 8 * ModuleWidth);
        }
        
        private void InitMasksFinderAndSeparators()
        {
            var borderSize = _masksMatrix.GetLength(0);
            CreateMaskFinderPatterns(0, 0, _masksMatrix);
            CreateMasksSeparators(0, 0);
            CreateMaskFinderPatterns(borderSize - 7, 0, _masksMatrix);
            CreateMasksSeparators(borderSize - 8, 0);
            CreateMaskFinderPatterns(0, borderSize - 7, _masksMatrix);
            CreateMasksSeparators(0, borderSize - 8);
        }
        private void CreateBestMaskQRCode(string message)
        {
            InitFindersAndSeparators();
            
            var borderSize = 8 * 2 + 4 * Version + 1;
            _masksMatrix = new bool[borderSize, borderSize, 8];
            IsFunctionModule = new bool[borderSize, borderSize];

            InitMasksFinderAndSeparators();
            
            CreateAlignmentPatternsWithMasks(_masksMatrix);

            CreateTimingPatterns();
            CreateMasksTimingPatterns(_masksMatrix, borderSize);
            
            AddDarkModule();
            AddMasksDarkModule(_masksMatrix);
            
            AddVersionInformationWithMasks(_masksMatrix);

            EncodeStringData(message);
            AddErrorData();
            
            AddMasksFormatInformation(_masksMatrix);
            MaskDataEncoding(_masksMatrix);
            ApplyAllMasksMatrix(_masksMatrix);
            
            var resultVector = QRMaskFunctions.PerformMaskEvaluations(_masksMatrix);
            MaskPattern = Array.IndexOf(resultVector, resultVector.Min());
        }



        private void CreateEmptyQrCode()
        {
            InitFindersAndSeparators();
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
            var spacing = (ModuleWidth - 1);
            for (int i = line; i < 7 * ModuleWidth + line; i++)
            {
                for (int j = col; j < 7* ModuleWidth + col; j++)
                {
                    if (j < col + ModuleWidth || i < line + ModuleWidth) ImageData[i, j] = new Pixel(0, 0, 0);
                    else if (j > col + 6 * ModuleWidth - 1 ||
                             i > line + 6 * ModuleWidth - 1) ImageData[i, j] = new Pixel(0, 0, 0);

                    else if (j >= col + 2 * ModuleWidth &&
                        i >= line + 2 * ModuleWidth &&
                        j <= col + 6 * ModuleWidth - 2 * ModuleWidth + spacing &&
                        i <= line + 6 * ModuleWidth - 2 * ModuleWidth + spacing) ImageData[i, j] = new Pixel(0, 0, 0);
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
                    IsFunctionModule[i, j] = true;
                }
            }
        }

        private void CreateSeparators(int line, int col)
        {
            for (int i = line; i < 8 * ModuleWidth + line; i++)
            {
                for (int j = col; j < 8 * ModuleWidth + col; j++)
                {
                    ImageData[i, j] ??= new Pixel(255, 255, 255);
                }
            }
        }
        
        

        private void CreateTimingPatterns()
        {
            for (int line = 7 * ModuleWidth + QuietZoneWidth; line <= Height - 7 * ModuleWidth - QuietZoneWidth; line++)
            {
                for (int col = 6 * ModuleWidth + QuietZoneWidth; col < ModuleWidth * 7 + QuietZoneWidth; col++) 
                {
                    if ((line - QuietZoneWidth) / ModuleWidth % 2 == 0) ImageData[line, col] = new Pixel(0, 0, 0);
                    else ImageData[line, col] = new Pixel(255, 255, 255);
                }
            }

            for (int col = 7 * ModuleWidth + QuietZoneWidth; col <= Height - 7 * ModuleWidth - QuietZoneWidth; col++) 
            {
                for (int line = 6 * ModuleWidth + QuietZoneWidth; line < ModuleWidth * 7 + QuietZoneWidth; line++) 
                {
                    if ((col - QuietZoneWidth) / ModuleWidth % 2 == 0) ImageData[line, col] = new Pixel(0, 0, 0);
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
                        IsFunctionModule[line, 6] = true;
                        continue;
                    }
                    if (line % 2 == 0) masksMatrix[line, 6, k] = true;
                }
            
                for (var col = 7; col <= borderSize - 7; col++)
                {
                    if (k == 8)
                    {
                        IsFunctionModule[6, col] = true;
                        continue;
                    }
                    if (col % 2 == 0) masksMatrix[6, col, k] = true;
                }
            }
        }

        private void AddDarkModule()
        {
            var startingLine = (4 * Version + 9)*ModuleWidth +QuietZoneWidth;
            for (int col = 8*ModuleWidth + QuietZoneWidth; col < ModuleWidth * 9 + QuietZoneWidth; col++)
            {
                for (int line = startingLine; line < ModuleWidth + startingLine; line++)
                {
                    ImageData[line, col] = new Pixel(0, 0, 0);
                }
            }
        }
        
        private void AddMasksDarkModule(bool[,,] masksMatrix)
        {
            var startingLine = (4 * Version + 9);
            IsFunctionModule[startingLine, 8] = true;
            for (var k = 0; k < 8; k++)
            {
                masksMatrix[startingLine, 8, k] = true;
            }
        }

        public void AddFormatInformation()
        {
            var formatBinary = GetFormatInfo();
            var fixedLine = 8 * ModuleWidth + QuietZoneWidth;
            var fixedCol = 8 * ModuleWidth + QuietZoneWidth;

            var movingCol = 0 + QuietZoneWidth;
            var movingLine = ImageData.GetLength(0) -1 - QuietZoneWidth - (ModuleWidth - 1);
            
            foreach (var bit in formatBinary)
            {
                if (ImageData[movingLine, fixedCol] != null) movingLine -= 1 * ModuleWidth;
                if (ImageData[fixedLine, movingCol] != null) movingCol += 1 * ModuleWidth;
                for (var l = 0; l < ModuleWidth; l++)
                {
                    for (var c = 0; c < ModuleWidth; c++)
                    {
                        ImageData[fixedLine + l, movingCol + c] =
                            bit == 0 ? new Pixel(255, 255, 255) : new Pixel(0, 0, 0);
                        ImageData[movingLine + l, fixedCol + c] =
                            bit == 0 ? new Pixel(255, 255, 255) : new Pixel(0, 0, 0);
                    }
                }
                if (movingLine - 1 * ModuleWidth == (4 * Version + 9) * ModuleWidth + QuietZoneWidth &&
                    movingCol == 7 * ModuleWidth + QuietZoneWidth)
                {
                    movingLine = 8 * ModuleWidth + QuietZoneWidth;
                    movingCol = ImageData.GetLength(1) - QuietZoneWidth - 8 * ModuleWidth;
                    continue;
                }

                movingLine -= 1 * ModuleWidth;
                movingCol += 1 * ModuleWidth;
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
                        IsFunctionModule[movingLine, fixedCol] = true;
                        IsFunctionModule[fixedLine, movingCol] = true;
                    }
                    
                    if (movingLine - 1 == (4 * Version + 9) &&
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
            if (Version < 7) return;
            var bitArray = GetQRVersion();
            for (int i = 0; i < bitArray.Length; i++)
            {
                var newPixel = bitArray[i] == 1 ? new Pixel(0, 0, 0) : new Pixel(255, 255, 255);
                var line = (2 - i % 3) * ModuleWidth + ImageData.GetLength(0) - 11 * ModuleWidth - QuietZoneWidth;
                var col = (5 - i / 3) * ModuleWidth + QuietZoneWidth;
                for (var l = line; l < ModuleWidth + line; l++)
                {
                    for (var c = col; c < ModuleWidth + col; c++)
                    {
                        ImageData[l, c] = newPixel;
                        ImageData[c, l] = newPixel;
                    }
                }
            }
        }
        
        private void AddVersionInformationWithMasks(bool[,,] masksMatrix)
        {
            if (Version < 7) return;
            var bitArray = GetQRVersion();
            for (int i = 0; i < bitArray.Length; i++)
            {
                var newPixel = bitArray[i] == 1 ? new Pixel(0, 0, 0) : new Pixel(255, 255, 255);
                var line = (2 - i % 3) * ModuleWidth + ImageData.GetLength(0) - 11 * ModuleWidth - QuietZoneWidth;
                var col = (5 - i / 3) * ModuleWidth + QuietZoneWidth;

                var maskLine = (2 - i % 3) + masksMatrix.GetLength(0) - 11;
                var maskCol = (5 - i / 3);
                for (var l = line; l < ModuleWidth + line; l++)
                {
                    for (var c = col; c < ModuleWidth + col; c++)
                    {
                        ImageData[l, c] = newPixel;
                        ImageData[c, l] = newPixel;
                    }
                }
                for (int k = 0; k < 8; k++)
                {
                    if (k == 0)
                    {
                        IsFunctionModule[maskLine, maskCol] = true;
                        IsFunctionModule[maskCol, maskLine] = true;
                    }
                    masksMatrix[maskLine, maskCol, k] = bitArray[i] == 1;
                    masksMatrix[maskCol, maskLine, k] = bitArray[i] == 1;
                }
            }
        }

        private void CreateAlignmentPatterns()
        {
            var spacing = (ModuleWidth - 1);
            if (Version < 2) return;
            var coordinates = new List<int>(GetQRAlignmentCoordinates());
            
            var arrayOfCoordinates = Functions.DoubleIntCombinations(coordinates);
            foreach (var item in arrayOfCoordinates)
            {
                CreateAlignmentPattern(item[0]*ModuleWidth + QuietZoneWidth, item[1]*ModuleWidth + QuietZoneWidth);
                CreateAlignmentPattern(item[1]*ModuleWidth + QuietZoneWidth, item[0]*ModuleWidth + QuietZoneWidth);
                CreateAlignmentPattern(item[0]*ModuleWidth + QuietZoneWidth, item[0]*ModuleWidth + QuietZoneWidth);
                CreateAlignmentPattern(item[1]*ModuleWidth + QuietZoneWidth, item[1]*ModuleWidth + QuietZoneWidth);
            }
        }
        
        private void CreateAlignmentPatternsWithMasks(bool[,,] masksMatrix)
        {
            if (Version < 2) return;
            var coordinates = new List<int>(GetQRAlignmentCoordinates());
            
            var arrayOfCoordinates = Functions.DoubleIntCombinations(coordinates);
            foreach (var item in arrayOfCoordinates)
            {
                CreateAlignmentPattern(item[0]*ModuleWidth + QuietZoneWidth, item[1]*ModuleWidth + QuietZoneWidth);
                CreateAlignmentPattern(item[1]*ModuleWidth + QuietZoneWidth, item[0]*ModuleWidth + QuietZoneWidth);
                CreateAlignmentPattern(item[0]*ModuleWidth + QuietZoneWidth, item[0]*ModuleWidth + QuietZoneWidth);
                CreateAlignmentPattern(item[1]*ModuleWidth + QuietZoneWidth, item[1]*ModuleWidth + QuietZoneWidth);
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
                            IsFunctionModule[i, j] = true;
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
            var spacing = (ModuleWidth - 1);
            if (ImageData[line, col] != null) return;
            for (int i = line - 2 * ModuleWidth; i <= line + 2 * ModuleWidth + spacing; i++)
            {
                for (int j = col - 2 * ModuleWidth; j <= col + 2 * ModuleWidth + spacing; j++)
                {
                    var fakeLine = (i - (line - 2 * ModuleWidth)) / ModuleWidth;
                    var fakeCol = (j - (col - 2 * ModuleWidth)) / ModuleWidth;

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
            var lines = Functions.ReadFile("../../../QRCode/qrSettings.txt").ToArray();
            var selectedLine = lines[Version-2];
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
            if (Version < 7) return Array.Empty<int>();
            var selectedLine = lines[Version-7];
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

            var selectedLine = lines[(Version - 1) * 4 + lineIndex];
            
            var infos = selectedLine.Split(";");
            var result = new int[infos.Length-2];
            for (var i = 1; i < infos.Length-1; i++)
            {
                result[i - 1] = Convert.ToInt32(infos[i]);
            }

            NumberDataCodewords = result[0];
            NumberEcCodewords = result[1] * result[2] + result[1] * result[4];
            
            NumberBlocksGroup1 = result[2];
            NumberBlocksGroup2 = result[4];
            
            NumberEcPerBlock = result[1];
            NumberDataPerBlockGrp1 = result[3];
            NumberDataPerBlockGrp2 = result[5];

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
                
                Version = Convert.ToInt32(infos[0].Split("-")[0]);
                
                var result = new int[infos.Length-2];
                for (var i = 1; i < infos.Length-1; i++)
                {
                    result[i - 1] = Convert.ToInt32(infos[i]);
                }
                
                NumberDataCodewords = result[0];
                NumberEcCodewords = result[1] * result[2] + result[1] * result[4];
            
                NumberBlocksGroup1 = result[2];
                NumberBlocksGroup2 = result[4];
            
                NumberEcPerBlock = result[1];
                NumberDataPerBlockGrp1 = result[3];
                NumberDataPerBlockGrp2 = result[5];
                

                break;
            }
        }


        private void AddErrorData()
        {
            var field = new GenericGF(285, 256, 0);
            var rse = new ReedSolomonEncoder(field);
            var maxNumberDataPerBlock = Math.Max(NumberDataPerBlockGrp1, NumberDataPerBlockGrp2);
            
            var encodedData = new int[NumberBlocksGroup1 + NumberBlocksGroup2, maxNumberDataPerBlock];
            var errorEncodedData = new int[NumberBlocksGroup1 + NumberBlocksGroup2, NumberEcPerBlock];

            // Error Data Group 1
            for (int i = 0; i < NumberBlocksGroup1; i++)
            {
                var errorFields = NumberEcPerBlock;
                
                var dataBlock = ByteEncodedData[(i * NumberDataPerBlockGrp1)..((i + 1) * NumberDataPerBlockGrp1)];
                var zerosArray = Enumerable.Repeat(0, errorFields);

                var byteArray = dataBlock.Concat(zerosArray).ToArray();
                rse.Encode(byteArray, errorFields);
                
                for (var j = 0; j < maxNumberDataPerBlock; j++)
                {
                    encodedData[i, j] = j >= NumberDataPerBlockGrp1 ? -1 : byteArray[j];
                }
                for (var j = 0; j < NumberEcPerBlock; j++)
                {
                    errorEncodedData[i, j] = byteArray[j + NumberDataPerBlockGrp1];
                }
            }
            
            //Error data group 2
            for (int i = 0; i < NumberBlocksGroup2; i++)
            {
                var errorFields = NumberEcPerBlock;
                
                var dataBlock = ByteEncodedData[(i * NumberDataPerBlockGrp2 + NumberBlocksGroup1 * NumberDataPerBlockGrp1)..((i + 1) * NumberDataPerBlockGrp2 + NumberBlocksGroup1 * NumberDataPerBlockGrp1)];
                var zerosArray = Enumerable.Repeat(0, errorFields);

                var byteArray = dataBlock.Concat(zerosArray).ToArray();
                rse.Encode(byteArray, errorFields);
                
                for (var j = 0; j < maxNumberDataPerBlock; j++)
                {
                    encodedData[i + NumberBlocksGroup1, j] = j >= NumberDataPerBlockGrp2 ? -1 : byteArray[j];
                }
                for (var j = 0; j < NumberEcPerBlock; j++)
                {
                    errorEncodedData[i + NumberBlocksGroup1, j] = byteArray[j + NumberDataPerBlockGrp2];
                }
            }
            
            var qrFinalData = new List<int>();
            
            for (var i = 0; i < maxNumberDataPerBlock; i++)
            {
                for (var j = 0; j < NumberBlocksGroup1 + NumberBlocksGroup2; j++)
                {
                    if (encodedData[j, i] == -1) continue;
                    qrFinalData.Add(encodedData[j, i]);
                }
            }
            
            for (var i = 0; i < NumberEcPerBlock; i++)
            {
                for (var j = 0; j < NumberBlocksGroup1 + NumberBlocksGroup2; j++)
                {
                    qrFinalData.Add(errorEncodedData[j, i]);
                }
            }


            var lines = Functions.ReadFile("../../../QRCode/qrRemainderBits.txt").ToArray();
            var selectedLine = lines[Version - 1];
            var coordinates = selectedLine.Split(";");
            var remainder = Convert.ToInt32(coordinates[1]);
            
            var bitArray = Functions.ConvertByteArrayToBitArray(qrFinalData.ToArray()).ToList();
            for (var i = 0; i < remainder; i++)
            {
                bitArray.Add(0);
            }
            QRCodeData = bitArray.ToArray();
        }
        
        private void GetErrorData()
        {
            var field = new GenericGF(285, 256, 0);
            var rse = new ReedSolomonEncoder(field);
            //Max byte value = 255 (OxFF)
            
            var errorFields = NumberEcCodewords;
            
            var zerosArray = Enumerable.Repeat((int) 0, errorFields);
            var byteArray = ByteEncodedData.Concat(zerosArray).ToArray();
            var intByteArray = Array.ConvertAll(byteArray, x => (int) x);
            rse.Encode(intByteArray, errorFields);
            
            // Add remainder bits if needed
            var lines = Functions.ReadFile("../../../QRCode/qrRemainderBits.txt").ToArray();
            var selectedLine = lines[Version - 1];
            var coordinates = selectedLine.Split(";");
            var remainder = Convert.ToInt32(coordinates[1]);
            
            var bitArray = Functions.ConvertByteArrayToBitArray(intByteArray).ToList();
            for (var i = 0; i < remainder; i++)
            {
                bitArray.Add(0);
            }

            QRCodeData = bitArray.ToArray();
        }


        private int[] GetFormatInfoOld()
        {
            var maskBinary = Functions.ConvertIntToBinaryArray(MaskPattern);
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

            var selectedLine = lines[lineIndex + MaskPattern];
            
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
                Version < 10 ? 9 : Version < 27 ? 11 : 13);
            result.AddRange(wordLengthBits);

            for (var i = 0; i < word.Length; i+=2)
            {
                if (i % 2 == 0 && i != word.Length - 1)
                {
                    AlphanumericTable.TryGetValue(word[i], out var highValue);
                    AlphanumericTable.TryGetValue(word[i + 1], out var lowValue);

                    result.AddRange(Functions.IntToDesiredLengthBit(highValue * 45 + lowValue, 11));
                }
                if (i == wordLength - 1 && wordLength % 2 == 1)
                {
                    AlphanumericTable.TryGetValue(word[i], out var value);
                    result.AddRange(Functions.IntToDesiredLengthBit(value, 6));
                }
            }
            
            
            //ADD Terminator
            var count = 0;
            while (result.Count < NumberDataCodewords * 8 && count < 4)
            {
                result.Add(0);
                count++;
            }
            //Add more 0 to make multiple of 8
            result = result.Count % 8 != 0
                ? Functions.Pad(result.ToArray(), result.Count + (8 - result.Count % 8)).ToList()
                : result;
            //Pad bytes if string still too short
            if (result.Count < NumberDataCodewords * 8)
            {
                var byte1 = Functions.IntToDesiredLengthBit(236, 8);
                var byte2 = Functions.IntToDesiredLengthBit(17, 8);

                var iterations = (NumberDataCodewords * 8 - result.Count);
                for (int i = 0; i < iterations/8; i++)
                {
                    result.AddRange(i % 2 == 0 ? byte1 : byte2);
                }
            }
            

            _wordEncodedData = result;
        }

        public void AddMask()
        {
            //Iterate through each pixel of the ImageData matrix
            for (int line = 0 + QuietZoneWidth; line < ImageData.GetLength(0) - QuietZoneWidth; line+=ModuleWidth)
            {
                for (int col = 0 + QuietZoneWidth; col < ImageData.GetLength(1) - QuietZoneWidth; col+=ModuleWidth)
                {
                    int fLine = (line - QuietZoneWidth)/ModuleWidth;
                    int fCol = (col - QuietZoneWidth)/ModuleWidth;

                    if (IsFunctionModule[(line - QuietZoneWidth) / ModuleWidth, (col - QuietZoneWidth) / ModuleWidth]) continue;
                    for (var i = line; i < line + ModuleWidth; i++)
                    {
                        for (var j = col; j < col + ModuleWidth; j++)
                        {
                            ImageData[i, j] = MaskPattern switch
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
        
        private void ApplyAllMasksMatrix(bool[,,] masksMatrix)
        {
            for (int line = 0; line < masksMatrix.GetLength(0); line += 1) 
            {
                for (int col = 0; col < masksMatrix.GetLength(1); col += 1) 
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if (IsFunctionModule[line, col]) continue;
                        
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
            var chain = QRCodeData;
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
                            
                            if (!IsFunctionModule[line, col]) 
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
                            
                            if (IsFunctionModule[line, col - 1]) continue;
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
                            
                            if (!IsFunctionModule[line, col]) 
                            {
                                for (var k = 0; k < 8; k++)
                                {
                                    masksMatrix[line, col, k] = chain[cpt] == 1;
                                }
                                cpt++;
                            }

                            if (IsFunctionModule[line, col - 1]) continue;
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
            var chain = QRCodeData;
            var upp = true;
            var cpt = 0;
            var skip = false;
                for (var col = Width - 1-QuietZoneWidth; col >QuietZoneWidth; col -=2*ModuleWidth)
                {
                    if (cpt >= chain.Length-1) break;
                    if (upp)
                    {
                        if (cpt >= chain.Length-1) break;
                        for (var line = Height - 1-QuietZoneWidth; line >= QuietZoneWidth; line-=ModuleWidth)
                        {
                            if (cpt >= chain.Length-1) break;
                            if (col <= 7 * ModuleWidth + QuietZoneWidth && skip == false)
                            {
                                col -= ModuleWidth;
                                skip = true;
                            }
                            
                            if (ImageData[line, col] == null)
                            {

                                if (chain[cpt] == 0)
                                {
                                    for (var i = 0; i < ModuleWidth; i++)
                                    {
                                        for (var j = 0; j < ModuleWidth; j++)
                                        {
                                            ImageData[line-i, col-j] = new Pixel(255, 255, 255);
                                            NotFunctionModules[line-i, col-j] = true;
                                        }
                                    }
                                }

                                if (chain[cpt] == 1)
                                {
                                    for (var i = 0; i < ModuleWidth; i++)
                                    {
                                        for (var j = 0; j < ModuleWidth; j++)
                                        {
                                            ImageData[line-i, col-j] = new Pixel(0, 0, 0);
                                            NotFunctionModules[line-i, col-j] = true;
                                        }
                                    }
                                }
                                
                                cpt++;
                            }

                            if (ImageData[line, col - ModuleWidth] != null) continue;
                            if (chain[cpt] == 0)
                            {
                                for (var i = 0; i < ModuleWidth; i++)
                                {
                                    for (var j = 0; j < ModuleWidth; j++)
                                    {
                                        ImageData[line - i, col - ModuleWidth - j] = new Pixel(255, 255, 255);
                                        NotFunctionModules[line - i, col - ModuleWidth - j] = true;
                                    }
                                }

                            }

                            if (chain[cpt] == 1)
                            {
                                for (var i = 0; i < ModuleWidth; i++)
                                {
                                    for (var j = 0; j < ModuleWidth; j++)
                                    {
                                        ImageData[line - i, col - ModuleWidth - j] = new Pixel(0, 0, 0);
                                        NotFunctionModules[line - i, col - j - ModuleWidth] = true;
                                    }
                                }

                            }

                            cpt++;
                        }
                    }
                    else
                    {
                        if (cpt >= chain.Length-1) break;
                        for (var line = QuietZoneWidth; line <=Height-1-QuietZoneWidth; line+= ModuleWidth)
                        {
                            if (cpt >= chain.Length-1) break;
                            if (col <= 7 * ModuleWidth + QuietZoneWidth && skip == false)
                            {
                                col -= ModuleWidth;
                                skip = true;
                            }
                            if (ImageData[line,col] == null)
                            {
                                if (chain[cpt] == 0)
                                {
                                    for (var i = 0; i < ModuleWidth; i++)
                                    {
                                        for (var j = 0; j < ModuleWidth; j++)
                                        {
                                            ImageData[line+i, col-j] = new Pixel(255, 255, 255);
                                            NotFunctionModules[line+i, col-j] = true;
                                        }
                                    }
                                }
                                if (chain[cpt] == 1)
                                {
                                    for (var i = 0; i < ModuleWidth; i++)
                                    {
                                        for (var j = 0; j < ModuleWidth; j++)
                                        {
                                            ImageData[line+i, col-j] = new Pixel(0, 0, 0);
                                            NotFunctionModules[line+i, col-j] = true;
                                        }
                                    }
                                }
                                cpt++;
                            }

                            if (ImageData[line, col - ModuleWidth] != null) continue;
                            if (chain[cpt] == 0)
                            {
                                for (var i = 0; i < ModuleWidth; i++)
                                {
                                    for (var j = 0; j < ModuleWidth; j++)
                                    {
                                        ImageData[line + i, col - ModuleWidth - j] = new Pixel(255, 255, 255);
                                        NotFunctionModules[line + i, col - ModuleWidth - j] = true;
                                    }
                                }

                            }

                            if (chain[cpt] == 1)
                            {
                                for (var i = 0; i < ModuleWidth; i++)
                                {
                                    for (var j = 0; j < ModuleWidth; j++)
                                    {
                                        ImageData[line + i, col - ModuleWidth - j] = new Pixel(0, 0, 0);
                                        NotFunctionModules[line + i, col - ModuleWidth - j] = true;
                                    }
                                }

                            }

                            cpt++;
                        }
                    }

                    upp = !upp;

                }
        }
        
        
        public static void InitializeAlphaNumericTable()
        {
            var tableData = Functions.ReadFile("../../../QRCode/alphanumericTable.txt");
            foreach (var item in tableData)
            {
                var args = item.Split(';');
                AlphanumericTable.Add(Convert.ToChar(args[0]), Convert.ToInt32(args[1]));
            }
        }
        public static string ReadQrCode(Image im)
        {
            var quietZoneWidth = 0;
            if (im.ImageData[quietZoneWidth, quietZoneWidth] != new Pixel(255, 255, 255))
            {
                while (im.ImageData[quietZoneWidth, quietZoneWidth].Red >0)
                {
                    quietZoneWidth++;
                }
            }
    
            var QrWidth = im.Width - (2 * quietZoneWidth);
            var QrHeight = im.Height - (2 * quietZoneWidth);
            var moduleWidth = quietZoneWidth;
            while (im.ImageData[moduleWidth, moduleWidth].Red < 255)
            {
                moduleWidth++;
            }

            moduleWidth -= quietZoneWidth;
            var version = (QrWidth / (4 * moduleWidth)) - (17 / 4);
            
            var QrRead = new QRCode(version, quietZoneWidth, moduleWidth);
            
            var format = ExtractFormatInfo(im, QrRead);
            var correctionLevel = format[0];
            var mask = Convert.ToInt32(format[1]);
            QrRead.MaskPattern = Convert.ToInt32(format[1]);
            var Data = new Pixel[QrWidth, QrWidth];
            for (var x = 0; x < QrWidth; x++)
            {
                for (var y = 0; y < QrWidth; y++)
                {
                    if (QrRead.ImageData[x+quietZoneWidth, y+quietZoneWidth] != im.ImageData[x + quietZoneWidth, y + quietZoneWidth])
                    {
                        Data[x, y] = im.ImageData[x + quietZoneWidth, y + quietZoneWidth];
                    }
                }
            }
            
            var data = DataDecoding(Data, quietZoneWidth, moduleWidth);
            var decoData = DecodeStringData(data, correctionLevel, version);
            var stringData = ExtractString(decoData, version);
            return stringData;

        }

        public static string ExtractString(int[] byteArray, int version)
        {
            
            var mode = new[] {0, 0, 1, 0};
            var desiredLength = version < 10 ? 9 : version < 27 ? 11 : 13;
            var word = new List<string>();
            var bitArray = Functions.ConvertByteArrayToBitArray(byteArray);
            
            var alphanumericTable = Functions.ReadFile("../../../QRCode/alphanumericTable.txt").ToArray();
            
            for (var index = mode.Length + desiredLength; index < bitArray.Length; index+=11)
            {
                int value = 0;

                for (int i = 10; i >=0; i--)
                {
                    if (bitArray[i]==1)
                        value += Convert.ToInt32(Math.Pow(2, i));
                }
                var infosH =alphanumericTable[value/45].Split(";");
                var infosL =alphanumericTable[value%45].Split(";");
                var highValue = infosH[0];
                var lowValue = infosL[0];
                word.Add(highValue);
                word.Add(lowValue);
                
            }
            var messageStr = string.Join("",word.ToArray());
            return messageStr;

        }
        public static string[] ExtractFormatInfo(Image im, QRCode QrRead)
        {
            var fixedLine = 8 * QrRead.ModuleWidth + QrRead.QuietZoneWidth;
            var movingCol = 0 + QrRead.QuietZoneWidth;
            int[] formatData = new int[15];
            var skip1 = false;
            var skip2 = false;
            var temp = 0;
            for (var i = 0; i < formatData.Length; i++)
            {
                if (movingCol+i*QrRead.ModuleWidth == 9 * QrRead.ModuleWidth + QrRead.QuietZoneWidth && skip1 == false)
                {
                    movingCol = im.ImageData.GetLength(1)- 7 * QrRead.ModuleWidth - QrRead.QuietZoneWidth;
                    skip1 = true;
                    temp = i;

                }
                if (movingCol+i*QrRead.ModuleWidth == 6 * QrRead.ModuleWidth + QrRead.QuietZoneWidth && skip2 == false)
                {
                    movingCol += QrRead.ModuleWidth;
                    skip2 = true;
                    

                }

                if (im.ImageData[fixedLine, movingCol + (i-temp)*QrRead.ModuleWidth].Red == 255) formatData[i] = 0;
                else formatData[i] = 1;
                
                
            }

            var ErMask = new string[2];
            var data = string.Join("",formatData);
            var lines = Functions.ReadFile("../../../QRCode/qrFormat.txt").ToArray();
            for (var i = 0; i < lines.Length; i++)
            {
                var infos = lines[i].Split(";");
                if (data == infos[2])
                {
                    ErMask[0] = infos[0];
                    ErMask[1] = infos[1];
                }
            }

            return ErMask;

        }

        public static int[] DecodeStringData(List<int> data, string correctionLevel, int version)
        {
            var lines = Functions.ReadFile("../../../QRCode/qrCodeDataLength.txt").ToArray();
            var startIndex = correctionLevel switch
            {
                "L" => 3,
                "M" => 2,
                "Q" => 1,
                "H" => 0,
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
                    var currentNumberDataPerBlock = numberDataPerBlockArray[j / numberDataPerBlockGrp1];
                    if (i >= currentNumberDataPerBlock)
                    {
                        decodedDataMatrix[i, j] = -1;
                        continue;
                    }
                    dataIndex++;
                }
            }
            
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
                                if (data[line, col].Red == 255)
                                {
                                    chain.Add(0);
                                }
                                if (data[line, col].Red == 0)
                                {
                                    chain.Add(1);
                                }
                            }

                            if (data[line, col-moduleWidth] != null)
                            {
                                if (data[line, col-moduleWidth].Red == 255)
                                {
                                    chain.Add(0);
                                }
                                if (data[line, col-moduleWidth].Red == 0)
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

                                if (data[line, col].Red == 255)
                                {
                                    chain.Add(0);
                                }
                                if (data[line, col].Red == 0)
                                {
                                    chain.Add(1);
                                }
                            }
                            if (data[line, col-moduleWidth] != null)
                            {
                                if (data[line, col-moduleWidth].Red == 255)
                                {
                                    chain.Add(0);
                                }
                                if (data[line, col-moduleWidth].Red == 0)
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
        
        public void InitFunctionModulesMatrix()
        {
            IsFunctionModule = new bool[Width, Height];
            CreateFinderAndSeparatorZones(0, 0);
            CreateFinderAndSeparatorZones(Width - 8, 0);
            CreateFinderAndSeparatorZones(0, Width - 8);
            CreateTimingPatternsZone();
            AddDarkModuleZone();
            CreateVersionInfoZone();
            CreateAlignmentPatternZones();
        }


        private void CreateAlignmentPatternZones()
        {
            if (Version < 2) return;
            var coordinates = new List<int>(GetQRAlignmentCoordinates());
            var arrayOfCoordinates = Functions.DoubleIntCombinations(coordinates);
            foreach (var item in arrayOfCoordinates)
            {
                CreateAlignmentPatternZone(item[0], item[1]);
                CreateAlignmentPatternZone(item[1], item[0]);
                CreateAlignmentPatternZone(item[0], item[0]);
                CreateAlignmentPatternZone(item[1], item[1]);
            }
        }
        
        private void CreateAlignmentPatternZone(int line, int col)
        {
            if (IsFunctionModule[line, col]) return;
            for (var i = line - 2; i <= line + 2; i++)
            {
                for (var j = col - 2; j <= col + 2; j++)
                {
                    IsFunctionModule[i, j] = true;
                }
            }
        }

        private void CreateVersionInfoZone()
        {
            if (Version < 7) return;
            for (var i = 0; i < 18; i++)
            {
                var maskLine = (2 - i % 3) + Height - 11;
                var maskCol = (5 - i / 3);
                IsFunctionModule[maskLine, maskCol] = true;
                IsFunctionModule[maskCol, maskLine] = true;
            }
        }

        private void CreateTimingPatternsZone()
        {
            for (var line = 7; line <= Height - 7; line++)
            {
                IsFunctionModule[line, 6] = true;
            }
        
            for (var col = 7; col <= Width - 7; col++)
            {
                IsFunctionModule[6, col] = true;
            }
        }

        private void AddDarkModuleZone()
        {
            var startingLine = (4 * Version + 9);
            IsFunctionModule[startingLine, 8] = true;
        }

        private void CreateFinderAndSeparatorZones(int line, int col)
        {
            for (var i = line; i < 8 + line; i++)
            {
                for (var j = col; j < 8 + col; j++)
                {
                    IsFunctionModule[i, j] = true;
                }
            }
        }
    
    }
}