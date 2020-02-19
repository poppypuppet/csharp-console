using System;
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
            "GET /v1.40/containers/5c797229cb21909ae02d3686988d0a0a0357cd20ca3d51dcd8225cfd4e474528/logs?timestamps=true&stdout=true&stderr=true HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "Accept: */*\r\n" +
            "\r\n";

            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            socket.ReceiveTimeout = 60 * 1000; // 60 seconds

            var endpoint = new UnixDomainSocketEndPoint("/var/run/docker.sock");
            socket.Connect(endpoint);

            byte[] requestBytes = Encoding.UTF8.GetBytes(LIST_REQUEST);
            await socket.SendAsync(requestBytes, SocketFlags.None);

            byte[] buffer = new byte[1024];
            int numBytes = await socket.ReceiveAsync(buffer, SocketFlags.None);

            Console.WriteLine($"recieve {Encoding.UTF8.GetString(buffer)}");

            // using (var resultStream = new MemoryStream())
            // {
            //     const int CHUNK_SIZE = 1 * 1024;
            //     byte[] buffer = new byte[CHUNK_SIZE];
            //     int bytesReceived;

            //     while ((bytesReceived = await socket.ReceiveAsync(buffer, SocketFlags.None)) > 0)
            //     {
            //         Console.WriteLine("s");

            //         byte[] actual = new byte[bytesReceived];
            //         Buffer.BlockCopy(buffer, 0, actual, 0, bytesReceived);

            //         await resultStream.WriteAsync(actual, 0, actual.Length);

            //         Console.WriteLine("e");
            //     }
            //     Console.WriteLine("done");
            //     Console.WriteLine(resultStream.ToString());
            // }

            socket.Disconnect(false);
            socket.Close();

            // SocketsHttpHandler handler = new SocketsHttpHandler();
            // HttpClient client = new HttpClient(handler);
            // client.Timeout = TimeSpan.FromSeconds(60);


            // var uri = new Uri("unix:///var/run/docker.sock");
            // if (uri.Scheme.ToLowerInvariant() != "unix")
            // {
            //     return;
            // }

            // var pipeString = uri.LocalPath;
            // var handler = new ManagedHandler(async (string host, int port, CancellationToken cancellationToken) =>
            // {
            //     var sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            //     await sock.ConnectAsync(new UnixDomainSocketEndPoint("/var/run/docker.sock"));
            //     return sock;
            // });

            // var client = new HttpClient(handler);
            // CancellationToken cancellationToken = new CancellationToken();
            // var request = new HttpRequestMessage(HttpMethod.Get, "/v1.40/containers/json");
            // request.Headers.Add("Host", "localhost");
            // request.Headers.Add("Accept", "*/*");
            // Console.WriteLine(request.ToString());
            // var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            // Console.WriteLine(response.ToString());
        }
    }
}