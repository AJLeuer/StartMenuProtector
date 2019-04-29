using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;

namespace StartMenuProtector
{
    public partial class App : Application
    {
        private readonly SystemStateService systemStateService = new SystemStateService();
        private StartMenuSentinel sentinel;
        private ActiveStartMenuDataService activeDataService;
        private SavedStartMenuDataService savedDataService;
        private StartMenuViewController activeStartMenuItemsViewController;
        private StartMenuViewController savedStartMenuItemsViewController;
        private StartMenuShortcutsView activeStartMenuItemsView;
        private StartMenuShortcutsView savedStartMenuItemsView;

        protected override void OnStartup(StartupEventArgs startup)
        {
            sentinel = new StartMenuSentinel(systemStateService);
            
            activeDataService = new ActiveStartMenuDataService(systemStateService);
            savedDataService = new SavedStartMenuDataService(systemStateService);
            
            activeStartMenuItemsViewController = new ActiveStartMenuViewController(activeDataService, savedDataService, systemStateService);
            savedStartMenuItemsViewController = new SavedStartMenuViewController(activeDataService, savedDataService, systemStateService);

            MainWindow = new MainWindow();
            activeStartMenuItemsView = ((MainWindow) MainWindow).ActiveProgramShortcutsView;
            savedStartMenuItemsView = ((MainWindow) MainWindow).SavedProgramShortcutsView;
            
            activeStartMenuItemsView.Controller = activeStartMenuItemsViewController;
            savedStartMenuItemsView.Controller = savedStartMenuItemsViewController;
            
            MainWindow.Show();
        }
    }
}
