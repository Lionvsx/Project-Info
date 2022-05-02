using System;
using System.Collections.Generic;
using static System.Console;

namespace Project_Info.Console_Display
{
    public static class AIMenu
    {
        public static void Initialize()
        {
            OpenAI.Login();
            OpenAI.SteamInConsole("Ecris un tutoriel afin d'utiliser ce programme"); 
        }
        public static void Invoke()
        {
            while (true)
            {
                var prompt = ReadLine();
                var output = OpenAI.GetStringCommand(prompt);
                if (TryRunCommand(output)) break;
                OpenAI.SteamInConsole(prompt);
            }
            
            WriteLine("Voulez vous fermer le programme?");
            var choice = ReadLine();
            if (!OpenAI.AskForYesOrNo(choice)) Invoke();
        }

        public static bool TryRunCommand(string[] input)
        {
            switch (input[0])
            {
                case "exit":
                    return true;
                case "create-qrcode":
                    if (input.Length < 3)
                    {
                        Commands.CreateQRCommand(input[1], "L");
                    }
                    else
                    {
                        string moduleWidth = "1";
                        string ecLevel = "L";
                        for (var index = 2; index < input.Length; index++)
                        {
                            var parameter = input[index];
                            var paramsArray = parameter.Split('=');
                            moduleWidth = paramsArray[0] == "moduleWidth" ? paramsArray[1] : moduleWidth;
                            ecLevel = paramsArray[0] == "ecLevel" ? paramsArray[1] : ecLevel;
                        }
                        Commands.CreateQRCommandAdvanced(input[1], ecLevel, moduleWidth);
                    }
                    return true;
                case "read-qrcode":
                    if (input[1] == "get-path")
                    {
                        var path = GetPath();
                        if (!path.Item2) return false;
                        Commands.ReadQRCommand(path.Item1);
                    }
                    else
                    {
                        //Commands.ReadQRCommand(input[1]);
                    }
                    return true;
                case "rotate-image":
                    return true;
                case "maximize":
                    return true;
                case "minimize":
                    return true;
                case "create-fractale":
                    Commands.CreateFractaleCommand();
                    return true;
                case "hide":
                    return true;
                case "found":
                    return true;
                case "convert-to-grey":
                    return true;
                case "convolution-filter":
                    return true;
                case "create-histogramme":
                    return true;
                default:
                    return false;
            }
        }

        private static (string, bool) GetPath()
        {
            WriteLine("Merci de spécifier un chemin");
            var path = ReadLine();
            if (path != null) return (path, true);
            WriteLine("Chemin invalide");
            return ("", false);

        }
    }
}