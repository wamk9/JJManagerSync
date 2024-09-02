using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net;
using System.Threading;
using WoteverCommon.Extensions;
using FMOD;
using System.Collections.ObjectModel;
using JJManagerSync.Classes;


namespace JJManagerSync 
{
    public class WebSocket
    {
        private HttpListener _HttpListener;
        private ObservableCollection<WebSocketClientInfo> _ConnectedClients = new ObservableCollection<WebSocketClientInfo>();
        private string _Url = "";
        private readonly int _keepAliveInterval = 500;
        private string _LastMessageToSend = "123";
        private static readonly object _lock = new object();


        public bool IsListening
        {
            get => _HttpListener.IsListening;
        }

        public ObservableCollection<WebSocketClientInfo> ConnectedClients
        {
            get => _ConnectedClients;
        }

        public async Task DisconnectClient(string id)
        {
            for (int i = 0; i < _ConnectedClients.Count; i++)
            {
                if (_ConnectedClients[i].Id == id)
                {
                    await _ConnectedClients[i].Stop();
                    _ConnectedClients.RemoveAt(i);
                    break;
                }
            }
        }

        public void Start(int port)
        {
            _Url = "http://localhost:" + port + "/jjmanager/";

            _HttpListener = new HttpListener();
            _HttpListener.Prefixes.Add(_Url);
            _HttpListener.Start();

            Task.Run(() => Listen());
        }

        public void Stop()
        {
            _HttpListener?.Stop();
        }

        private void Listen()
        {
            while (_HttpListener.IsListening)
            {
                var context = _HttpListener.GetContextAsync().Result;

                if (context.Request.IsWebSocketRequest && (context.Request.QueryString.AllKeys.Contains("device") || context.Request.Headers.AllKeys.Contains("device")))
                {
                    bool isOnConnectedClients = false;

                    string id = context.Request.QueryString["device"] ?? context.Request.Headers.Get("device") ?? "Unknown Device";

                    foreach (WebSocketClientInfo client in _ConnectedClients)
                    {
                        isOnConnectedClients = (client.Id == id) ? true : isOnConnectedClients;
                    }

                    if (!isOnConnectedClients)
                    {
                        WebSocketClientInfo webSocketClient = new WebSocketClientInfo(context, id);
                        webSocketClient.PropertyChanged += WebSocketClient_PropertyChanged;
                        Task.Run(() => webSocketClient.Run());
                        //await webSocketClient.Run();
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private void WebSocketClient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is WebSocketClientInfo client)
            {
                int index = _ConnectedClients.FindIndex(obj => obj.Id == client.Id);

                if (index > -1 && !client.IsConnected)
                {
                    _ConnectedClients.RemoveAt(index);
                }

                if (index == -1 && client.IsConnected)
                {
                    _ConnectedClients.Add(client);
                }
            }
        }

        public void SendMessageToAllClients(dynamic message)
        {
            for (int i = 0; i < _ConnectedClients.Count; i++)
            {
                _ConnectedClients[i].LastSimHubData = message;
            }
        }
    }
}
