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
            Console.WriteLine("(Appuyez sur une touche pour continuer)");
            Console.ReadKey();
            QRCode.QRCode.InitializeAlphaNumericTable();
            AIMenu.Initialize();
            AIMenu.Invoke();
        }
    }
}