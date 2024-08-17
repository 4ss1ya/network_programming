using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

class FileServer
{
    private static string saveDirectory = "C:\\temp\\ReceivedFiles";

    static void Main(string[] args)
    {
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Сервер запущен и ожидает подключений...");

        while (true)
        {
            using (TcpClient client = server.AcceptTcpClient())
            using (NetworkStream networkStream = client.GetStream())
            {
                Console.WriteLine("Клиент подключен.");

                for (int i = 1; i <= 4; i++)
                {
                    string fileName = $"sample{i}.txt";
                    string filePath = Path.Combine(saveDirectory, fileName);

                    byte[] fileSizeBytes = new byte[8];
                    networkStream.Read(fileSizeBytes, 0, fileSizeBytes.Length);
                    long fileSize = BitConverter.ToInt64(fileSizeBytes, 0);

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[4096];
                        long bytesReceived = 0;
                        int bytesRead;
                        while (bytesReceived < fileSize && (bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                            bytesReceived += bytesRead;
                        }
                    }
                    Console.WriteLine($"Файл {fileName} сохранен.");
                }

                byte[] statusMessage = System.Text.Encoding.UTF8.GetBytes("Файлы успешно сохранены.");
                networkStream.Write(statusMessage, 0, statusMessage.Length);
                networkStream.Flush();

                Console.WriteLine("Статус отправлен клиенту.");
            }
        }
    }
}
