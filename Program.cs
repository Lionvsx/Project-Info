using System;
using System.IO;
using Project_Info.Console_Display;
using Project_Info.QRCode;

/***
 *      _____           _           _     _____        __      
 *     |  __ \         (_)         | |   |_   _|      / _|     
 *     | |__) | __ ___  _  ___  ___| |_    | |  _ __ | |_ ___  
 *     |  ___/ '__/ _ \| |/ _ \/ __| __|   | | | '_ \|  _/ _ \ 
 *     | |   | | | (_) | |  __/ (__| |_   _| |_| | | | || (_) |
 *     |_|   |_|  \___/| |\___|\___|\__| |_____|_| |_|_| \___/ 
 *                    _/ |                                     
 *                   |__/                                      
 */

namespace Project_Info
{
    class Program
    {
        static void Main()
        {
            OpenAI.Login();
            ConsoleFunctions.DisplayAppHeader2();
            QRCode.QRCode.InitializeAlphaNumericTable();
            //var test = Functions.ReadImage("../../../images/Test.bmp");
            //var QRTest = new QRCode(1, 0, 1, 5, 1, "NIQUE TA RACE");
            var qrTest = new QRCode.QRCode("HELLO EST CE QUE TU VEUX QUE JE TE BAISE TA MERE OU PASSSSSSS", 2);
            ConsoleFunctions.DisplayIntChain(qrTest.WordEncodedData.ToArray());
            Functions.WriteImage(qrTest, "../../../images/Test7.bmp");

            var decodedQR = new QRReader("../../../images/Test7.bmp");
            Console.WriteLine();
            ConsoleFunctions.DisplayIntChain(decodedQR.WordEncodedData.ToArray());

        }
    }
}