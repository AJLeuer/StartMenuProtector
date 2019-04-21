using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;


/* Todo: Refactor StartMenuItem into two separate subclasses. One for directories, one for files */
/* Todo: Only directory items should change color when dragged over*/
/* Todo: In Saved view, *no* items (including directories) should ever change color when dragged over*/
/* Todo: Clicking on the "User" or "System" view reloads all items, but clicking on Active, Saved, etc. tabs does not */
/* Todo: Active view does not refresh its contents from the file system after save */

namespace StartMenuProtector
{
    public partial class App : Application
    {
        private readonly StartMenuSentinel sentinel = new StartMenuSentinel();
        private readonly SystemStateService systemStateService = new SystemStateService();
        private ActiveStartMenuDataService activeDataService;
        private SavedStartMenuDataService savedDataService;
        private StartMenuViewController activeStartMenuItemsViewController;
        private StartMenuViewController savedStartMenuItemsViewController;
        private StartMenuShortcutsView activeStartMenuItemsView;
        private StartMenuShortcutsView savedStartMenuItemsView;

        protected override void OnStartup(StartupEventArgs startup)
        {
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
