using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Speech
{
    class Program
    {
        private const string cKey = "<< key goes here >>";
        private const string cRegion = "eastus";

        public static async Task TextToSpeechSynthesisAsync(string text)
        {
            var config = SpeechConfig.FromSubscription(cKey, cRegion);

            using (var synthesizer = new SpeechSynthesizer(config))
                await Synthesize(text, synthesizer);
        }

        public static async Task TextToAudioFileAsync(string text, string fn)
        {
            var config = SpeechConfig.FromSubscription(cKey, cRegion);

            using (FileStream f = new FileStream(fn, FileMode.Create))
                using (BinaryWriter wr = new BinaryWriter(f))
                    wr.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));

            using (var fo = AudioConfig.FromWavFileOutput(fn))
                using (var synthesizer = new SpeechSynthesizer(config, fo))
                    await Synthesize(text, synthesizer);
        }

        private static async Task Synthesize(string text, SpeechSynthesizer synthesizer)
        {
            using (var r = await synthesizer.SpeakTextAsync(text))
            {
                if (r.Reason == ResultReason.SynthesizingAudioCompleted)
                    Console.WriteLine($"Speech synthesized " +
                        $"to speaker for text [{text}]");
                else if (r.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(r);
                    Console.WriteLine($"CANCELED: " +
                        $"Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"Cancelled with " +
                            $"Error Code {cancellation.ErrorCode}");
                        Console.WriteLine($"Cancelled with " +
                            $"Error Details " +
                            $"[{cancellation.ErrorDetails}]");
                    }
                }
            }

            Console.WriteLine("Waiting to play " +
                "back to the audio...");
            Console.ReadKey();
        }

        public static async Task SpeechToTextAsync()
        {
            var config = SpeechConfig.FromSubscription(cKey, cRegion);

            using (var recognizer = new SpeechRecognizer(config))
                await Recognize(recognizer);
        }

        public static async Task AudioToTextAsync(string fn)
        {
            var config = SpeechConfig.FromSubscription(cKey, cRegion);

            using (var ai = AudioConfig.FromWavFileInput(fn))
                using (var recognizer = new SpeechRecognizer(config, ai))
                    await Recognize(recognizer);
        }

        public static async Task AudioToTextContinuousAsync(string fn)
        {
            var config = SpeechConfig.FromSubscription(cKey, cRegion);

            using (var ai = AudioConfig.FromWavFileInput(fn))
                using (var recognizer = new SpeechRecognizer(config, ai))
                    await RecognizeAll(recognizer);
        }

        private static async Task RecognizeAll(SpeechRecognizer recognizer)
        {
            var taskCompletetion = new TaskCompletionSource<int>();

            // Events.  
            recognizer.Recognizing += (sender, eventargs) =>
            {
                // Handle recognized intermediate result  
            };

            recognizer.Recognized += (sender, eventargs) =>
            {
                if (eventargs.Result.Reason == ResultReason.RecognizedSpeech)
                    Console.WriteLine($"Recognized: {eventargs.Result.Text}");
            };

            recognizer.Canceled += (sender, eventargs) =>
            {
                if (eventargs.Reason == CancellationReason.Error)
                    Console.WriteLine("Error reading the audio file.");

                if (eventargs.Reason == CancellationReason.EndOfStream)
                    Console.WriteLine("End of file.");

                taskCompletetion.TrySetResult(0);
            };

            recognizer.SessionStarted += (sender, eventargs) =>
            {
                //Started recognition session  
            };

            recognizer.SessionStopped += (sender, eventargs) =>
            {
                //Ended recognition session  
                taskCompletetion.TrySetResult(0);
            };

            // Starts continuous recognition.
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            // Waits for completion.  
            Task.WaitAny(new[] { taskCompletetion.Task });

            // Stops recognition.  
            await recognizer.StopContinuousRecognitionAsync();
        }

        private static async Task Recognize(SpeechRecognizer recognizer)
        {
            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
                Console.WriteLine($"Recognized: {result.Text}");
            else if (result.Reason == ResultReason.NoMatch)
                Console.WriteLine("Speech could not be recognized.");
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                Console.WriteLine($"Cancelled due to reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"Error code={cancellation.ErrorCode}");
                    Console.WriteLine($"Error details={cancellation.ErrorDetails}");
                    Console.WriteLine($"Did you update the subscription info?");
                }
            }
        }

        static void Main()
        {
            string txt = "Hey, how are you? " +
                "Are you going out now with Cathy?";

            string fn = @"C:\Projects\Azure Cognitive Services Succinctly\Code\Speech\hello.wav";

            //TextToSpeechSynthesisAsync(txt).Wait();
            //TextToAudioFileAsync(txt, fn).Wait();

            //SpeechToTextAsync().Wait();
            //AudioToTextAsync(fn).Wait();
            //AudioToTextContinuousAsync(fn).Wait();
            
            Console.ReadLine();
        }
    }
}