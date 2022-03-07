using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;

namespace Project_Info
{
    class Program
    {

        static void Main(string[] args)
        {
            //var test = Functions.ReadImage("../../../images/Test.bmp");
            
            var QRTest = new QRCode(4, 0, 2);
            Functions.WriteImage(QRTest, "../../../images/Test7.bmp");





        }
    }
}