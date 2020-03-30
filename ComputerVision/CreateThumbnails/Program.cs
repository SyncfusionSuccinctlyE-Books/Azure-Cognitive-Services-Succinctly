using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace Demo
{
    class Program
    {
        const string API_key = "<< key goes here >>";
        const string API_location = "https://computervisionsuccinctly.cognitiveservices.azure.com/";

        static void Main(string[] args)
        {
            string imgToAnalyze = @"C:\Projects\Azure Cognitive Services Succinctly\" +
                @"Code\ComputerVision\CreateThumbnails\Test.jpg";

            SmartThumbnail(imgToAnalyze, 80, 80, true);

            Console.ReadKey();
        }

        public static ComputerVisionClient Authenticate(string key, string endpoint)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };

            return client;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static void SmartThumbnail(string fname, int width, int height, bool smartCropping)
        {
            Task.Run(async () => {

                string imgname = Path.GetFileName(fname);
                Console.WriteLine($"Thumbnail for image: {imgname}");

                Stream thumbnail = await SmartThumbnailGeneration(fname, width, height, smartCropping);

                string thumbnailFullPath = string.Format("{0}\\thumbnail_{1:yyyy-MMM-dd_hh-mm-ss}.jpg",
                    Path.GetDirectoryName(fname), DateTime.Now);

                using (BinaryWriter bw = new BinaryWriter(new FileStream(thumbnailFullPath,
                    FileMode.OpenOrCreate, FileAccess.Write)))
                    bw.Write(ReadFully(thumbnail));

            }).Wait();
        }

        public static async Task<Stream> SmartThumbnailGeneration(string fname, int width, int height, bool smartCropping)
        {
            Stream thumbnail = null;
            ComputerVisionClient client = Authenticate(API_key, API_location);

            if (File.Exists(fname))
                using (Stream stream = File.OpenRead(fname))
                    thumbnail = await client.GenerateThumbnailInStreamAsync(width, height, stream, smartCropping);

            return thumbnail;
        }
    }
}
