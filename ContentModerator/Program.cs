using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

namespace HttpClientDemo
{
    static class Program
    {
        // Azure Content Moderator
        private const string cEndpoint = "https://eastus.api.cognitive.microsoft.com/contentmoderator/";
        private const string cModerate = "moderate/v1.0/";
        private const string cOcpApimSubscriptionKey = "Ocp-Apim-Subscription-Key";
        private const string cSubscriptionKey = "<< Your key goes here >>"; // Change this!!

        // Image API
        private const string cImageApi = "ProcessImage/";
        // Text API
        private const string cTextApi = "ProcessText/";

        // Files to Test
        private const string cPath = @"C:\Projects\Azure Cognitive Services Succinctly\Code\Accessing the API";
        private static string cStrImage1 = $@"{cPath}\naked.jpg"; // Change this!!
        private static string cStrText1 = $@"{cPath}\test.txt"; // Change this!!

        static void Main()
        {
            ProcessRequest(cStrImage1, "image/jpeg", QryStrEvaluateImage(false));
            ProcessRequest(cStrText1, "text/plain", QryStrScreenText(false, true, "", true, ""));
            Console.ReadLine();
        }

        public static void ProcessRequest(string image, string contentType, string uri)
        {
            Task.Run(async () => {
                string res = await MakeRequest(image, contentType, uri);

                Console.WriteLine("\nResponse:\n");
                Console.WriteLine(JsonPrettyPrint(res));
            });
        }

        public static async Task<string> MakeRequest(string image, string contentType, string uri)
        {
            string contentString = string.Empty;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add(cOcpApimSubscriptionKey, cSubscriptionKey);

            HttpResponseMessage response = null;

            if (File.Exists(image) && uri != string.Empty && contentType != string.Empty)
            {
                byte[] byteData = GetAsByteArray(image); // This is important 
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    response = await client.PostAsync(uri, content);
                    contentString = await response.Content.ReadAsStringAsync(); // This is important
                }
            }

            return contentString;
        }

        public static byte[] GetAsByteArray(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        // Specific to the Image API
        public static string QryStrEvaluateImage(bool cacheImage)
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["CacheImage"] = cacheImage.ToString();

            return cEndpoint + cModerate + cImageApi + "Evaluate?" + queryString.ToString().ToLower();
        }

        // Specific to the Text API
        public static string QryStrScreenText(bool autoCorrect, bool pii, string listId, bool classify, string language)
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["autocorrect"] = autoCorrect.ToString().ToLower();
            queryString["PII"] = pii.ToString().ToLower();
            if (listId != string.Empty) queryString["listId"] = listId;
            queryString["classify"] = classify.ToString().ToLower();
            if (language != string.Empty) queryString["language"] = language;

            return cEndpoint + cModerate + cTextApi + "Screen?" + queryString.ToString();
        }

        public static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}