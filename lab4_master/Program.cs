using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;



namespace lab4_master_serv
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Socket listenSocket = ConfigureSocket();

                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    //Console.WriteLine("Установленно соединение");
                    CreateProcess(handler);
                    //Console.WriteLine("Соединение закрыто");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        static public Socket ConfigureSocket()
        {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ServerSettings.ServerIp), ServerSettings.ServerPort);

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // связываем сокет с локальной точкой, по которой будем принимать данные
            listenSocket.Bind(ipPoint);

            // начинаем прослушивание
            listenSocket.Listen(ServerSettings.RequestQueueLength);

            return listenSocket;

        }
        public static string Serialize(SocketInformation si)
        {
            return $"{si.Options}:{Convert.ToBase64String(si.ProtocolInformation)}";
        }

        static public void CreateProcess(Socket requestHandler)
        {
            var guid = Guid.NewGuid();

            using (NamedPipeServerStream pipeServer =
            new NamedPipeServerStream(guid.ToString(), PipeDirection.Out))
            {
                Console.WriteLine("Именованый канал создан.");

                Process myProcess = new Process();
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.FileName = "E:\\ВМСиС\\7 семестр\\СПОЛКС\\lab1\\Repository\\spolks_lab1_serv_v2\\spolks_lab1_serv_v2\\bin\\Debug\\spolks_lab1_serv_v2.exe";
                myProcess.StartInfo.Arguments = guid.ToString();
                myProcess.Start();

                var socketInfo = requestHandler.DuplicateAndClose(myProcess.Id);
                var serializedSocket = Serialize(socketInfo);

                Console.Write("Ожидание соединения процесса по кананлу...");
                pipeServer.WaitForConnection();

                using (StreamWriter pipeWriter = new StreamWriter(pipeServer))
                {
                    pipeWriter.AutoFlush = true;
                    pipeWriter.WriteLine(serializedSocket);
                }

                Console.WriteLine("Канал с новым процессом подключен");
            }

        }

    }
}
