using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JJManagerSync.Classes
{
    public class WebSocketClientInfo : INotifyCollectionChanged, INotifyPropertyChanged
    {
        private string _Id = null;
        private System.Net.WebSockets.WebSocket _WebSocket = null;
        private System.Net.WebSockets.WebSocketContext _WebSocketContext = null;
        private System.Net.HttpListenerContext _HttpListenerContext = null;
        private dynamic _LastSimHubData = new ExpandoObject();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public dynamic LastSimHubData
        {
            set => _LastSimHubData = value;
            get => _LastSimHubData;
        }

        public string Id
        {
            get => _Id;
        }

        public bool IsConnected
        {
            get
            {
                return (_WebSocket != null && _WebSocket.State == System.Net.WebSockets.WebSocketState.Open);
            }
        }

        public WebSocketClientInfo(System.Net.HttpListenerContext httpListenerContext, string id)
        {
            Task.Run(async () =>
            {
                try
                {
                    _HttpListenerContext = httpListenerContext;
                    _WebSocketContext = await _HttpListenerContext.AcceptWebSocketAsync(null);
                    _Id = id;
                    _WebSocket = _WebSocketContext.WebSocket;
                    Console.WriteLine("WebSocket connection established");
                }
                catch (Exception e)
                {
                    _HttpListenerContext.Response.StatusCode = 500;
                    _HttpListenerContext.Response.Close();
                    Console.WriteLine($"Exception: {e.Message}");
                }
            }).Wait();
        }

        public async Task Run()
        {
            if (_WebSocket != null)
            {
                byte[] bufferReceive = new byte[1024 * 4];
                byte[] bufferSend = new byte[1024 * 4];
                System.Net.WebSockets.WebSocketReceiveResult result = null;

                
                if (_WebSocket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    OnPropertyChanged("IsConnected");
                }

                try
                {
                    while (_WebSocket.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        result = await _WebSocket.ReceiveAsync(new ArraySegment<byte>(bufferReceive), CancellationToken.None);

                        if (result != null && result.MessageType == WebSocketMessageType.Close)
                        {
                            await _WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ContinueWith((x) => OnPropertyChanged("IsConnected"));
                            break;
                        }
                        else if (result != null && result.MessageType == WebSocketMessageType.Text)
                        {
                            dynamic json = Json.Parse(Encoding.UTF8.GetString(bufferReceive, 0, result.Count));
                            dynamic jsonToSend = new ExpandoObject();
                                
                            jsonToSend.status = "online";
                            jsonToSend.data = new ExpandoObject();

                            if (Json.HasKey(json, "request"))
                            {
                                foreach (var value in json.request)
                                {
                                    switch (value)
                                    {
                                        case "SimHubLastData":
                                            jsonToSend.data.SimHubLastData = LastSimHubData;
                                            break;
                                        case "PluginVersion":
                                            jsonToSend.data.PluginVersion = Assembly.GetExecutingAssembly().GetName().Version;
                                            break;
                                    }
                                }
                            }

                            var responseMessage = Encoding.UTF8.GetBytes(Json.Serialize(jsonToSend));
                            await _WebSocket.SendAsync(new ArraySegment<byte>(responseMessage), WebSocketMessageType.Text, true, CancellationToken.None);
                            bufferReceive = new byte[1024 * 4];
                            bufferSend = new byte[1024 * 4];
                            result = null;
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine($"WebSocketException: {ex.Message}");
                    OnPropertyChanged("IsConnected");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    OnPropertyChanged("IsConnected");
                }
            }
        }

        public async Task Stop()
        {
            if (_WebSocket != null && _WebSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                await _WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ContinueWith((x) => OnPropertyChanged("IsConnected"));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
