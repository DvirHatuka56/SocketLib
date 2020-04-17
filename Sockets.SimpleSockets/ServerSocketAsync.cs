using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sockets.SimpleSockets
{
    public class ServerSocketAsync
    {
        private Socket Listener { get; set; }
        private int Port { get; }
        private int ConnectionLimit { get; }
        private ClientSocketAsync CurrentClient { get; set; }
        private ManualResetEvent ThreadNotifier { get; }

        public event EventHandler Bound;
        public event EventHandler<Exception> OnError;
        public event EventHandler<ClientSocketAsync> OnClientAccepted;

        public ServerSocketAsync(int connectionLimit, int port = 65432)
        {
            Port = port;
            ConnectionLimit = connectionLimit;
            ThreadNotifier = new ManualResetEvent(false);
            Listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public void Bind()
        {
            try
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Loopback, Port);
                Listener.Bind(localEndPoint);
                Listener.Listen(ConnectionLimit);
                Bound?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, e);
            }
        }

        public void StartMainLoop()
        {
            for (int i = 0; i < ConnectionLimit; ++i)
            {
                AcceptOne();
            }
        }

        public void AcceptOne()
        {
            try
            {
                ThreadNotifier.Reset();
                Listener.BeginAccept(AcceptCallback, Listener);
                ThreadNotifier.WaitOne();
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, e);
            }
        }
        
        private void AcceptCallback(IAsyncResult ar)
        {
            ThreadNotifier.Set();
            Listener = (Socket) ar.AsyncState;  
            CurrentClient = new ClientSocketAsync(Listener.EndAccept(ar));  
            OnClientAccepted?.Invoke(this, CurrentClient);
        }

        public bool ClientConnected()
        {
            return CurrentClient != null && CurrentClient.IsConnected;
        }

        public void Close()
        {
            CurrentClient?.Close();
            Listener?.Close();
        }
    }
}