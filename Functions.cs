using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project_Info
{
    public static class Functions
    {
        public static Image ReadImage(string path)
        {
            byte[] myfile = File.ReadAllBytes(path);
            //myfile est un vecteur composé d'octets représentant les métadonnées et les données de l'image
           
            //Métadonnées du fichier
            byte[] sign = {myfile[0],myfile[1]};
            byte[] fsize = {myfile[2], myfile[3], myfile[4], myfile[5]};
            byte[] off = {myfile[10], myfile[11], myfile[12], myfile[13]};
            
            
            //Métadonnées de l'image
            for (int i = 14; i< 54; i++)
                Console.Write(myfile[i] + " ");
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
    }
}