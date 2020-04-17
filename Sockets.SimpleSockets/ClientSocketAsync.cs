using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sockets.SimpleSockets
{
    internal class StateObject {  
        // Client socket.  
        public Socket workSocket = null;  
        // Size of receive buffer.  
        public int BufferSize = 256;  
        // Receive buffer.  
        public byte[] buffer = new byte[256];  
        // Received data string.  
        public StringBuilder sb = new StringBuilder();  
    }  
    
    public class ClientSocketAsync
    {
        private Socket Client { get; set; }
        private string Host { get; set; }
        private int Port { get; set; }

        public bool IsConnected => Client.Connected;

        public event EventHandler Connected;
        public event EventHandler<int> Sent;
        public event EventHandler<string> Received;
        public event EventHandler<ErrorEventArgs> ErrorAccrued; 

        public ClientSocketAsync(string ip="127.0.01", int port=65432)
        {
            IPHostEntry host = Dns.GetHostEntry(ip);
            Host = ip;
            Port = port;
            Client = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public ClientSocketAsync(Socket socket)
        {
            Client = socket;
        }

        public void Connect()
        {
            if (Client.Connected) { return; }
            try
            {
                Client.Connect(Host, Port);
                Connected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e));
            }
        }
        
        public void ConnectAsync()
        {  
            if (Client.Connected) { return; }
            Client.BeginConnect(Host, Port,
                ConnectCallback, Client );
        }
        
        public void Send(String data, Encoding encoding)
        {  
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = encoding.GetBytes(data);  
  
            // Begin sending the data to the remote device.  
            Client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,  
                SendCallback, Client);  
        }  
        
        public void Receive(int bytes)
        {  
            try
            {  
                // Create the state object.  
                StateObject state = new StateObject {workSocket = Client, BufferSize = bytes};
                
                // Begin receiving the data from the remote device.  
                Client.BeginReceive( state.buffer, 0,
                    state.BufferSize,
                    0, ReceiveCallback, state);  
            } 
            catch (Exception e)
            {  
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e));  
            }
        }

        public void Close()
        {
            try
            {
                if (!Client.Connected) { return; }
                Client.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine($"caught {e}");
            }
        }
        
        private void ReceiveCallback(IAsyncResult ar) 
        {  
            try {  
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject) ar.AsyncState;  
                Client = state.workSocket;  
                // Read data from the remote device.  
                int bytesRead = Client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,bytesRead));
                }
                if (bytesRead < state.BufferSize) 
                {
                    //  Get the rest of the data.  
                    Client.BeginReceive(state.buffer,0, state.BufferSize,0,  
                                        ReceiveCallback, state);  
                } 
                else 
                {  
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length >= state.BufferSize)
                    {  
                        Received?.Invoke(this, state.sb.ToString());
                    }
                }  
            } catch (Exception e) {  
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e));    
            }  
        }  
        
        private void SendCallback(IAsyncResult ar)
        {  
            try {  
                // Retrieve the socket from the state object.  
                Client = (Socket) ar.AsyncState;  
  
                // Complete sending the data to the remote device.  
                int bytesSent = Client.EndSend(ar);  
                Sent?.Invoke(this, bytesSent);
            } catch (Exception e) {  
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e));    
            }  
        } 
        
        private void ConnectCallback(IAsyncResult ar) {  
            try {  
                // Retrieve the socket from the state object.  
                Client = (Socket) ar.AsyncState;
                // Complete the connection.  
                Client.EndConnect(ar);  
                Connected?.Invoke(this, EventArgs.Empty);
            } catch (Exception e) {  
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e));    
            }  
        }  
        
    }
}