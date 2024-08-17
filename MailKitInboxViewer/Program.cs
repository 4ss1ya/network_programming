using System;
using MailKit.Net.Imap;
using MailKit;
using MailKit.Search;
using System.Linq;

namespace MailKitInboxViewer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Подключение к Mail.ru
            using (var client = new ImapClient())
            {
                try
                {
                    client.Connect("imap.mail.ru", 993, true);
                    client.Authenticate("assiya.bagitzhankyzy@mail.ru", "ваш_пароль_для_внешних_прилож");

                    // Открываем папку "Входящие"
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    // Получаем все сообщения, сортируем по дате и берем нужное количество
                    var messages = inbox.Fetch(0, -1, MessageSummaryItems.Envelope)
                                        .OrderBy(ms => ms.Envelope.Date)
                                        .Take(100);  // число можно поменять, чтобы выбрать нужное количество писем

                    Console.WriteLine("Темы писем в папке 'Входящие':");
                    foreach (var message in messages)
                    {
                        Console.WriteLine(message.Envelope.Subject);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла ошибка: " + ex.Message);
                }
                finally
                {
                    client.Disconnect(true);
                }
            }
        }
    }
}
