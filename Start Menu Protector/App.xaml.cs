using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;

namespace StartMenuProtector
{
    public partial class App : Application
    {
        private readonly SystemStateService systemStateService = new SystemStateService();
        private StartMenuSentinel           sentinel;
        private ActiveDataService           activeDataService;
        private SavedDataService            savedDataService;
        private QuarantineDataService       quarantineDataService;
        private StartMenuViewController     activeStartMenuItemsViewController;
        private StartMenuViewController     savedStartMenuItemsViewController;
        private StartMenuShortcutsView      activeStartMenuItemsView;
        private StartMenuShortcutsView      savedStartMenuItemsView;

        public App()
        {
            Startup += StartApplication;
            Exit    += CloseApplication;
        }

        private void StartApplication(object sender, StartupEventArgs @event)
        {
            activeDataService     = new ActiveDataService(systemStateService);
            savedDataService      = new SavedDataService(systemStateService);
            quarantineDataService = new QuarantineDataService(systemStateService);

            activeStartMenuItemsViewController = new ActiveViewController(activeDataService, savedDataService, systemStateService);
            savedStartMenuItemsViewController  = new SavedViewController(activeDataService, savedDataService, systemStateService);
                
            MainWindow               = new MainWindow();
            activeStartMenuItemsView = ((MainWindow) MainWindow).ActiveProgramShortcutsView;
            savedStartMenuItemsView  = ((MainWindow) MainWindow).SavedProgramShortcutsView;
            
            activeStartMenuItemsView.Controller = activeStartMenuItemsViewController;
            savedStartMenuItemsView.Controller  = savedStartMenuItemsViewController;
            
            sentinel = new StartMenuSentinel(systemStateService, savedDataService, quarantineDataService, ((MainWindow) MainWindow).SentinelToggleButton);
            sentinel.Start();
            
            MainWindow.Show();
        }
        
        private void CloseApplication(object sender, ExitEventArgs @event)
        {
            sentinel.Stop();
        }
    }
}
