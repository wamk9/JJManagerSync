using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class TcpServer
{
    private TcpListener listener;
    private bool _IsRunning = false;
    private List<TcpClient> _Clients = new List<TcpClient>();

    public TcpServer(string ipAddress, int port)
    {
        listener = new TcpListener(IPAddress.Parse(ipAddress), port);
    }

    public void Start()
    {
        listener.Start();
        _IsRunning = true;
        Console.WriteLine("Server started...");

        Thread listenerThread = new Thread(ListenForClients);
        listenerThread.Start();
    }

    public void SendMessagesToClients(string messageToSend)
    {
        if (_IsRunning)
        {
            int clientCount = _Clients.Count;
            //use 'lock (clients)' if have problems on foreach...
            for (int i = 0; i < clientCount; i++)
            {
                if (_Clients[i].Connected)
                {
                    NetworkStream stream = _Clients[i].GetStream();
                    byte[] message = Encoding.ASCII.GetBytes(messageToSend);
                    byte[] lengthPrefix = BitConverter.GetBytes(message.Length);
                    stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                    stream.Write(message, 0, message.Length);
                    //stream.Close();
                }
                else
                {
                    _Clients.RemoveAt(i);
                    i--;
                    clientCount--;
                }
            }
        }
    }

    private bool IsClientConnectedBefore(TcpClient client)
    {
        IPEndPoint remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
        foreach (TcpClient existingClient in _Clients)
        {
            IPEndPoint existingEndPoint = (IPEndPoint)existingClient.Client.RemoteEndPoint;
            if (existingEndPoint.Address.Equals(remoteEndPoint.Address) &&
                existingEndPoint.Port == remoteEndPoint.Port)
            {
                return true;
            }
        }
        return false;
    }

    private void ListenForClients()
    {
        try
        {
            while (_IsRunning)
            {
                TcpClient client = listener.AcceptTcpClient();

                if (!IsClientConnectedBefore(client))
                {
                    Console.WriteLine("Client connected...");
                    _Clients.Add(client);
                }
            }
        }
        catch (SocketException)
        {
            if (_IsRunning)
            {
                Stop();
                Start();
            }
        }
    }

    private void HandleClientComm(object client_obj)
    {
        try
        {
            TcpClient client = (TcpClient)client_obj;
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead;

            while (_IsRunning && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: " + message);

                // Send a response back to the client
                byte[] response = Encoding.ASCII.GetBytes("Message received");
                stream.Write(response, 0, response.Length);
            }

            Console.WriteLine("Client disconnected...");
        }
        catch (SocketException)
        {
            if (_IsRunning)
            {
                Stop();
                Start();
            }
        }
    }

    public void Stop()
    {
        _IsRunning = false;
        listener.Stop();
        _Clients.Clear();
    }
}
