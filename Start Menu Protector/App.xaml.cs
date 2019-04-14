using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;

namespace StartMenuProtector
{
    public partial class App : Application
    {
        public StartMenuViewController Controller { get; } = new StartMenuViewController();
        public StartMenuView View { get; private set; }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = new MainWindow();
            View = ((MainWindow) MainWindow).startMenuView;
            View.Controller = Controller;
            
            MainWindow.Show();
        }
    }
}
