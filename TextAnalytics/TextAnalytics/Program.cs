using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Rest;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;

namespace TextAnalytics
{
    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private const string cKeyLbl = "Ocp-Apim-Subscription-Key";
        private readonly string subscriptionKey;

        public ApiKeyServiceClientCredentials(string subscriptionKey)
        {
            this.subscriptionKey = subscriptionKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request != null)
            {
                request.Headers.Add(cKeyLbl, subscriptionKey);
                return base.ProcessHttpRequestAsync(request, cancellationToken);
            }
            else return null;
        }
    }

    class Program
    {
        private const string cKey = "<< Key goes here >>";
        private const string cEndpoint = 
            "https://textanalyticssuccinctly.cognitiveservices.azure.com/";

        private static TextAnalyticsClient InitApi(string key)
        {
            return new TextAnalyticsClient(new 
                ApiKeyServiceClientCredentials(key)) 
                { 
                    Endpoint = cEndpoint 
                };
        }

        private static MultiLanguageBatchInput GetMLBI(string[] items)
        {
            List<MultiLanguageInput> lst = new List<MultiLanguageInput>();

            foreach (string itm in items)
            {
                string[] p = itm.Split('|');
                lst.Add(new MultiLanguageInput(p[0], p[1], p[2]));
            }

            return new MultiLanguageBatchInput(lst);
        }

        private static LanguageBatchInput GetLBI(string[] items)
        {
            List<LanguageInput> lst = new List<LanguageInput>();

            for (int i = 0; i <= items.Length - 1; i++)
                lst.Add(new LanguageInput((i + 1).ToString(), items[i]));

            return new LanguageBatchInput(lst);
        }

        private static async Task RunSentiment(TextAnalyticsClient client, 
            MultiLanguageBatchInput docs)
        {
            var res = await client.SentimentBatchAsync(docs);

            foreach (var document in res.Documents)
                Console.WriteLine($"Document ID: {document.Id}, " +
                    $"Sentiment Score: {document.Score:0.00}");
        }

        private static async Task<string[]> GetDetectLanguage(TextAnalyticsClient client,
            LanguageBatchInput docs)
        {
            List<string> ls = new List<string>();

            var res = await client.DetectLanguageBatchAsync(docs);

            foreach (var document in res.Documents)
                ls.Add("|" + document.DetectedLanguages[0].Iso6391Name);

            return ls.ToArray();
        }

        private static async Task RunRecognizeEntities(TextAnalyticsClient client, 
            MultiLanguageBatchInput docs)
        {
            var res = await client.EntitiesBatchAsync(docs);

            foreach (var document in res.Documents)
            {
                Console.WriteLine($"Document ID: {document.Id} ");
                Console.WriteLine("\tEntities:");

                foreach (var entity in document.Entities)
                {
                    Console.WriteLine($"\t\t{entity.Name}");
                    Console.WriteLine($"\t\tType: {entity.Type ?? "N/A"}");
                    Console.WriteLine($"\t\tSubType: {entity.SubType ?? "N/A"}");

                    foreach (var match in entity.Matches)
                        Console.WriteLine($"\t\tScore: {match.EntityTypeScore:F3}");

                    Console.WriteLine($"\t");
                }
            }
        }

        private static async Task RunKeyPhrasesExtract(TextAnalyticsClient client, 
            MultiLanguageBatchInput docs)
        {
            var res = await client.KeyPhrasesBatchAsync(docs);

            foreach (var document in res.Documents)
            {
                Console.WriteLine($"Document ID: {document.Id} ");
                Console.WriteLine("\tKey phrases:");

                foreach (string keyphrase in document.KeyPhrases)
                    Console.WriteLine($"\t\t{keyphrase}");
            }
        }

        private static string[] MergeItems(string[] a1,  string[] a2)
        {
            List<string> r = new List<string>();

            if (a2 == null || a1.Length == a2.Length)
                for (int i = 0; i <= a1.Length - 1; i++)
                    r.Add($"{(i + 1).ToString()}|{a1[i]}{a2[i]}");

            return r.ToArray();
        }

        private static async Task ProcessSentiment(string[] items)
        {
            string[] langs = await GetDetectLanguage(InitApi(cKey), GetLBI(items));

            RunSentiment(InitApi(cKey), GetMLBI(MergeItems(items, langs))).Wait();
            Console.WriteLine($"\t");
        }

        private static async Task ProcessRecognizeEntities(string[] items)
        {
            string[] langs = await GetDetectLanguage(InitApi(cKey), GetLBI(items));
 
            RunRecognizeEntities(InitApi(cKey), GetMLBI(MergeItems(items, langs))).Wait();
            Console.WriteLine($"\t");
        }

        private static async Task ProcessKeyPhrasesExtract(string[] items)
        {
            string[] langs = await GetDetectLanguage(InitApi(cKey), GetLBI(items));

            RunKeyPhrasesExtract(InitApi(cKey), GetMLBI(MergeItems(items, langs))).Wait();
            Console.WriteLine($"\t");
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string[] items = new string[] {
                "Microsoft was founded by Bill Gates" +
                " and Paul Allen on April 4, 1975, " +
                    "to develop and sell BASIC " +
                    "interpreters for the Altair 8800",

                "La sede principal de Microsoft " +
                "se encuentra en la ciudad de " +
                    "Redmond, a 21 kilómetros " +
                    "de Seattle"
            };

            ProcessSentiment(items).Wait();
            ProcessRecognizeEntities(items).Wait();
            ProcessKeyPhrasesExtract(items).Wait();

            Console.ReadLine();
        }
    }
}
