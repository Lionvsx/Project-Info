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
            ConsoleFunctions.ClearConsole();
            WriteLine("Bienvenue dans notre projet info de traitement d'image\n" +
                                  "Je suis une intelligence artificielle qui vous aidera à naviguer dans notre programme\n" +
                                  "Toutes les images que vous souhaiterez utiliser dans notre programme sont à placer dans le dossier /ImageInput/ à la racine du projet\n" +
                                  "Toutes les images créees par le programme pourront être trouvées dans le dossier /images/ à la racine du projet\n" +
                                  "Afin de naviguer dans le programme, parlez moi dans la console comme si vous parliez à un assistant intelligent !\n" +
                                  "Vous pouvez par exemple me demander qu'est ce que le programme est capable de faire." +
                                  "\nBonne navigation !\n");
        }
        public static void Invoke()
        {
            WriteLine("Veuillez entrez votre requête :");
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
                        for (var index = 1; index < input.Length; index++)
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
                        Commands.ReadQRCommand(input[1]);
                    }
                    return true;
                case "rotate-image":
                    var deg = input[2].Split("=")[1];
                    if (input[1] == "get-path")
                    {
                        var path = GetPath();
                        if (!path.Item2) return false;
                        Commands.RotationCommand(path.Item1,deg);
                    }
                    else
                    {
                        Commands.RotationCommand(input[1],deg);
                        
                    }
                    return true;
                case "maximize":
                   var factor = input[2].Split("=")[1];
                    if (input[1] == "get-path")
                    {
                        var path = GetPath();
                        if (!path.Item2) return false;
                        Commands.MaximizeCommand(path.Item1,factor);
                        
                    }
                    else
                    {
                        Commands.MaximizeCommand(input[1],factor);
                        
                    }
                    return true;
                case "minimize":
                    var ty = input[2].Split("=")[1];
                    if (input[1] == "get-path")
                    {
                        var path = GetPath();
                        if (!path.Item2) return false;
                        Commands.MinimizeCommand(path.Item1,ty);
                        
                    }
                    else
                    {
                        Commands.MinimizeCommand(input[1],ty);
                        
                    }
                    return true;
                case "create-fractale":
                    Commands.CreateFractaleCommand();
                    return true;
                case "hide":
                    var smol = input[2].Split("=")[0];
                    var big = input[2].Split("=")[1];
                    
                        
                    Commands.HideCommand(big,smol);
                        
                    
                    return true;
                case "found":
                    
                    if (input[1] == "get-path")
                    {
                        var path = GetPath();
                        if (!path.Item2) return false;
                        Commands.FoundCommand(path.Item1);
                        
                    }
                    else
                    {
                        Commands.FoundCommand(input[1]);
                        
                    }
                    return true;
                    
                case "convert-to-grey":
                    
                    if (input[1] == "get-path")
                    {
                        var path = GetPath();
                        if (!path.Item2) return false;
                        Commands.ConvertToGreyCommand(path.Item1);
                        
                    }
                    else
                    {
                        Commands.ConvertToGreyCommand(input[1]);
                        
                    }
                    return true;
                case "convolution-filter":
                    var type = input[2].Split("=")[1];
                    if (input[1] == "get-path")
                    {
                        var path = GetPath();
                        if (!path.Item2) return false;
                        Commands.ConvolutionFilterCommand(path.Item1,type);
                        
                    }
                    else
                    {
                        Commands.ConvolutionFilterCommand(input[1],type);
                        
                    }
                    return true;
                case "create-histogramme":
                    
                    if (input[1] == "get-path")
                    {
                        var path = GetPath();
                        if (!path.Item2) return false;
                        Commands.CreateHistogrammeCommand(path.Item1);
                        
                    }
                    else
                    {
                        Commands.CreateHistogrammeCommand(input[1]);
                    }
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