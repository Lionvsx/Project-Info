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
            // var test = new byte[] {255, 0, 0, 0};
            // var endian = Functions.ConvertToEndian(2500, 4);
            var test = Functions.ReadImage(@"../../../images/lac.bmp");
            test.Minimize(2);
            //test2.Rotate90L();
            //test2.Maximize(2);
            Functions.WriteImage(test, @"../../../images/Test3.bmp");
            Console.WriteLine("Test");
        }
    }
}