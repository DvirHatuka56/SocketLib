using System;
using System.Net;
using System.Net.Sockets;

namespace Sockets.SimpleSockets
{
    public class ServerSocket
    {
        private TcpListener Server { get; }
        public bool Stop { get; set; }
        public int AcceptedClientsAmount { get; private set; }
        public event EventHandler<ClientSocket> OnClientAccepting;
        public event EventHandler Started; 
        public event EventHandler ToStop;

        public ServerSocket(int listenPort = 13000)
        {
            Stop = false;
            AcceptedClientsAmount = 0;
            Server = new TcpListener(IPAddress.Parse("127.0.0.1"), listenPort);
        }

        public void Start()
        {
            Server.Start();
            Started?.Invoke(this, EventArgs.Empty);
            while (!Stop)
            {
                ToStop?.Invoke(this, EventArgs.Empty);
                if (Stop)
                {
                    Close();
                    break;
                }
                var client = new ClientSocket(Server.AcceptTcpClient());
                OnClientAccepting?.Invoke(this, client);
                AcceptedClientsAmount++;
            }
        }

        private void Close()
        {
            Server.Stop();
        }
    }
}