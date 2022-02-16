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
            var test = Functions.ReadImage(@"../../../images/lac.bmp");

            //test.ConvertToGrey();
            //test.DoubleConvolutionFilter(Kernel.SobelX, Kernel.SobelY);
            //test.ConvolutionFilter(Kernel.Contour);
            //test.RotateAngle(Math.PI / 8);
            //test.DisplayImage();
            //test.Rotate90L();
           
           var testb = Functions.Histograme(test);
           if (test.Height > 300)
           {
               testb.Minimize(5);
           }
           
            Functions.WriteImage(testb, @"../../../images/Test6.bmp");
            
           
        }
    }
}