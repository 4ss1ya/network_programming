using System;
using System.IO;
using System.IO.Compression;
using System.Net;


class FTPFileTransfer
{
    static void Main(string[] args)
    {
        string[] filePaths = new string[]
        {
            "C:\\temp\\Exam\\sample1.txt",
            "C:\\temp\\Exam\\sample2.txt",
            "C:\\temp\\Exam\\sample3.txt",
            "C:\\temp\\Exam\\sample4.txt"
        };

        string zipFilePath = "C:\\temp\\Exam\\files.zip";

        // Заархивируем файлы в один ZIP архив
        using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (var filePath in filePaths)
                {
                    archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                }
            }
        }
        Console.WriteLine($"Архив {zipFilePath} создан.");

        // Загрузка архива на FTP сервер
        string ftpUrl = "ftp://ftp.dlptest.com/files.zip";
        string ftpUsername = "dlpuser";
        string ftpPassword = "rNrKYTX9g7z3RgJRmxWuGHbeu";

        UploadFileToFTP(ftpUrl, zipFilePath, ftpUsername, ftpPassword);
        Console.WriteLine("Файл загружен на FTP сервер.");

        // Загрузка архива с FTP сервера
        string downloadedZipFilePath = "C:\\temp\\Exam\\downloaded_files.zip";
        DownloadFileFromFTP(ftpUrl, downloadedZipFilePath, ftpUsername, ftpPassword);
        Console.WriteLine("Файл загружен с FTP сервера.");

        // Разархивирование загруженного файла
        string extractPath = "C:\\temp\\Exam\\ExtractedFiles";
        ZipFile.ExtractToDirectory(downloadedZipFilePath, extractPath);
        Console.WriteLine("Файлы разархивированы.");
    }

    public static void UploadFileToFTP(string ftpUrl, string filePath, string username, string password)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
        request.Method = WebRequestMethods.Ftp.UploadFile;
        request.Credentials = new NetworkCredential(username, password);
        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = false;
        request.ContentLength = fileInfo.Length;

        using (FileStream fs = fileInfo.OpenRead())
        {
            byte[] buffer = new byte[2048];
            int bytesRead = 0;
            using (Stream requestStream = request.GetRequestStream())
            {
                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }
            }
        }

        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
        Console.WriteLine($"Загрузка завершена, статус: {response.StatusDescription}");
        response.Close();
    }

    public static void DownloadFileFromFTP(string ftpUrl, string filePath, string username, string password)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
        request.Method = WebRequestMethods.Ftp.DownloadFile;
        request.Credentials = new NetworkCredential(username, password);
        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = false;

        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        using (Stream responseStream = response.GetResponseStream())
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            byte[] buffer = new byte[2048];
            int bytesRead;
            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
        }
        Console.WriteLine("Скачивание завершено.");
    }
}
