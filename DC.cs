using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PEngineModule.Logs
{
    public class DC
    {
        public static async Task DockerQuery()
        {
            const string LIST_REQUEST =
            "GET /v1.40/containers/json HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "Accept: */*\r\n" +
            "\r\n";

            const string LOG_REQUEST =
            "GET /v1.40/containers/acd30b303fa6c7b8ec47e06fc36479b1513eeb3a13f75295b85b399a4e435354/logs?timestamps=true&stdout=true&stderr=true HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "Accept: */*\r\n" +
            "\r\n";

            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            socket.ReceiveTimeout = 60 * 1000; // 60 seconds
            var endpoint = new UnixDomainSocketEndPoint("/var/run/docker.sock");
            socket.Connect(endpoint);
            byte[] requestBytes = Encoding.UTF8.GetBytes(LOG_REQUEST);
            socket.Send(requestBytes);
            byte[] recvBytes = new byte[socket.ReceiveBufferSize];
            int numBytes = socket.Receive(recvBytes, socket.ReceiveBufferSize, SocketFlags.None);
            socket.Disconnect(false);
            Console.WriteLine(Encoding.UTF8.GetString(recvBytes));

            // using (var resultStream = new MemoryStream())
            // {
            //     const int CHUNK_SIZE = 10 * 1024; // 2KB, could be anything that fits your needs
            //     byte[] buffer = new byte[CHUNK_SIZE];
            //     int bytesReceived;

            //     while ((bytesReceived = socket.Receive(buffer, buffer.Length, SocketFlags.None)) > 0)
            //     {
            //         Console.WriteLine("s");
            //         byte[] actual = new byte[bytesReceived];
            //         Console.WriteLine(bytesReceived);
            //         Buffer.BlockCopy(buffer, 0, actual, 0, bytesReceived);
            //         resultStream.Write(actual, 0, actual.Length);
            //         Console.WriteLine("e");
            //     }
            //     Console.WriteLine("done");
            //     Console.WriteLine(resultStream.ToString());
            // }
        }
    }
}