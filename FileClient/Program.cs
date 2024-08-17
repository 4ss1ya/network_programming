using System;
using System.IO;
using System.Net.Sockets;

class FileClient
{
    static void Main(string[] args)
    {
        string serverAddress = "127.0.0.1"; // Адрес сервера (локальный)
        int serverPort = 5000; // Порт сервера

        string[] filePaths = new string[]
        {
            "C:\\temp\\Exam\\sample1.txt",
            "C:\\temp\\Exam\\sample2.txt",
            "C:\\temp\\Exam\\sample3.txt",
            "C:\\temp\\Exam\\sample4.txt"
        };

        foreach (var path in filePaths)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Файл не найден: {path}");
                return;
            }
        }

        using (TcpClient client = new TcpClient(serverAddress, serverPort))
        using (NetworkStream networkStream = client.GetStream())
        {
            foreach (var filePath in filePaths)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);
                networkStream.Write(fileSizeBytes, 0, fileSizeBytes.Length);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        networkStream.Write(buffer, 0, bytesRead);
                    }
                }
                Console.WriteLine($"Файл {Path.GetFileName(filePath)} отправлен.");
            }

            byte[] endOfTransmissionMarker = System.Text.Encoding.UTF8.GetBytes("<EOF>");
            networkStream.Write(endOfTransmissionMarker, 0, endOfTransmissionMarker.Length);

            byte[] responseBuffer = new byte[4096];
            int responseBytesRead = networkStream.Read(responseBuffer, 0, responseBuffer.Length);
            string responseMessage = System.Text.Encoding.UTF8.GetString(responseBuffer, 0, responseBytesRead);
            Console.WriteLine($"Статус от сервера: {responseMessage}");
        }
    }
}
