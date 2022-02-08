using System;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;

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
           // test.ConvertToGrey();
            //test.DoubleConvolutionFilter(Kernel.SobelX, Kernel.SobelY);
            //test.ConvolutionFilter(Kernel.Contour);
            //test2.Rotate90L();
           test.Minimize(2.2);
            Functions.WriteImage(test, @"../../../images/Test4.bmp");
            
        }
    }
}