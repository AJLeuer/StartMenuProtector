using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;

namespace StartMenuProtector
{
    public partial class App : Application
    {
        private readonly StartMenuSentinel sentinel = new StartMenuSentinel();
        private readonly SystemStateController systemStateController = new SystemStateController();
        private StartMenuDataController dataController;
        private StartMenuViewController viewController;
        private StartMenuView view;
        
        protected override void OnStartup(StartupEventArgs startup)
        {
            dataController = new StartMenuDataController(systemStateController);
            viewController = new StartMenuViewController(dataController, systemStateController);

            MainWindow = new MainWindow();
            view = ((MainWindow) MainWindow).startMenuView;
            view.Controller = viewController;
            
            MainWindow.Show();
        }
    }
}
