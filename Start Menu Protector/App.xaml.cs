using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;


/* Todo: Refactor StartMenuItem into two separate subclasses. One for directories, one for files */
/* Todo: Only directory items should change color when dragged over*/
/* Todo: No items (including directories) should ever change color when dragged over in Saved view*/
/* Todo: Clicking on the "User" or "System" view reloads all items, but clicking on Active, Saved, etc. tabs does not */
/* Todo: Active view does not refresh its contents from the file system after save */
/* Todo: Each view controller should have one of each type of data service */
/* Todo: Related to the above: saving a new Saved state should be moved to SavedStartMenuDataService */

namespace StartMenuProtector
{
    public partial class App : Application
    {
        private readonly StartMenuSentinel sentinel = new StartMenuSentinel();
        private readonly SystemStateService systemStateService = new SystemStateService();
        private StartMenuDataService activeDataService;
        private StartMenuDataService savedDataService;
        private StartMenuViewController activeProgramsViewController;
        private StartMenuViewController savedProgramsViewController;
        private StartMenuShortcutsView activeProgramsView;
        private StartMenuShortcutsView savedProgramsView;

        protected override void OnStartup(StartupEventArgs startup)
        {
            activeDataService = new ActiveStartMenuDataService(systemStateService);
            savedDataService = new SavedStartMenuDataService(systemStateService);
            
            activeProgramsViewController = new ActiveStartMenuViewController(activeDataService, systemStateService);
            savedProgramsViewController = new SavedStartMenuViewController(savedDataService, systemStateService);

            MainWindow = new MainWindow();
            activeProgramsView = ((MainWindow) MainWindow).ActiveProgramShortcutsView;
            savedProgramsView = ((MainWindow) MainWindow).SavedProgramShortcutsView;
            
            activeProgramsView.Controller = activeProgramsViewController;
            savedProgramsView.Controller = savedProgramsViewController;
            
            MainWindow.Show();
        }
    }
}
