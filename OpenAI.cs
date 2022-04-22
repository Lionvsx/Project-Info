using System;
using System.Threading.Tasks;
using OpenAI_API;

namespace Project_Info
{
    public class OpenAI
    {
        public OpenAIAPI Api { get; set; }

        private readonly string _apiKey = "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";

        public OpenAI()
        {
            Api = new OpenAIAPI(_apiKey, "text-davinci-002");
        }

        public async void SteamInConsole(string input)
        {
            await Api.Completions.StreamCompletionAsync(
                new CompletionRequest($"This is a conversation with an AI assistant. The assistant is friendly, creative and very helpful:\nHuman: {input}\nAI:", 200, 0.4, presencePenalty: 0.1, frequencyPenalty: 0.1, stopSequences: "Human:"),
                res => Console.Write(res.ToString()));
            Console.WriteLine();
        }
        
        

        public async Task<CompletionResult> Completion(string input)
        {
            return await Api.Completions.CreateCompletionAsync(new CompletionRequest(input, temperature: 0.1, max_tokens: 1000, presencePenalty: 0.1, frequencyPenalty: 0.1));
        }

        public async void TextToCommand(string input)
        {
            
        }
    }
}