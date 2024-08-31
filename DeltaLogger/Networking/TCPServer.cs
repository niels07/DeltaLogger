using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DeltaLogger.Networking
{
    public class TCPServer
    {
        private readonly TcpListener _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public Action<string>? OnClientUpdate { get; set; }
        public bool IsRunning { get; private set; } = false;
        public IPAddress IPAddress { get; set; }

        public TCPServer(string ipAddress, int port, Action<string> onClientUpdate)
        {
            if (!IPAddress.TryParse(ipAddress, out IPAddress? ip))
            {
                throw new TCPException("Invalid IP address");
            }
            IPAddress = ip;
            OnClientUpdate = onClientUpdate;
            _listener = new(ip, port);
        }

        public void Start() 
        {
            if (IsRunning)
            {
                return;
            }
            _listener.Start();
            _cancellationTokenSource = new();
            _cancellationToken = _cancellationTokenSource.Token;
            Task.Run(() => ListenForClients(), _cancellationTokenSource.Token);
            IsRunning = true;
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();
            IsRunning = false;
        }

        private void ListenForClients()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (_listener == null)
                {
                    throw new TCPException("Listener is unexpectedly null");
                }

                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    if (client != null)
                    {
                        Thread clientThread = new(HandleClientComm);
                        clientThread.IsBackground = true;
                        clientThread.Start(client);
                    }
                }
                catch (SocketException ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }                              
            }
        }

        private void HandleClientComm(object? client_obj)
        {
            if (client_obj is not TcpClient client)
            {
                throw new TCPException("Client object is not a TCP client");
            }

            string? clientIp = ((IPEndPoint?)client.Client.RemoteEndPoint)?.Address.ToString();
            
            if (clientIp == null)
            {
                throw new TCPException("Failed to get client IP address");
            }

            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                OnClientUpdate?.Invoke(receivedData);
            }
        }
    }
}
