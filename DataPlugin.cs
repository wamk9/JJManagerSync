using GameReaderCommon;
using log4net.Plugin;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Effects;
using WoteverCommon.Extensions;
using System.Linq;
using JJManagerSync.Classes;
using System.Windows.Shell;
using SimHub.Plugins.DataPlugins.RGBDriver.UI;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace JJManagerSync
{
    [PluginDescription("Send SimHub data to JohnJohn 3D's devices (Needs JJManager installed and connected with device)")]
    [PluginAuthor("JohnJohn 3D")]
    [PluginName("JJManager Sync")]
    public class DataPlugin : SimHub.Plugins.IPlugin, IDataPlugin, IWPFSettingsV2
    {
        public static DataPluginSettings Settings = null;
        private static JJManagerSync.WebSocket _WebSocketServer;
        //private TcpServer tcpServer;
        private Dictionary<string, dynamic> valuesToSend = new Dictionary<string, dynamic>();
        private bool _IsSendingData = false;

        public static ObservableCollection<WebSocketClientInfo> ConnectedClients
        {
            get => _WebSocketServer.ConnectedClients;
        }

        public static void DisconnectClient(string connectionKey)
        {
            _WebSocketServer.DisconnectClient(connectionKey);
        }

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.jjmanagericon);

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => null;

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (!_IsSendingData)
            {
                // Define the value of our property (declared in init)
                if (data.GameRunning)
                {
                    if (data.OldData != null && data.NewData != null)
                    {
                        bool effectExecuted = false;

                        foreach (JJManagerSync.Classes.EffectsList effect in Settings.Effects)
                        {
                            if (effect.IsActivated && !effectExecuted)
                            {
                                if (effect.Redline != null)
                                {
                                    if (effect.Redline.CalculateRedline(ref data))
                                    {
                                        valuesToSend.Add("led_mode", 3);
                                        valuesToSend.Add("brightness", 100);
                                        SendToJJManager();
                                        effectExecuted = true;
                                    }
                                }
                                else if (effect.Rpm != null)
                                {
                                    int rpmPercentValue = effect.Rpm.CalculateRpm(ref data);

                                    if ((!Settings.Effects.First(obj => obj.EngineIgnition != null).IsActivated || rpmPercentValue > 0))
                                    {
                                        valuesToSend.Add("led_mode", 1);
                                        valuesToSend.Add("brightness", rpmPercentValue);
                                        SendToJJManager();
                                        effectExecuted = true;
                                    }
                                }
                                else if (effect.EngineIgnition != null)
                                {
                                    valuesToSend.Add("led_mode", 1);
                                    valuesToSend.Add("brightness", (effect.EngineIgnition.EngineIgnitionState(ref data) ? 100 : 0));
                                    SendToJJManager();
                                    effectExecuted = true;
                                }
                                else
                                {
                                    valuesToSend.Add("led_mode", 0);
                                    valuesToSend.Add("brightness", 0);
                                    SendToJJManager();
                                }
                            }
                        }
                    }
                }
                else
                {
                    valuesToSend.Add("led_mode", 0);
                    valuesToSend.Add("brightness", 0);
                    SendToJJManager();
                }
            }
        }

        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            // Save settings
            _WebSocketServer.Stop();
            valuesToSend.Add("led_mode", 0);
            valuesToSend.Add("brightness", 0);
            SendToJJManager();
            
            //this.SaveCommonSettings<DataPluginSettings>("JJManagerSync", Settings);
            this.SaveCommonSettings("JJManagerSync", Settings);
        }

        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return new Main(this);
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            SimHub.Logging.Current.Info("Starting JJManager Sync");

            // Load settings
            DataPluginSettings newSettings = new DataPluginSettings();
            Settings = this.ReadCommonSettings<DataPluginSettings>("JJManagerSync", () => new DataPluginSettings());
            Settings = (Settings.Effects.Length != newSettings.Effects.Length ? newSettings : Settings);

            _WebSocketServer = new JJManagerSync.WebSocket();
            _WebSocketServer.Start(2920);

            //_webSocketServer = new JJManagerSync.WebSocket();
            //_webSocketServer.Start();
            // Declare a property available in the property list, this gets evaluated "on demand" (when shown or used in formulas)
            this.AttachDelegate("CurrentDateTime", () => DateTime.Now);

            valuesToSend.Add("led_mode", 0);
            valuesToSend.Add("brightness", 0);
            SendToJJManager();

            // Order the effects by order var
            Settings.Effects = Settings.Effects.OrderBy(list => list.Order).ToArray();
        }

        public static void RestartWebSocketService()
        {
            if (_WebSocketServer != null)
            {
                _WebSocketServer.Stop();
            }

            _WebSocketServer = new JJManagerSync.WebSocket();
            _WebSocketServer.Start(2920);
        }

        private void SendToJJManager()
        {
            _IsSendingData = true;

            dynamic dataToSend = new ExpandoObject();
            var dataDict = dataToSend as IDictionary<string, object>;

            List<string> jsonString = new List<string>();

            foreach (KeyValuePair<string, dynamic> value in valuesToSend)
            {
                dataDict[value.Key] = value.Value;
            }

            _WebSocketServer.SendMessageToAllClients(dataToSend);
            valuesToSend.Clear();
            jsonString.Clear();
            _IsSendingData = false;
        }


    }
}