using System;
using System.Net.Mail;

namespace MySMTPApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string zipFilePath = @"C:\temp\Exam\files.zip";

            if (!System.IO.File.Exists(zipFilePath))
            {
                Console.WriteLine($"Файл по пути '{zipFilePath}' не найден.");
                return;
            }

            SendMail.Send(zipFilePath);
        }
    }

    public class SendMail
    {
        public static void Send(string zipFilePath)
        {
            try
            {
                // Настройка сообщения
                MailMessage mail = new MailMessage();
                mail.IsBodyHtml = true;
                mail.From = new MailAddress("assiya.bagitzhankyzy@mail.ru");
                mail.To.Add("assiya.bagitzhankyzy@mail.ru"); 
                mail.Subject = "sample";
                mail.Body = "body sample";

                // Добавляем вложение - архив
                Attachment attachment = new Attachment(zipFilePath);
                mail.Attachments.Add(attachment);

                // Настройка SMTP-клиента
                using (SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential("assiya.bagitzhankyzy@mail.ru", "ваш_пароль_для_внешних_прилож");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }

                Console.WriteLine("Письмо успешно отправлено.");
            }
            catch (Exception err)
            {
                Console.WriteLine("Ошибка: " + err.Message);
            }
        }
    }
}
