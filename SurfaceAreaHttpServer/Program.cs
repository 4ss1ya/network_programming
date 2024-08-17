using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Threading;

namespace SurfaceAreaHttpServer
{
    internal class Program
    {
        static Thread threadListener;

        static void Main(string[] args)
        {
            threadListener = new Thread(new ParameterizedThreadStart(StartServer));
            threadListener.Start("http://localhost:12345/");
        }

        public static void StartServer(object prefix)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(prefix.ToString());
            listener.Start();
            Console.WriteLine("HTTP Server started...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();

                HttpListenerRequest request = context.Request;
                string responseString;

                if (request.HttpMethod == "POST")
                {
                    string requestBody;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        requestBody = reader.ReadToEnd();
                    }

                    var parsedForm = ParseQueryString(requestBody);
                    if (double.TryParse(parsedForm["length"], out double length) &&
                        double.TryParse(parsedForm["width"], out double width) &&
                        double.TryParse(parsedForm["height"], out double height))
                    {
                        double surfaceArea = 2 * (length * width + width * height + height * length);
                        responseString = GenerateResponse(surfaceArea);
                    }
                    else
                    {
                        responseString = GenerateErrorResponse("Invalid input! Please enter valid numbers.");
                    }
                }
                else
                {
                    responseString = GenerateForm();
                }

                HttpListenerResponse response = context.Response;
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }

        private static NameValueCollection ParseQueryString(string query)
        {
            NameValueCollection collection = new NameValueCollection();
            string[] pairs = query.Split('&');
            foreach (string pair in pairs)
            {
                string[] kv = pair.Split('=');
                if (kv.Length == 2)
                {
                    collection.Add(WebUtility.UrlDecode(kv[0]), WebUtility.UrlDecode(kv[1]));
                }
            }
            return collection;
        }

        private static string GenerateForm()
        {
            string template = File.ReadAllText("index.html");
            return template.Replace("{{RESULT_SECTION}}", string.Empty);
        }

        private static string GenerateResponse(double surfaceArea)
        {
            string resultSection = $"<h2>Surface Area: {surfaceArea} square units</h2>";
            string template = File.ReadAllText("index.html");
            return template.Replace("{{RESULT_SECTION}}", resultSection);
        }

        private static string GenerateErrorResponse(string errorMessage)
        {
            string errorSection = $"<h2>Error: {errorMessage}</h2>";
            string template = File.ReadAllText("index.html");
            return template.Replace("{{RESULT_SECTION}}", errorSection);
        }
    }
}
