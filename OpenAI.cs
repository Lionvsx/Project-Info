using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OpenAI_API;

namespace Project_Info
{
    public static class OpenAI
    {
        private static OpenAIAPI _api;

        private const string _apiKey = "";

        public static void Login()
        {
            _api = new OpenAIAPI(_apiKey, "text-davinci-002");
        }

        public static async void SteamInConsole(string input)
        {
            await _api.Completions.StreamCompletionAsync(
                new CompletionRequest(
                    $"Ceci est une conversation entre un humain et l'IA d'un programme de traitement d'image. L'IA est créative, gentille et très formelle. L'IA doit renseigner l'utilisateur sur ce que le programme peut faire.\n" +
                    $"Un example de ce que l'humain peut demander est de générer un qr code pour accéder au site de google.com." +
                    $"Toutes les images qui seront utilisées dans le programme doivent être dans le dossier ImageInput, les images sortant du programmes seront dans le dossier ImageOuput à la racine du projet" +
                    $"Voici la liste complète de ce que peut faire le programme : " +
                    $":\nHuman: {input}\nIA:",
                    400, 0.7, presencePenalty: 0.1, frequencyPenalty: 0.1, stopSequences: "Human:"),
                res => Console.Write(res.ToString()));
            Console.WriteLine();
        }



        public static async Task<CompletionResult> Completion(string input)
        {
            return await _api.Completions.CreateCompletionAsync(new CompletionRequest(input, temperature: 0.1,
                max_tokens: 1000, presencePenalty: 0.1, frequencyPenalty: 0.1));
        }

        public static async Task<bool> YesNoToCommand(string input)
        {
            var task = await _api.Completions.CreateCompletionAsync(new CompletionRequest(
                $"Convert this text to a programmatic command:\n" +
                $"Example: oui\nOutput: true\n" +
                $"Example: Je veux\nOutput: true\n" +
                $"Example: non\nOutput: false\n" +
                $"{input}:",
                temperature: 0, max_tokens: 200, presencePenalty: 0, frequencyPenalty: 0.2));
            var result = task.ToString();
            return result.Contains("true");
        }
        
        public static bool AskForYesOrNo(string input)
        {
            var test = YesNoToCommand(input);
            return test.Result;
        }

        public static string[] GetStringCommand(string input)
        {
            var result = TextToCommand(input);
            var command = result.Result.ToString();
            command = command.Remove(0, 2);
            return command.Split('|');
        }
        public static async Task<CompletionResult> TextToCommand(string input)
        {
            return await _api.Completions.CreateCompletionAsync(new CompletionRequest(
                $"Convert this text to a programmatic command:\n\n" +
                $"Example: Genère un qr code qui va sur le site de Google\nOutput: create-qrcode|https://google.com/\n\n" +
                $"Example: Genère un qr code qui envoie salut au 0620330631\nOutput: create-qrcode|SMSTO:0620330631:salut\n\n" +
                $"Example: Genère une image fractale\nOutput: create-fractale\n\n" +
                $"Example: Genère un qr code qui contient la phrase Il aime les pommes avec une taille de module de 3\nOutput: create-qrcode|Il aime les pommes|moduleWidth=3\n\n" +
                $"Example: Genère un qr code qui contient le texte Je joue au cartes avec un module de 2 et un niveau de correction Q\nOutput: create-qrcode|Je joue au cartes|moduleWidth=2|ecLevel=Q\n\n" +
                $"Example: Lire un qr code\nOutput: read-qrcode|get-path\n\n" +
                $"Example: Lire le qr code qrTest7.bmp\nOutput: read-qrcode|../../../ImageInput/qrTest7.bmp\n\n" +
                $"Example: Lire le qr code Hello\nOutput: read-qrcode|../../../ImageInput/Hello.bmp\n\n" +
                $"Example: Faire une rotation de l'image Patate de 36 degrés\nOutput: rotate-image|../../../ImageInput/Patate.bmp|degree=36\n\n" +
                $"Example: Agrandir l'image Bonjour d'un facteur de 2.5\nOutput: maximize|../../../ImageInput/Bonjour.bmp|factor=2.5\n\n" +
                $"Example: Agrandir l'image Bonjour d'un facteur de 2,5\nOutput: maximize|../../../ImageInput/Bonjour.bmp|factor=2.5\n\n" +
                $"Example: Rétrécir l'image Tomate d'un facteur de 3.7\nOutput: minimize|../../../ImageInput/Tomate.bmp|factor=3.7\n\n" +
                $"Example: Appliquer une matrice de convolution floue sur l'image Montre\nOutput: convolution-filter|../../../ImageInput/Montre.bmp|kernel=Flou\n\n" +
                $"Example: Appliquer une matrice de convolution de détection des contours sur l'image Pièce\nOutput: convolution-filter|../../../ImageInput/Pièce.bmp|kernel=Contour\n\n" +
                $"Example: Appliquer une matrice de convolution de détection des contours sur l'image test1 en utilisant sobel\nOutput: convolution-filter|../../../ImageInput/test1.bmp|kernel=Sobel\n\n" +
                $"Example: Transformer l'image Couteau en noir et blanc\nOutput: convert-to-grey|../../../ImageInput/Couteau.bmp\n\n" +
                $"Example: Genère un histogramme pour l'image Toto\nOutput: create-histogramme|../../../ImageInput/Toto.bmp\n\n" +
                $"Example: Cacher l'image Clavier dans l'image Ecran\nOutput: hide|../../../ImageInput/Clavier.bmp|../../../ImageInput/Ecran.bmp\n\n" +
                $"Example: Trouver une image cachée dans l'image Stylo\nOutput: found|../../../ImageInput/Stylo.bmp\n\n" +
                $"Example: Quitter le programme\nOutput: exit\n\n" +
                $"Example: Bonjour ca va?\nOutput: not valid command\n\n" +
                $"Example: N'importe quoi tant que c'est pas une commande?\nOutput: not valid command\n\n" +
                $"{input}:",
                temperature: 0, max_tokens: 200, presencePenalty: 0, frequencyPenalty: 0.2));
        }
    }
}