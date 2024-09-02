using FMOD;
using JJManagerSync.Classes;
using MahApps.Metro.IconPacks;
using SimHub.Plugins;
using SimHub.Plugins.Styles;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Resources.ResXFileRef;

namespace JJManagerSync
{
    /// <summary>
    /// Logique d'interaction pour SettingsControlDemo.xaml
    /// </summary>
    /// 

    public partial class Main : UserControl
    {
        public DataPlugin Plugin { get; }
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private System.Windows.Media.Brush _ListBackgroundColor = (System.Windows.Media.Brush) (new System.Windows.Media.BrushConverter()).ConvertFromString("#222");

        public Main()
        {
            InitializeComponent();

        }

        public Main(DataPlugin plugin) : this()
        {
            this.Plugin = plugin;

            BtnResetService.Click += BtnResetService_Click;

            RefreshEffectsList();
            timer.Interval = 2000; // 2000 milliseconds = 2 seconds
            timer.Tick += (sender, args) => RefreshConnectionsList();
            timer.Start();
        }

        private async void BtnResetService_Click(object sender, RoutedEventArgs e)
        {
            BtnResetService.IsEnabled = false;
            BtnResetService.Content = "Resetando Serviço...";

            DataPlugin.RestartWebSocketService();

            BtnResetService.Content = "Serviço Resetado!";

            await Task.Delay(5000);

            BtnResetService.IsEnabled = true;
            BtnResetService.Content = "Resetar Serviço";

        }

        private void RefreshConnectionsList()
        {
            lbConnections.Items.Clear();
            lbConnections.Background = _ListBackgroundColor;

            if (DataPlugin.ConnectedClients.Count > 0)
            {
                for (int i = 0; i < DataPlugin.ConnectedClients.Count; i++)
                {
                    ListBoxItem row = new ListBoxItem();

                    // Create a SHFlowGrid to hold the children
                    StackPanel rowGrid = new StackPanel
                    {
                        Width = Double.NaN // Auto width
                    };

                    SHButtonPrimary rowBtnDisconnect = new SHButtonPrimary
                    {
                        Uid = DataPlugin.ConnectedClients.ElementAt(i).Id,
                        Width = Double.NaN, // Auto width
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, -20, 20, 10),
                        Content = new PackIconMaterial { Kind = PackIconMaterialKind.LanDisconnect },
                    };

                    rowBtnDisconnect.Click += RowBtnDisconnect_Click;
                    
                    // Create a TextBlock
                    TextBlock rowTitle = new TextBlock
                    {
                        Text = DataPlugin.ConnectedClients.ElementAt(i).Id,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    };

                    Grid rowBtnGrid = new Grid
                    {
                        Width = Double.NaN, // Auto width
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };


                    // Add the children to the SHFlowGrid
                    rowGrid.Children.Add(rowTitle);
                    rowBtnGrid.Children.Add(rowBtnDisconnect);
                    rowGrid.Children.Add(rowBtnGrid);

                    // Set the SHFlowGrid as the content of the ListBoxItem
                    //row.Content = rowGrid;
                    row.Content = rowGrid;
                    row.Background = _ListBackgroundColor;
                    // Add the ListBoxItem to the ListBox
                    lbConnections.Items.Add(row);
                }
            }
            else
            {
                lbConnections.Items.Add(new ListBoxItem
                {
                    Content = "Sem items",
                    Background = _ListBackgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Height = 75,
                    IsEnabled = false,
                });
            }

            lbConnections.UpdateLayout();
        }

        private void RowBtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            (sender as SHButtonPrimary).IsEnabled = false;
            DataPlugin.DisconnectClient((sender as SHButtonPrimary).Uid);
            RefreshConnectionsList();
        }

        private void RefreshEffectsList()
        {
            lbEffects.Items.Clear();
            lbEffects.Background = _ListBackgroundColor;

            foreach (EffectsList effect in DataPlugin.Settings.Effects)
            {
                if (effect != null)
                {
                    // Create a new ListBoxItem
                    ListBoxItem row = new ListBoxItem();

                    // Create a SHFlowGrid to hold the children
                    StackPanel rowGrid = new StackPanel
                    {
                        Width = Double.NaN // Auto width
                    };


                    SHButtonPrimary rowBtnOrderUp = new SHButtonPrimary
                    {
                        Uid = effect.Order.ToString(),
                        Width = Double.NaN, // Auto width
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, -20, 120, 10),
                        Content = new PackIconMaterial { Kind = PackIconMaterialKind.ArrowUpBold },
                        IsEnabled = (effect.Order != 0)
                    };

                    SHButtonPrimary rowBtnOrderDown = new SHButtonPrimary
                    {
                        Uid = effect.Order.ToString(),
                        Width = Double.NaN, // Auto width
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, -20, 60, 10),
                        Content = new PackIconMaterial { Kind = PackIconMaterialKind.ArrowDownBold },
                        IsEnabled = (effect.Order < (DataPlugin.Settings.Effects.Count() - 1))
                    };


                    // Create a SHToggleButton
                    SHToggleButton rowToggle = new SHToggleButton
                    {
                        Uid = effect.Order.ToString(),
                        IsChecked = effect.IsActivated,
                        Width = Double.NaN, // Auto width
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, -20, 20, 10)
                    };

                    rowBtnOrderUp.Click += BtnOrderUp_Click;
                    rowBtnOrderDown.Click += BtnOrderDown_Click;
                    rowToggle.Checked += RowToggle_Checked;

                    // Create a TextBlock
                    TextBlock rowTitle = new TextBlock
                    {
                        Text = effect.Redline?.Name ?? effect.Rpm?.Name ?? effect.EngineIgnition?.Name ?? string.Empty,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    };

                    Grid rowBtnGrid = new Grid
                    {
                        Width = Double.NaN, // Auto width
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };


                    // Add the children to the SHFlowGrid
                    rowGrid.Children.Add(rowTitle);
                    rowBtnGrid.Children.Add(rowBtnOrderUp);
                    rowBtnGrid.Children.Add(rowBtnOrderDown);
                    rowBtnGrid.Children.Add(rowToggle);
                    rowGrid.Children.Add(rowBtnGrid);

                    // Set the SHFlowGrid as the content of the ListBoxItem
                    //row.Content = rowGrid;
                    row.Content = rowGrid;
                    row.Background = _ListBackgroundColor;

                    // Add the ListBoxItem to the ListBox
                    lbEffects.Items.Add(row);
                }
            }

            lbEffects.UpdateLayout();
        }

        private void BtnOrderUp_Click(object sender, RoutedEventArgs e)
        {
            uint actualOrder = uint.Parse((sender as SHButtonPrimary).Uid);

            if (actualOrder == 0)
            {
                return;
            }

            DataPlugin.Settings.Effects[actualOrder].Order--;
            DataPlugin.Settings.Effects[(actualOrder - 1)].Order++;
            DataPlugin.Settings.Effects = DataPlugin.Settings.Effects.OrderBy(effect => effect.Order).ToArray();
            Plugin.SaveCommonSettings("JJManagerSync", DataPlugin.Settings.Effects);
            RefreshEffectsList();
        }
        private void BtnOrderDown_Click(object sender, RoutedEventArgs e)
        {
            uint actualOrder = uint.Parse((sender as SHButtonPrimary).Uid);

            if (actualOrder > (DataPlugin.Settings.Effects.Count() - 1))
            {
                return;
            }

            DataPlugin.Settings.Effects[actualOrder].Order++;
            DataPlugin.Settings.Effects[(actualOrder + 1)].Order--;
            DataPlugin.Settings.Effects = DataPlugin.Settings.Effects.OrderBy(effect => effect.Order).ToArray();
            Plugin.SaveCommonSettings("JJManagerSync", DataPlugin.Settings.Effects);
            RefreshEffectsList();
        }

        private void RowToggle_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            SHToggleButton rowToggle = sender as SHToggleButton;
            
            if (rowToggle.Uid != null)
            {
                DataPlugin.Settings.Effects[int.Parse(rowToggle.Uid)].IsActivated = (bool) rowToggle.IsChecked;
                Plugin.SaveCommonSettings("JJManagerSync", DataPlugin.Settings.Effects);
            }
        }

        private async void StyledMessageBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var res = await SHMessageBox.Show("Message box", "Hello", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Question);

            await SHMessageBox.Show(res.ToString());
        }

        private void DemoWindow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var window = new DemoWindow();

            window.Show();
        }

        private async void DemodialogWindow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialogWindow = new DemoDialogWindow();

            var res = await dialogWindow.ShowDialogWindowAsync(this);

            await SHMessageBox.Show(res.ToString());
        }
    }
}