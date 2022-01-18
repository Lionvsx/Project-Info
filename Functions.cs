using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;


namespace Project_Info
{
    public static class Functions
    {
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
            image.Size = ConvertToInt(fsize);
            image.Offset = ConvertToInt(off);
            image.BitRgb = ConvertToInt(bpp);
            
            List<byte>
            
            //L'image elle-même
            for (int i = 54; i < myfile.Length; i = i + ConvertToInt(wid))
            {
                for (int j = i; j < i + ConvertToInt(wid); j++)
                {
                    
                }
            }
            
        }

        public static int ConvertToInt(IEnumerable<byte> data)
        {
            int result = 0;
            var enumerable = data.ToList();
            for (var i = 0; i < enumerable.Count; i++)
            {
                result = (int) (result + enumerable[i] * Math.Pow(256, i));
            }
            return result;
        }
        public static byte[] ConvertToByte(int data)
        {
            byte[] endian = new byte[4];
            for (var i = 3; i >= 0; i--)
            {
                endian[i] = Convert.ToByte(data % Convert.ToInt32(Math.Pow(256,i )));
                data -= endian[i] * Convert.ToInt32(Math.Pow(256, i));
            }
            return endian;
        }
    }
}