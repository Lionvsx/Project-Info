using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_Info.QRCode
{
    public class QRReader : QRCode
    {
        public QRReader(string path)
        {
            var image = Functions.ReadImage(path);
            
            ImageData = image.ImageData;
            Width = image.Width;
            Height = image.Height;
            Offset = image.Offset;
            BitRgb = image.BitRgb;
            Type = image.Type;

            DiscoverAndDeleteQuietZone();
            DiscoverModuleWidthAndMinimized();
            
            Version = Width / (4 * 1) - 17 / 4;
            
            InitFunctionModulesMatrix();
            DiscoverFormatInfo();
            AddMask();
            ReadData();
            DecodeData();
            CleanWordData();
            GetWordData();
        }

        private void CleanWordData()
        {
            for (int i = WordEncodedData.Count - 8; i >= 0; i-=8)
            {
                var tempArray = WordEncodedData.GetRange(i, 8).ToArray();
                var byte1 = Functions.IntToDesiredLengthBit(236, 8);
                var byte2 = Functions.IntToDesiredLengthBit(17, 8);
                var byte3 = Functions.IntToDesiredLengthBit(0, 8);
                
                if (Functions.CheckArrayEquality(tempArray, byte1) || Functions.CheckArrayEquality(tempArray, byte2) || Functions.CheckArrayEquality(tempArray, byte3))
                {
                    WordEncodedData.RemoveRange(i, 8);
                }
            }
        }

        private void GetWordData()
        {
            var desiredLength = Version < 10 ? 9 : Version < 27 ? 11 : 13;
            var word = new List<char>();
            
            for (var index = 4 + desiredLength; index < WordEncodedData.Count - 11; index+=11)
            {
                var value = 0;
                for (var i = 0; i <=10; i++)
                {
                    if (WordEncodedData[i+index]==1)
                        value += Convert.ToInt32(Math.Pow(2, 10 - i));
                }

                var highValue = AlphanumericTable.FirstOrDefault(x => x.Value == value / 45).Key;
                var lowValue = AlphanumericTable.FirstOrDefault(x => x.Value == value % 45).Key;
                word.Add(highValue);
                word.Add(lowValue);
            }

            foreach (var item in word)
            {
                Console.Write(item);
            }
        }

        private void DecodeData()
        {
            var lines = Functions.ReadFile("../../../QRCode/qrCodeDataLength.txt").ToArray();
            var startIndex = CorrectionLevel switch
            {
                1 => 0,
                3 => 2,
                2 => 3,
                0 => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(CorrectionLevel))
            };


            var infos = lines[(Version - 1) * 4 + startIndex].Split(";");
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
            
            var numberDataPerBlockArray = new[] {NumberDataPerBlockGrp1, NumberDataPerBlockGrp2};
            
            var maxNumberDataPerBlock = Math.Max(NumberDataPerBlockGrp1, NumberDataPerBlockGrp2);

            var data = Functions.ConvertBitArrayToByteArray(QRCodeData.ToArray());
            
            var decodedDataMatrix = new int[NumberBlocksGroup1 + NumberBlocksGroup2, maxNumberDataPerBlock];

            var dataIndex = 0;
            for (var j = 0; j < decodedDataMatrix.GetLength(1); j++)
            {
                for (var i = 0; i < decodedDataMatrix.GetLength(0); i++)
                {
                    decodedDataMatrix[i, j] = data[dataIndex];
                    var currentNumberDataPerBlock = numberDataPerBlockArray[i / NumberBlocksGroup1];
                    if (j >= currentNumberDataPerBlock)
                    {
                        decodedDataMatrix[i, j] = -1;
                        continue;
                    }
                    dataIndex++;
                }
            }
            
            var message = new List<int>();
            for (var i = 0; i < decodedDataMatrix.GetLength(0); i++)
            {
                for (var j = 0; j < decodedDataMatrix.GetLength(1); j++)
                {
                    if (decodedDataMatrix[i,j] == -1) continue;
                    message.Add(decodedDataMatrix[i,j]);
                }
            }
            
            WordEncodedData = Functions.ConvertByteArrayToBitArray(message.ToArray()).ToList();
        }

        private void ReadData()
        {
            var binaryData = new List<int>();
            var stopNumber = 1000000;
            var upp = true;
            var cpt = 0;
            var skip = false;
                for (var col = Width - 1; col > 0; col -=2)
                {
                    if (cpt >= stopNumber-1) break;
                    if (upp)
                    {
                        if (cpt >= stopNumber-1) break;
                        for (var line = Height - 1; line >= 0; line--)
                        {
                            if (cpt >= stopNumber-1) break;
                            
                            //SKIP TIMING PATTERNS
                            if (col <= 7 && skip == false)
                            {
                                col--;
                                skip = true;
                            }
                            
                            if (!IsFunctionModule[line, col]) 
                            {
                                binaryData.Add(ImageData[line, col].IsBlack ? 1 : 0);
                                cpt++;
                            }
                            
                            if (IsFunctionModule[line, col - 1]) continue;
                            binaryData.Add(ImageData[line, col - 1].IsBlack ? 1 : 0);
                            cpt++;
                        }
                    }
                    else
                    {
                        if (cpt >= stopNumber-1) break;
                        for (var line = 0; line <= Height - 1; line++) 
                        {
                            if (cpt >= stopNumber-1) break;
                            //SKIP TIMING PATTERNS
                            if (col <= 7 && skip == false)
                            {
                                col --;
                                skip = true;
                            }
                            
                            if (!IsFunctionModule[line, col]) 
                            {
                                binaryData.Add(ImageData[line, col].IsBlack ? 1 : 0);
                                cpt++;
                            }

                            if (IsFunctionModule[line, col - 1]) continue;
                            binaryData.Add(ImageData[line, col - 1].IsBlack ? 1 : 0);
                            cpt++;
                        }
                    }
                    upp = !upp;

                }
            QRCodeData = binaryData.ToArray();
        }

        private void DiscoverFormatInfo()
        {
            var formatString = DiscoverFormatInfoString();
            
            var lines = Functions.ReadFile("../../../QRCode/qrFormat.txt");
            foreach (var line in lines)
            {
                var infos = line.Split(';');
                if (infos[2] != formatString) continue;
                CorrectionLevel = infos[0] switch
                {
                    "L" => 1,
                    "M" => 0,
                    "Q" => 3,
                    "H" => 2,
                    _ => throw new ArgumentOutOfRangeException()
                };
                MaskPattern = int.Parse(infos[1]);
                break;
            }
            
        }
        
        public string DiscoverFormatInfoString()
        {
            const int fixedLine = 8;
            const int fixedCol = 8;
            var binaryFormat = "";

            var movingCol = 0;
            var movingLine = Height - 1;
            for (var i = 0; i < 15; i++)
            {
                if (movingCol == 6) movingCol += 1;
                if (movingLine == 6) movingLine -= 1;
                
                binaryFormat += ImageData[fixedLine, movingCol].IsBlack ? "1" : "0";
                
                IsFunctionModule[fixedLine, movingCol] = true;
                IsFunctionModule[movingLine, fixedCol] = true;
                if (movingLine - 1 == (4 * Version + 9) &&
                    movingCol == 7)
                {
                    movingCol = Width - 8;
                    movingLine = 8;
                    continue;
                }
                movingLine -= 1;
                movingCol += 1;
            }

            return binaryFormat;
        }

        private void DiscoverModuleWidthAndMinimized()
        {
            var moduleWidth = 0;
            while (ImageData[moduleWidth, moduleWidth].IsBlack)
            {
                moduleWidth++;
            }
            ModuleWidth = 1;
            if (moduleWidth == 1) return; 
            Minimize(moduleWidth);
        }

        private void DiscoverAndDeleteQuietZone()
        {
            var localQuietZoneWidth = 0;
            while (!ImageData[localQuietZoneWidth, localQuietZoneWidth].IsBlack)
            {
                localQuietZoneWidth++;
            }
            QuietZoneWidth = 0;
            if (localQuietZoneWidth == 0) return;
            
            var newImageData = new Pixel[Width - localQuietZoneWidth * 2, Height - localQuietZoneWidth * 2];
            for (var i = localQuietZoneWidth; i < Width - localQuietZoneWidth; i++)
            {
                for (var j = localQuietZoneWidth; j < Height - localQuietZoneWidth; j++)
                {
                    newImageData[i - localQuietZoneWidth, j - localQuietZoneWidth] = ImageData[i, j];
                }
            }
            Height -= localQuietZoneWidth * 2;
            Width -= localQuietZoneWidth * 2;

            ImageData = newImageData;
        }
    }
}