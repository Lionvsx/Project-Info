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
            byte[] myfile = File.ReadAllBytes(path);
            //myfile est un vecteur composé d'octets représentant les métadonnées et les données de l'image

            //Métadonnées du fichier
            byte[] sign = {myfile[0], myfile[1]};
            byte[] fsize = {myfile[2], myfile[3], myfile[4], myfile[5]};
            byte[] off = {myfile[10], myfile[11], myfile[12], myfile[13]};
            //Métadonnées de l'image
            byte[] wid = {myfile[18], myfile[19], myfile[20], myfile[21]};
            byte[] hei = {myfile[22], myfile[23], myfile[24], myfile[25]};
            byte[] bpp = {myfile[28], myfile[29]};
            //L'image elle-même
            for (int i = 54; i < myfile.Length; i = i + ConvertToInt(wid))
            {
                for (int j = i; j < i + ConvertToInt(wid); j++)
                {


                }
            }

        }
        public static byte[] WriteImage(Image im)
        {
            List<byte> file = new List<byte>();
            
            List<byte> size2 = new List<byte>(){};
            file.Concat()
            for (int i = 0; i < ConvertToendian(im.Size,4).Length; i++)
            {
                file.Add(ConvertToendian(im.Size,4)[i]);
            }

            for (int i = 0; i < ConvertToendian(im.Offset, 4).Length; i++)
            {
                file.Add(ConvertToendian(im.Offset, 4)[i]);
            }

            for (int i = 0; i < ConvertToendian(im.Height,4).Length; i++)
            {
                file.Add(ConvertToendian(im.Height,4)[i]);
            }
            
            for (int i = 0; i < ConvertToendian(im.Width,4).Length; i++)
            {
                file.Add(ConvertToendian(im.Width,4)[i]);
            }
            
            for (int i = 0; i < ConvertToendian(im.BitRgb,2).Length; i++)
            {
                file.Add(ConvertToendian(im.BitRgb,2)[i]);
            }
        }
        
        public static int ConvertToInt(IEnumerable<byte> data)
        {
            int result = 0;
            var enumerable = data.ToList();
            for (var i = 0; i < enumerable.Count; i++)
            {
                Console.WriteLine(enumerable[i]);
                result = (int) (result + enumerable[i] * Math.Pow(256, i));
            }
            return result;
        }
        public static byte[] ConvertToendian(int data, int size)
        {
            byte[] endian = new byte[size];
            for (var i = 3; i >= 0; i--)
            {
                endian[i] = Convert.ToByte(data % Convert.ToInt32(Math.Pow(256,i )));
                data -= endian[i] * Convert.ToInt32(Math.Pow(256, i));
            }
            return endian;
        }
    }
}