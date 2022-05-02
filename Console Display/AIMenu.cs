using System.Collections.Generic;
using static System.Console;

namespace Project_Info.Console_Display
{
    public static class AIMenu
    {
        public static void Invoke()
        {
            OpenAI.Login();
            OpenAI.SteamInConsole("Bonjour"); 
            while (true)
            {
                var prompt = ReadLine();
                var output = OpenAI.GetStringCommand(prompt);
                if (TryRunCommand(output)) break;
                OpenAI.SteamInConsole(prompt);
                WriteLine();
            }
        }

        public static bool TryRunCommand(string[] input)
        {
            switch (input[0])
            {
                case "exit":
                    return true;
                case "create-qrcode":
                    if (input.Length < 2)
                    {
                        //Commands.CreateQRCommand(input[1]);
                    }
                    else
                    {
                        var moduleWidth = 1;
                        string ecLevel = "L";
                        var quietZoneWidth = 2;
                        for (var index = 2; index < input.Length; index++)
                        {
                            var parameter = input[index];
                            var paramsArray = parameter.Split('=');
                            moduleWidth = paramsArray[0] == "moduleWidth" ? int.Parse(paramsArray[1]) : moduleWidth;
                            ecLevel = paramsArray[0] == "ecLevel" ? paramsArray[1] : ecLevel;
                            quietZoneWidth = paramsArray[0] == "quietZoneWidth" ? int.Parse(paramsArray[1]) : quietZoneWidth;
                        }
                    }
                    return true;
                case "read-qrcode":
                    if (input[1] == "get-path")
                    {
                        WriteLine("Merci de spécifier un chemin");
                        var path = ReadLine();
                        if (path == null)
                        {
                            WriteLine("Chemin invalide");
                            return false;
                        }
                        //Commands.ReadQRCommand(path);
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
                    //Commands.CreateFractaleCommand();
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
    }
}