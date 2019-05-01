using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using static StartMenuProtector.Configuration.Config;
using static StartMenuProtector.Util.Util;

namespace StartMenuProtector.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskbarIcon TrayIcon = new TaskbarIcon { ToolTipText = "Start Menu Protector", IconSource = new BitmapImage(GetResourceURI(TrayIconFilePath))};

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            TrayIcon.TrayMouseDoubleClick += Restore;
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
