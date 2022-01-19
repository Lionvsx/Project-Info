using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;


namespace Project_Info
{
    public static class Functions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Image ReadImage(string path)
        {
            var image = new Image();
            
            byte[] myfile = File.ReadAllBytes(path);
            //myfile est un vecteur composé d'octets représentant les métadonnées et les données de l'image
           
            //Métadonnées du fichier
            byte[] sign = {myfile[0],myfile[1]};
            byte[] fsize = {myfile[2], myfile[3], myfile[4], myfile[5]};
            byte[] off = {myfile[10], myfile[11], myfile[12], myfile[13]};
            //Métadonnées de l'image
            byte[] wid = {myfile[18], myfile[19], myfile[20], myfile[21]};
            byte[] hei = {myfile[22], myfile[23], myfile[24], myfile[25]};
            byte[] bpp = {myfile[28], myfile[29]};

            image.Height = ConvertToInt(hei);
            image.Width = ConvertToInt(wid);
            image.Offset = ConvertToInt(off);
            image.Size = image.Height * image.Width * 3 + image.Offset;
            image.BitRgb = ConvertToInt(bpp);
            

            var imageData = new Pixel[image.Height,image.Width];
            
            //L'image elle-même
            var line = image.Height-1;
            var emptyBytes = (4 - (image.Width * 3 % 4))%4;
            for (var i = 54; i < image.Size; i = i + (image.Width*3 + emptyBytes))
            {
                var col = 0;
                for (var j = i; j < i + image.Width*3; j++)
                {
                    switch (col % 3)
                    {
                        case 0:
                            imageData[line, col / 3] = new Pixel
                            {
                                Red = myfile[j]
                            };
                            break;
                        case 1:
                            imageData[line, col / 3].Green = myfile[j];
                            break;
                        case 2:
                            imageData[line, col / 3].Blue = myfile[j];
                            break;
                    }
                    ++col;
                }

                --line;
            }

            image.ImageData = imageData;
            return image;

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="im"></param>
        /// <returns></returns>
        public static byte[] WriteImage(Image im)
        {
            var file = new List<byte>();
            const byte vide = byte.MinValue;
            const int b = 66;
            const int m = 77;
            var type = new List<byte>(){b, m};
            var size2 = new List<byte>(){vide, vide};
            var size4 = new List<byte>(){vide, vide, vide, vide};
            
            file = (List<byte>) file.Concat(type);
            for (var i = 0; i < ConvertToEndian(im.Size,4).Length; i++)
            {
                file.Add(ConvertToEndian(im.Size,4)[i]);
            }
            
            file = (List<byte>) file.Concat(size4);
            
            for (var i = 0; i < ConvertToEndian(im.Offset, 4).Length; i++)
            {
                file.Add(ConvertToEndian(im.Offset, 4)[i]);
            }
            
            file = (List<byte>) file.Concat(size4);
            
            for (var i = 0; i < ConvertToEndian(im.Width,4).Length; i++)
            {
                file.Add(ConvertToEndian(im.Width,4)[i]);
            }
            
            for (var i = 0; i < ConvertToEndian(im.Height,4).Length; i++)
            {
                file.Add(ConvertToEndian(im.Height,4)[i]);
            }
            file  = (List<byte>) file.Concat(size2);
            for (var i = 0; i < ConvertToEndian(im.BitRgb, 2).Length; i++)
            {
                file.Add(ConvertToEndian(im.BitRgb, 2)[i]);
            }
            for (var i = 0; i < 6; i++)
            {
                file = (List<byte>) file.Concat(size4);
            }

            for (var i = im.ImageData.GetLength(0) - 1; i >= 0; i--)
            {
                for (var j = 0; j < im.ImageData.GetLength(1); j++)
                {
                    file.Add(Convert.ToByte(im.ImageData[i,j].Red));
                    file.Add(Convert.ToByte(im.ImageData[i,j].Green));
                    file.Add(Convert.ToByte(im.ImageData[i,j].Blue));
                }
            }
            return file.ToArray();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int ConvertToInt(IEnumerable<byte> data)
        {
            var result = 0;
            var enumerable = data.ToList();
            for (var i = 0; i < enumerable.Count; i++)
            {
                Console.WriteLine(enumerable[i]);
                result = (int) (result + enumerable[i] * Math.Pow(256, i));
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] ConvertToEndian(int data, int size)
        {
            var endian = new byte[size];
            for (var i = 3; i >= 0; i--)
            {
                endian[i] = Convert.ToByte(data % Convert.ToInt32(Math.Pow(256,i )));
                data -= endian[i] * Convert.ToInt32(Math.Pow(256, i));
            }
            return endian;
        }
    }
}