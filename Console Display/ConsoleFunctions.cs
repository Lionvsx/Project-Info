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
        
        public static void DisplayBoolQRCodeMatrix(bool[,,] qrCode, int index)
        {
            for (int line = 0; line < qrCode.GetLength(0); line++)
            {
                for (int col = 0; col < qrCode.GetLength(1); col++)
                {
                    Console.Write(qrCode[line, col, index] ? "██" : "  ");
                }
                Console.WriteLine();
            }
        }
        
        public static void DisplayBoolQRCode(bool[,] qrCode)
        {
            for (int line = 0; line < qrCode.GetLength(0); line++)
            {
                for (int col = 0; col < qrCode.GetLength(1); col++)
                {
                    Console.Write(qrCode[line, col] ? "██" : "  ");
                }
                Console.WriteLine();
            }
        }

        public static void DisplayIntChain(int[] chain)
        {
            for (var index = 0; index < chain.Length; index++)
            {
                if (index % 8 == 0 && index != 0) Console.Write(" ");
                var bit = chain[index];
                Console.Write(bit);
            }
            Console.WriteLine();
        }

        public static void DisplayAppHeader()
        {
            Console.WriteLine("  _____           _           _     _____        __      ");
            Console.WriteLine(" |  __ \\         (_)         | |   |_   _|      / _|     ");
            Console.WriteLine(" | |__) | __ ___  _  ___  ___| |_    | |  _ __ | |_ ___  ");
            Console.WriteLine(" |  ___/ '__/ _ \\\\| |/ _ \\/ __| __|   | | | '_ \\|  _/ _ \\ ");
            Console.WriteLine(" | |   | | | (_) | |  __/ (__| |_   _| |_| | | | || (_) |");
            Console.WriteLine(" |_|   |_|  \\___/| |\\___|\\___|\\__| |_____|_| |_|_| \\___/ ");
            Console.WriteLine("                _/ |                                     ");
            Console.WriteLine("               |__/                                      ");

        }

        public static void DisplayAppHeader2()
        {
            Console.WriteLine( "                                                                                                                                                    ");
            Console.WriteLine( " ▄▄▄▄▄▄▄▄▄▄▄  ▄▄▄▄▄▄▄▄▄▄▄  ▄▄▄▄▄▄▄▄▄▄▄  ▄▄▄▄▄▄▄▄▄▄▄  ▄▄▄▄▄▄▄▄▄▄▄  ▄▄▄▄▄▄▄▄▄▄▄  ▄▄▄▄▄▄▄▄▄▄▄    ");
            Console.WriteLine( "▐░░░░░░░░░░░▌▐░░░░░░░░░░░▌▐░░░░░░░░░░░▌▐░░░░░░░░░░░▌▐░░░░░░░░░░░▌▐░░░░░░░░░░░▌▐░░░░░░░░░░░▌   ");
            Console.WriteLine( "▐░█▀▀▀▀▀▀▀█░▌▐░█▀▀▀▀▀▀▀█░▌▐░█▀▀▀▀▀▀▀█░▌ ▀▀▀▀▀█░█▀▀▀ ▐░█▀▀▀▀▀▀▀▀▀ ▐░█▀▀▀▀▀▀▀▀▀  ▀▀▀▀█░█▀▀▀▀    ");
            Console.WriteLine( "▐░▌       ▐░▌▐░▌       ▐░▌▐░▌       ▐░▌      ▐░▌    ▐░▌          ▐░▌               ▐░▌        ");
            Console.WriteLine( "▐░█▄▄▄▄▄▄▄█░▌▐░█▄▄▄▄▄▄▄█░▌▐░▌       ▐░▌      ▐░▌    ▐░█▄▄▄▄▄▄▄▄▄ ▐░▌               ▐░▌        ");
            Console.WriteLine( "▐░░░░░░░░░░░▌▐░░░░░░░░░░░▌▐░▌       ▐░▌      ▐░▌    ▐░░░░░░░░░░░▌▐░▌               ▐░▌        ");
            Console.WriteLine( "▐░█▀▀▀▀▀▀▀▀▀ ▐░█▀▀▀▀█░█▀▀ ▐░▌       ▐░▌      ▐░▌    ▐░█▀▀▀▀▀▀▀▀▀ ▐░▌               ▐░▌        ");
            Console.WriteLine( "▐░▌          ▐░▌     ▐░▌  ▐░▌       ▐░▌      ▐░▌    ▐░▌          ▐░▌               ▐░▌        ");
            Console.WriteLine( "▐░▌          ▐░▌      ▐░▌ ▐░█▄▄▄▄▄▄▄█░▌ ▄▄▄▄▄█░▌    ▐░█▄▄▄▄▄▄▄▄▄ ▐░█▄▄▄▄▄▄▄▄▄      ▐░▌        ");
            Console.WriteLine( "▐░▌          ▐░▌       ▐░▌▐░░░░░░░░░░░▌▐░░░░░░░▌    ▐░░░░░░░░░░░▌▐░░░░░░░░░░░▌     ▐░▌        ");
            Console.WriteLine( " ▀            ▀         ▀  ▀▀▀▀▀▀▀▀▀▀▀  ▀▀▀▀▀▀▀      ▀▀▀▀▀▀▀▀▀▀▀  ▀▀▀▀▀▀▀▀▀▀▀       ▀         ");
            Console.WriteLine( "                                                                                                                                                    ");
            Console.WriteLine("");
            Console.WriteLine("                   ▄▄▄▄▄▄▄▄▄▄▄  ▄▄        ▄  ▄▄▄▄▄▄▄▄▄▄▄  ▄▄▄▄▄▄▄▄▄▄▄ ");
            Console.WriteLine("                  ▐░░░░░░░░░░░▌▐░░▌      ▐░▌▐░░░░░░░░░░░▌▐░░░░░░░░░░░▌");
            Console.WriteLine("                   ▀▀▀▀█░█▀▀▀▀ ▐░▌░▌     ▐░▌▐░█▀▀▀▀▀▀▀▀▀ ▐░█▀▀▀▀▀▀▀█░▌");
            Console.WriteLine("                       ▐░▌     ▐░▌▐░▌    ▐░▌▐░▌          ▐░▌       ▐░▌");
            Console.WriteLine("                       ▐░▌     ▐░▌ ▐░▌   ▐░▌▐░█▄▄▄▄▄▄▄▄▄ ▐░▌       ▐░▌");
            Console.WriteLine("                       ▐░▌     ▐░▌  ▐░▌  ▐░▌▐░░░░░░░░░░░▌▐░▌       ▐░▌");
            Console.WriteLine("                       ▐░▌     ▐░▌   ▐░▌ ▐░▌▐░█▀▀▀▀▀▀▀▀▀ ▐░▌       ▐░▌");
            Console.WriteLine("                       ▐░▌     ▐░▌    ▐░▌▐░▌▐░▌          ▐░▌       ▐░▌");
            Console.WriteLine("                   ▄▄▄▄█░█▄▄▄▄ ▐░▌     ▐░▐░▌▐░▌          ▐░█▄▄▄▄▄▄▄█░▌");
            Console.WriteLine("                  ▐░░░░░░░░░░░▌▐░▌      ▐░░▌▐░▌          ▐░░░░░░░░░░░▌");
            Console.WriteLine("                   ▀▀▀▀▀▀▀▀▀▀▀  ▀        ▀▀  ▀            ▀▀▀▀▀▀▀▀▀▀▀ ");


        }
    }
}