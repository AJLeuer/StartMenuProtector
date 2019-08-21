using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using StartMenuProtector.Control;
using static StartMenuProtector.Configuration.Config;
using static StartMenuProtector.Util.Util;

namespace StartMenuProtector.View
{
    /// <summary>
    /// Interaction logic for StartMenuProtectorWindow.xaml
    /// </summary>
    public partial class StartMenuProtectorWindow : Window
    {
        private TaskbarIcon TrayIcon = new TaskbarIcon { ToolTipText = "Start Menu Protector", IconSource = new BitmapImage(GetResourceURI(TrayIconFilePath))};
        public StartMenuSentinel StartMenuSentinel { get; set; }
        
        public StartMenuProtectorWindow(StartMenuSentinel sentinel)
        {
            InitializeComponent();
            DataContext = this;

            TrayIcon.TrayMouseDoubleClick += Restore;
            StartMenuSentinel = sentinel;
            
            SentinelToggleButton.ToggleOnEvent  += StartMenuSentinel.Enable;
            SentinelToggleButton.ToggleOffEvent += StartMenuSentinel.Disable;
        }

        protected override void OnStateChanged(EventArgs @event)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            
            base.OnStateChanged(@event);
        }
        
        private void Restore(object sender, RoutedEventArgs routedEvent)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }
    }
}
