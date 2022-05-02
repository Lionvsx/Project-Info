using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Project_Info.QRCode
{
    /// <summary>
     /// Cette classe regroupe toute les methodes lié à la lecture de QR Code
     /// La classe est lié a la classe QRCode
     /// </summary>
    public class QRReader : QRCode
    {
        /// <summary>
        /// Constructeur de la classe qui permet d'utiliser toutes les methodes de la classe et ainsi de d'afficher le message du QR code mis en parametre via le math
        /// </summary>
        /// <param name="path"></param>
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
        /// <summary>
        /// Cette methode permet d'isoler de la caine des binaire la data contenant le message à trouver
        /// </summary>
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
        /// <summary>
        /// Cettte methode permet de retranscrire la chaine de binaire en un string d'alphanumerques et de l'afficher
        /// </summary>
        private void GetWordData()
        {
            var desiredLength = Version < 10 ? 9 : Version < 27 ? 11 : 13;
            var word = new List<char>();
            var wordlength = 0;
            for (var x = 0; x <desiredLength; x++)
            {
                if (WordEncodedData[x+4]==1) 
                    wordlength += Convert.ToInt32(Math.Pow(2, desiredLength - x-1));
            }

            var padding = WordEncodedData.Count - (wordlength / 2) * 11 - (wordlength % 2) * 6 - 4 - desiredLength;
            for (var index = 4 + desiredLength; index < (wordlength / 2) * 11+desiredLength+4; index+=11)
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
            if(wordlength%2!=0)
            {
                var remainder = 0;
                for (var j = 0; j < (wordlength % 2) * 6; j++)
                {
                    if (WordEncodedData[j + (wordlength / 2) * 11 + desiredLength + 4] == 1)
                        remainder += Convert.ToInt32(Math.Pow(2, 5 - j));
                }

                var Value = AlphanumericTable.FirstOrDefault(x => x.Value == remainder).Key;
                word.Add(Value);
            }
            foreach (var item in word)
            {
                Write(item);
            }
            WriteLine();
        }
        /// <summary>
        /// Cette méthode permet de réaranger la chaine de binaire contenat la data selon l'arangement des blocs et des groupes
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
        /// <summary>
        /// Cette methode permet d'extraire une chaine de binaire contenant le message du QR codes initil
        /// </summary>
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
        /// <summary>
        /// Cette méthode permet d'obtenir les informations concerant le niveau de correction ainsi que le masque afin de pouvoir par la suite traduire correctement la data
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
        /// <summary>
        /// Cette methode permet d'extraire les informations de format, le masque et la correction level, du QR COde initiale
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Cette méthode permet de transformer la module width du QR Code initial en  afin de faciliter la réalisation du rest des methodes
        /// </summary>
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
        /// <summary>
        /// Cette methode permet de supprimer la bordure mise autour du QR COde initaile afi de faciliter la réalisation du rest de methodes de la classe
        /// </summary>
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