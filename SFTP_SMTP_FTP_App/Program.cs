using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using Renci.SshNet;
using FluentFTP;

namespace SFTP_SMTP_FTP_App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string sftpHost = "test.rebex.net";
            string sftpUsername = "demo";
            string sftpPassword = "password";
            string sftpFilePath = "/pub/example/readme.txt"; 
            string archiveFileName = "readme.zip";

            // Скачивание файла с SFTP
            using (var sftp = new SftpClient(sftpHost, sftpUsername, sftpPassword))
            {
                sftp.Connect();

                using (var memoryStream = new MemoryStream())
                {
                    // Скачиваем файл в память
                    sftp.DownloadFile(sftpFilePath, memoryStream);
                    memoryStream.Position = 0;

                    // Архивируем файл "налету"
                    using (var archiveStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                        {
                            var zipEntry = archive.CreateEntry(Path.GetFileName(sftpFilePath));
                            using (var entryStream = zipEntry.Open())
                            {
                                memoryStream.CopyTo(entryStream);
                            }
                        }

                        // Устанавливаем позицию потока на начало
                        archiveStream.Position = 0;

                        //Отправляем по SMTP
                        SendEmailWithAttachment(archiveStream, archiveFileName);

                        //Загружаем по FTP
                        UploadToFTP(archiveStream, archiveFileName);
                    }
                }

                sftp.Disconnect();
            }
        }

        static void SendEmailWithAttachment(MemoryStream archiveStream, string fileName)
        {
            var smtpClient = new SmtpClient("smtp.mail.ru")
            {
                Port = 587,
                Credentials = new NetworkCredential("assiya.bagitzhankyzy@mail.ru", "ваш_пароль_для_внешних_прилож"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("assiya.bagitzhankyzy@mail.ru"),
                Subject = "Загруженный файл",
                Body = "Файл был успешно загружен и заархивирован.",
                IsBodyHtml = false,
            };
            mailMessage.To.Add("assiya.bagitzhankyzy@mail.ru");

            archiveStream.Position = 0; // Убедитесь, что поток установлен на начало
            mailMessage.Attachments.Add(new Attachment(archiveStream, fileName, "application/zip"));

            smtpClient.Send(mailMessage);

            Console.WriteLine("Письмо успешно отправлено.");
        }

        static void UploadToFTP(MemoryStream archiveStream, string fileName)
        {
            var ftpClient = new FtpClient("ftp.dlptest.com")
            {
                Credentials = new NetworkCredential("dlpuser", "rNrKYTX9g7z3RgJRmxWuGHbeu"),
            };

            ftpClient.Connect();

            archiveStream.Position = 0; // Убедитесь, что поток установлен на начало
            ftpClient.UploadStream(archiveStream, $"/{fileName}");

            Console.WriteLine("Файл успешно загружен на FTP сервер.");

            ftpClient.Disconnect();
        }
    }
}
