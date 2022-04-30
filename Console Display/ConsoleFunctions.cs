using System;

namespace Project_Info.Console_Display
{
    public static class ConsoleFunctions
    {
        public static void ClearConsole()
        {
            Console.Clear();
        }

        public static void DisplayQRCode(QRCode.QRCode qrCode)
        {
            for (int line = 0; line < qrCode.Height; line++)
            {
                for (int col = 0; col < qrCode.Width; col++)
                {
                    Console.Write(qrCode.ImageData[line, col].IsBlack ? "██" : "  ");
                }
                Console.WriteLine();
            }
        }
    }
}