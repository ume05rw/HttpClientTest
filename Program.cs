using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientTest
{
    public class Program
    {
        // Set your Mopidy Address
        private const string MopidyRpcUrl = "http://192.168.254.251:6680/mopidy/rpc";

        private static int _count = 0;

        public static void Main(string[] args)
        {
            var withUsing = false;
            if (args.Contains("--withoutusing"))
                withUsing = false;
            else if (args.Contains("--withusing"))
                withUsing = true;

            Console.WriteLine("Start Socket Overflow Test: " + ((withUsing) ? "with Using" : "without Using"));

            while (true)
            {
                try
                {
                    Program._count++;

                    var sendJson = $"{{\"jsonrpc\": \"2.0\", \"method\":\"core.playback.get_state\", \"id\": {Program._count}}}";
                    var content = new StringContent(sendJson, Encoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.WriteLine($"Try {((withUsing) ? "with Using" : "without Using")} Count: " + Program._count.ToString());
                    Console.WriteLine("Send Message: " + sendJson);

                    var result = (withUsing)
                        ? Program.GetResultWithUsing(content)
                        : Program.GetResultWithoutUsing(content);

                    Console.WriteLine("Result: " + result);

                    Task.Delay(200).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception!");
                    Program.DumpException(ex);

                    Task.Delay(1000).GetAwaiter().GetResult();
                }
            }
        }

        private static string GetResultWithUsing(StringContent content)
        {
            using (var client = Program.GetHttpClient())
            {
                var message = client.PostAsync(Program.MopidyRpcUrl, content).GetAwaiter().GetResult();
                var result = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                return result;
            }
        }

        private static string GetResultWithoutUsing(StringContent content)
        {
            var client = Program.GetHttpClient();

            var message = client.PostAsync(Program.MopidyRpcUrl, content).GetAwaiter().GetResult();
            var result = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return result;
        }

        private static HttpClient GetHttpClient()
        {
            var result = new HttpClient();
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            result.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            result.Timeout = TimeSpan.FromMilliseconds(1000);

            return result;
        }

        private static void DumpException(Exception ex)
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
                Program.DumpException(ex.InnerException);
        }
    }
}
