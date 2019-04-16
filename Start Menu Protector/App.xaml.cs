using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;

namespace StartMenuProtector
{
    public partial class App : Application
    {
        private readonly StartMenuSentinel sentinel = new StartMenuSentinel();
        private readonly StartMenuViewController controller = new StartMenuViewController();
        private StartMenuView view;
        
        protected override void OnStartup(StartupEventArgs startup)
        {
            sentinel.Start();
            
            MainWindow = new MainWindow();
            view = ((MainWindow) MainWindow).startMenuView;
            view.Controller = controller;
            
            MainWindow.Show();
        }
    }
}
