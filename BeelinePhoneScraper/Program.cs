using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Linq;

namespace BeelinePhoneScraper
{
    internal class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            string url = "https://beeline.kz/ru";

            // HttpClient для загрузки страницы
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Загружаем HTML-страницу
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string htmlContent = await response.Content.ReadAsStringAsync();

                    // Парсим HTML-контент с помощью HtmlAgilityPack
                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(htmlContent);

                    // Ищем все номера телефонов на странице
                    var phoneNodes = htmlDocument.DocumentNode.SelectNodes("//footer//a[contains(@href, 'tel')]");

                    if (phoneNodes != null && phoneNodes.Any())
                    {
                        Console.WriteLine("Найдены телефоны:");
                        foreach (var node in phoneNodes)
                        {
                            // Извлекаем номер телефона 
                            string phoneNumber = node.GetAttributeValue("href", "").Replace("tel:", "");
                            Console.WriteLine(phoneNumber);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Номера телефонов не найдены.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла ошибка: " + ex.Message);
                }
            }
        }
    }
}
