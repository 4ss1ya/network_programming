using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace ZipFileHandler
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string zipUrl = "https://github.com/mbaibatyr/SEP_221_NET/archive/refs/heads/master.zip";
            string zipFilePath = "C:\\temp\\master.zip";
            string extractPath = "C:\\temp\\SEP_221_NET";

            // Скачать ZIP файл
            using (HttpClient client = new HttpClient())
            {
                byte[] fileBytes = await client.GetByteArrayAsync(zipUrl);

                // Используем синхронный метод для записи файла
                File.WriteAllBytes(zipFilePath, fileBytes);
                Console.WriteLine($"Архив сохранен по пути: {zipFilePath}");
            }

            // Извлечь архив в папку
            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
            Console.WriteLine($"Архив извлечен в папку: {extractPath}");

            // Найти и отобразить содержимое файла .gitignore
            string gitignoreFilePath = Path.Combine(extractPath, "SEP_221_NET-master", ".gitignore");
            if (File.Exists(gitignoreFilePath))
            {
                // синхронный метод для чтения файла
                string gitignoreContent = File.ReadAllText(gitignoreFilePath);
                Console.WriteLine("Содержимое файла .gitignore:");
                Console.WriteLine(gitignoreContent);
            }
            else
            {
                Console.WriteLine("Файл .gitignore не найден.");
            }

            // Удалить архивный файл
            File.Delete(zipFilePath);
            Console.WriteLine("Архивный файл удален.");

            // Удалить извлеченную папку
            Directory.Delete(extractPath, true);
            Console.WriteLine("Извлеченная папка удалена.");
        }
    }
}
