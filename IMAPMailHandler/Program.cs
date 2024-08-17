using System;
using MailKit.Net.Imap;
using MailKit;
using MailKit.Search;

namespace IMAPMailHandler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var client = new ImapClient())
                {
                    client.Connect("imap.mail.ru", 993, true);
                    client.Authenticate("assiya.bagitzhankyzy@mail.ru", "ваш_пароль_для_внешних_прилож");

                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadWrite);

                    // Вывод всех тем писем для диагностики
                    var messages = inbox.Fetch(0, -1, MessageSummaryItems.Envelope);
                    foreach (var message in messages)
                    {
                        Console.WriteLine("Тема: " + message.Envelope.Subject);
                    }

                    // Ищем письмо по теме 
                    var results = inbox.Search(SearchQuery.SubjectContains("Message send"));

                    if (results.Count > 0)
                    {
                        var uniqueId = results[0];
                        inbox.AddFlags(uniqueId, MessageFlags.Seen, true);

                        // Получаем доступ к целевой папке 
                        var targetFolder = client.GetFolder("Archive");

                        if (targetFolder == null)
                        {
                            Console.WriteLine("Целевая папка не найдена.");
                            return;
                        }

                        // Перемещаем письмо в целевую папку
                        inbox.MoveTo(uniqueId, targetFolder);

                        Console.WriteLine("Письмо найдено, помечено как прочитанное и перемещено в папку 'Archive'.");
                    }
                    else
                    {
                        Console.WriteLine("Письмо с указанной темой не найдено.");
                    }

                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка: " + ex.Message);
            }
        }
    }
}
