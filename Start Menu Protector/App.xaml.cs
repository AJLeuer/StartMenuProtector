using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;


/* Todo: Items deleted in Active view are being copied to Saved */
/* Todo: Refactor StartMenuItem into two separate subclasses. One for directories, one for files */
/* Todo: Only directory items should change color when dragged over*/
/* Todo: In Saved view, *no* items (including directories) should ever change color when dragged over*/
/* Todo: Clicking on the "User" or "System" view reloads all items, but clicking on Active, Saved, etc. tabs does not */
/* Todo: Active view does not refresh its contents from the file system after save */
/* Todo: SavedStartMenuDataService should delete old contents when copying new state */
/* Todo: ActiveViewController should enter a new state if it detects the user has made a change to the active view.
         Normally, the controller overwrites the contents of the active view with the start menu contents 
         loaded from the OS environment any time the Active view is reloaded. In this state however, the controller
         should *not* overwrite the contents of the Active view. This state should last from the time the first change
         is made, and only end when the user saves */
/* Todo: The top level directory objects in Global (e.g. ActiveStartMenuShortcuts, SavedSystemStartMenuShortcuts, etc.) could
         be organized into Dictionaries by the type of view they represent (Active, Saved, etc.) */

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
