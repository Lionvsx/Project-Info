using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

using ReedSolomonCore;

namespace Project_Info
{
    class Program
    {

        static void Main(string[] args)
        {
            QRCode.InitializeAlphaNumericTable();
            //var test = Functions.ReadImage("../../../images/Test.bmp");
            var bjr = Functions.ConvertIntToBinaryArray(1);
            var QRTest = new QRCode(2, 0, 1);
            Functions.WriteImage(QRTest, "../../../images/Test7.bmp");
            
            


            
        }
    }
}