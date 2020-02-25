using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PEngineModule.Logs
{
    public class DockerClient
    {
        public static async Task DockerQuery()
        {
            const string LOG_REQUEST =
            "GET /v1.40/containers/b791050acf2870933238cf1cb49d3d071e5b86a52c151357b0cf8444aaf7e6ed/logs?timestamps=true&stdout=true&stderr=true HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "Accept: */*\r\n" +
            "\r\n";

            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            socket.ReceiveTimeout = 60 * 1000; // 60 seconds

            var endpoint = new UnixDomainSocketEndPoint("/var/run/docker.sock");
            socket.Connect(endpoint);

            byte[] requestBytes = Encoding.UTF8.GetBytes(LOG_REQUEST);
            await socket.SendAsync(requestBytes, SocketFlags.None);

            // byte[] buffer = new byte[1024];
            // int numBytes = await socket.ReceiveAsync(buffer, SocketFlags.None);
            // Console.WriteLine($"recieve {Encoding.UTF8.GetString(buffer)}");

            byte[] endb = new byte[] { 3, 10 };

            using (var resultStream = new MemoryStream())
            {
                const int CHUNK_SIZE = 1 * 1024;
                byte[] buffer = new byte[CHUNK_SIZE];
                int bytesReceived;

                while ((bytesReceived = await socket.ReceiveAsync(buffer, SocketFlags.None)) > 0)
                {
                    Console.WriteLine("s");

                    byte[] actual = new byte[bytesReceived];
                    Buffer.BlockCopy(buffer, 0, actual, 0, bytesReceived);
                    string str = Encoding.UTF8.GetString(actual);
                    Console.WriteLine(str);

                    // read buffer until it reads 13 followed by 10 (CRLF)
                    if (actual[bytesReceived - 2].Equals(13) && actual[bytesReceived - 1].Equals(10))
                    {
                        Console.WriteLine("true");
                        break;
                    }
                    await resultStream.WriteAsync(actual, 0, actual.Length);

                    Console.WriteLine("e");
                }
                Console.WriteLine("done");
            }

            socket.Disconnect(false);
            socket.Close();
        }

        public static async Task ListContainer()
        {

            const string LIST_REQUEST =
            "GET /v1.40/containers/json HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "Accept: */*\r\n" +
            "\r\n";


            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            socket.ReceiveTimeout = 60 * 1000; // 60 seconds

            var endpoint = new UnixDomainSocketEndPoint("/var/run/docker.sock");
            socket.Connect(endpoint);

            byte[] requestBytes = Encoding.UTF8.GetBytes(LIST_REQUEST);
            await socket.SendAsync(requestBytes, SocketFlags.None);


            using (var resultStream = new MemoryStream())
            {
                const int CHUNK_SIZE = 1 * 1024;
                byte[] buffer = new byte[CHUNK_SIZE];
                int bytesReceived;

                while ((bytesReceived = await socket.ReceiveAsync(buffer, SocketFlags.None)) > 0)
                {
                    Console.WriteLine("s");

                    byte[] actual = new byte[bytesReceived];
                    Buffer.BlockCopy(buffer, 0, actual, 0, bytesReceived);
                    string str = Encoding.UTF8.GetString(actual);
                    Console.WriteLine(str);
                    if (str.EndsWith("\r\n\r\n"))
                    {
                        Console.WriteLine("true");

                    }

                   
                }
                Console.WriteLine("done");
            }
        }


    }
}