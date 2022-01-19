using System;
using System.Drawing;
using System.IO;

namespace Project_Info
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var test = new byte[] {255, 0, 0, 0};
            Console.WriteLine(Functions.ConvertToInt(test));
            var test2 = Functions.ReadImage(@"../../../images/Test.bmp");
            Functions.WriteImage(test2, @"../../../images/Test2.bmp");
        }
    }
}