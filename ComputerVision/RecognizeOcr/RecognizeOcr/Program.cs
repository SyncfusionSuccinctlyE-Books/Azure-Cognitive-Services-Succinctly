using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace RecognizeOcr
{
    class Program
    {
        const string API_key = "<< key goes here >>";
        const string API_location = "https://computervisionsuccinctly.cognitiveservices.azure.com/";

        static void Main(string[] args)
        {
            string imgToAnalyze = @"C:\Projects\Azure Cognitive Services Succinctly\" +
                @"Code\ComputerVision\RecognizeOcr\RecognizeOcr\receipt.jpg";

            TextExtractionCore(imgToAnalyze).Wait();

            Console.ReadLine();
        }

        public static ComputerVisionClient Authenticate(string key, string endpoint)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };

            return client;
        }

        public static List<string> GetWords(OcrLine line)
        {
            List<string> words = new List<string>();

            foreach (OcrWord w in line.Words)
                words.Add(w.Text);

            return words;
        }

        public static string GetLineAsString(OcrLine line)
        {
            List<string> words = GetWords(line);
            return words.Count > 0 ? string.Join(" ", words) : string.Empty;
        }

        public static async Task TextExtractionCore(string fname)
        {
            List<string> strList = new List<string>();

            using (Stream stream = File.OpenRead(fname))
            {
                ComputerVisionClient client = Authenticate(API_key, API_location);
                OcrResult ocrRes = await client.RecognizePrintedTextInStreamAsync(true, stream);

                foreach (var localRegion in ocrRes.Regions)
                    foreach (var line in localRegion.Lines)
                        strList.Add(GetLineAsString(line));

                Console.WriteLine("Date: " + 
                    GetDate(strList.ToArray()));
                Console.WriteLine("Highest amount: " + 
                    HighestAmount(strList.ToArray()));
            }
        }

        public static string ParseDate(string str)
        {
            string result = string.Empty;
            string[] formats = new string[]
                { "dd MMM yy h:mm", "dd MMM yy hh:mm" };

            foreach (string fmt in formats)
            {
                try
                {
                    str = str.Replace("'", "");

                    if (DateTime.TryParseExact(str, fmt, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime dateTime))
                    {
                        result = str;
                        break;
                    }
                }
                catch { }
            }

            return result;
        }

        public static string GetDate(string[] res)
        {
            string result = string.Empty;

            foreach (string l in res)
            {
                result = ParseDate(l);
                if (result != string.Empty) break;
            }

            return result;
        }

        public static string HighestAmount(string[] res)
        {
            string result = string.Empty;
            float highest = 0;

            Regex r = new Regex(@"[0-9]+\.[0-9]+");

            foreach (string l in res)
            {
                Match m = r.Match(l);

                if (m != null && m.Value != string.Empty &&
                    Convert.ToDouble(m.Value) > highest)
                    result = m.Value;
            }

            return result;
        }
    }
}