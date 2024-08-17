using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Renci.SshNet;

class Program
{
    static async Task Main(string[] args)
    {
        string fileUrl = "https://beeline.kz/binaries/content/assets/public_offer/public_offer_ru.pdf";
        string localFilePath = "C:\\temp\\public_offer_ru.pdf";
        string ftpUrl = "ftp://ftp.dlptest.com/public_offer_ru.pdf";
        string ftpUsername = "dlpuser";
        string ftpPassword = "rNrKYTX9g7z3RgJRmxWuGHbeu";
        string sftpDirectory = "/pub/incoming/my_directory"; // Создаем свою папку
        string sftpFilePath = $"{sftpDirectory}/public_offer_ru.pdf";
        string sftpHost = "test.rebex.net";
        string sftpUsername = "demo";
        string sftpPassword = "password";

        // Сохранение файла на диск
        using (HttpClient client = new HttpClient())
        {
            byte[] fileBytes = await client.GetByteArrayAsync(fileUrl);

            // Асинхронная запись файла
            using (FileStream fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await fs.WriteAsync(fileBytes, 0, fileBytes.Length);
            }

            Console.WriteLine($"Файл сохранен на диск по пути: {localFilePath}");
        }

        // Отправка файла на FTP сервер без сохранения на диск
        using (HttpClient client = new HttpClient())
        using (Stream fileStream = await client.GetStreamAsync(fileUrl))
        {
            UploadToFTP(ftpUrl, ftpUsername, ftpPassword, fileStream);
            Console.WriteLine("Файл отправлен на FTP сервер.");
        }

        // Отправка файла на SFTP сервер в свою папку без сохранения на диск
        using (HttpClient client = new HttpClient())
        using (Stream fileStream = await client.GetStreamAsync(fileUrl))
        {
            UploadToSFTP(sftpHost, sftpUsername, sftpPassword, sftpDirectory, sftpFilePath, fileStream);
            Console.WriteLine("Файл отправлен на SFTP сервер в свою папку.");
        }
    }

    public static void UploadToFTP(string ftpUrl, string username, string password, Stream fileStream)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
        request.Method = WebRequestMethods.Ftp.UploadFile;
        request.Credentials = new NetworkCredential(username, password);
        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = false;

        using (Stream requestStream = request.GetRequestStream())
        {
            fileStream.CopyTo(requestStream);
        }

        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
            Console.WriteLine($"Загрузка завершена, статус: {response.StatusDescription}");
        }
    }

    public static void UploadToSFTP(string host, string username, string password, string remoteDirectory, string remoteFilePath, Stream fileStream)
    {
        using (var sftp = new SftpClient(host, username, password))
        {
            sftp.Connect();

            // Создаем директорию, если ее не существует
            if (!sftp.Exists(remoteDirectory))
            {
                sftp.CreateDirectory(remoteDirectory);
                Console.WriteLine($"Директория {remoteDirectory} создана на SFTP сервере.");
            }

            // Загружаем файл в созданную директорию
            sftp.UploadFile(fileStream, remoteFilePath);
            Console.WriteLine($"Файл загружен в директорию {remoteDirectory} на SFTP сервере.");

            sftp.Disconnect();
        }
    }
}
