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
            Console.WriteLine("\n Header \n");
            for (int i = 0; i < 14; i++)
                Console.Write(myfile[i] + " ");
            //Métadonnées de l'image
            Console.WriteLine("\n HEADER INFO \n");
            for (int i = 14; i< 54; i++)
                Console.Write(myfile[i] + " ");
        }

        public static double ConvertToInt(IEnumerable<byte> data)
        {
            double result = 0;
            var enumerable = data.ToList();
            for (var i = 0; i < enumerable.Count; i++)
            {
                Console.WriteLine(enumerable[i]);
                result = result + enumerable[i] * Math.Pow(256, i);
            }
            return result;
        }
    }
}