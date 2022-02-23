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
            var test = Functions.ReadImage(@"../../../images/Test.bmp");

            //test.ConvertToGrey();
            //test.DoubleConvolutionFilter(Kernel.SobelX, Kernel.SobelY);
            //test.ConvolutionFilter(Kernel.Contour);
            //test.RotateAngle(Math.PI / 8);
            //test.DisplayImage();
            //test.Rotate90L();
           
           //test.RotateAngle(3*Math.PI/2);
           test.Maximize(25);
           test.RotateAngle(Math.PI);

           Functions.WriteImage(test, @"../../../images/Test6.bmp");

            


        }
    }
}