using System;
using System.Net.Sockets;
using System.Text;

namespace Sockets.SimpleSockets
{ 
    public class ClientSocket
	{
		private readonly TcpClient _client;
		private readonly NetworkStream _serverStream;
		
		
		/// <summary>
		///  Start communication with the server on the given ip and port
		/// </summary>
		/// <param name="ip"> dest ip, default: 127.0.0.1</param>
		/// <param name="port">dest port, default: 42069</param>
		public ClientSocket(string ip = "127.0.0.1", int port = 42069)
		{
			_client = new TcpClient(ip, port);
			_serverStream = _client.GetStream();
		}
		
		/// <summary>
		/// Inits the class with custom client
		/// </summary>
		/// <param name="client">Tcp client</param>
		public ClientSocket(TcpClient client)
		{
			_client = client;
			_serverStream = _client.GetStream();
		}
		
		/// <summary>
		/// Gets message from the server
		/// </summary>
		/// <returns>the message that came from the server</returns>
		public string ReceiveString(int bytes)
		{
			byte[] inStream = new byte[bytes];
			_serverStream.Read(inStream, 0, bytes);
			inStream = ResizeBuffer(inStream);
			return Encoding.UTF8.GetString(inStream);
		}
		
		/// <summary>
		/// Gets int message from the server
		/// </summary>
		/// <returns>the message that came from the server</returns>
		public int ReceiveInt(int bytes)
		{
			byte[] inStream = new byte[bytes];
			_serverStream.Read(inStream, 0, bytes);
			inStream = ResizeBuffer(inStream);
			return int.Parse(Encoding.UTF8.GetString(inStream));
		}
		
		/// <summary>
		/// Send a message to the server
		/// </summary>
		/// <param name="msg">message to send</param>
		public void SendMessage(string msg)
		{
			try
			{
				byte[] outStream = Encoding.UTF8.GetBytes(msg);
				_serverStream.Write(outStream, 0, outStream.Length);
				_serverStream.Flush();
			}
			catch (Exception)
			{
				// in case the communication lost
			} 
		}
		
		/// <summary>
		/// Closes the socket.
		/// To open new one use "new" statement
		/// </summary>
		public void Close()
		{
			if (!_client.Connected)
			{
				return;
			}
			_client.Close();
			_serverStream.Close();
		}
		
		/// <summary>
		///  Resizes the buffer to it's actual length
		/// </summary>
		/// <param name="buffer">Buffer to resize</param>
		/// <returns>new buffer</returns>
		private byte[] ResizeBuffer(byte[] buffer)
		{
			if (buffer.Length == 1) return buffer;
			byte[] newBuffer = new byte[GetActualLen(buffer)];
			for (int i = 0; i < newBuffer.Length; i++)
			{
				newBuffer[i] = buffer[i];
			}
			return newBuffer;
		}
		
		/// <summary>
		/// Checks the real buffer length
		/// </summary>
		/// <param name="buffer">buffer to check</param>
		/// <returns>actual length of the given buffer</returns>
		private static int GetActualLen(byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				if (buffer[i] == 0)
				{
					return i;
				}
			}
			return buffer.Length;
		}

	}
}