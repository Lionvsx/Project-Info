using System;
using Project_Info.Console_Display;

namespace Project_Info
{
    class Program
    {
        static void Main()
        {
            QRCode.QRCode.InitializeAlphaNumericTable();
            //var test = Functions.ReadImage("../../../images/Test.bmp");
            //var QRTest = new QRCode(1, 0, 1, 5, 1, "NIQUE TA RACE");
            var Qr = new QRCode.QRCode("HELLO WORLD", 2);
            Functions.WriteImage(Qr, "../../../images/Test7.bmp");
            //var QRTest = new QRCode(1, 0, 1, 5, 1, "NIQUE TA RACE");
            var QRTest = Functions.ReadImage("../../../images/Test7.bmp");
            var data = QRCode.QRCode.ReadQrCode(QRTest);
            Console.WriteLine(data);
            //var QRTest = new QRCode.QRCode("https://www.notion.so/fr-fr", 3);
            
            
        }
    }
}