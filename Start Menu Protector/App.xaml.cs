using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;


/* Todo: Refactor StartMenuItem into two separate subclasses. One for directories, one for files */
/* Todo: Only directory items should change color when dragged over*/
/* Todo: No items (including directories) should ever change color when dragged over in Saved view*/
/* Todo: Clicking on the "User" or "System" view reloads all items, but clicking on Active, Saved, etc. tabs does not */
/* Todo: Active view does not refresh its contents from the file system after save */
/* Todo: Each view controller should have one of each type of data controller */
/* Todo: Related to the above: saving a new Saved state should be moved to SavedStartMenuDataController */

namespace StartMenuProtector
{
    public partial class App : Application
    {
        private readonly StartMenuSentinel sentinel = new StartMenuSentinel();
        private readonly SystemStateController systemStateController = new SystemStateController();
        private StartMenuDataController activeDataController;
        private StartMenuDataController savedDataController;
        private StartMenuViewController activeProgramsViewController;
        private StartMenuViewController savedProgramsViewController;
        private StartMenuShortcutsView activeProgramsView;
        private StartMenuShortcutsView savedProgramsView;

        protected override void OnStartup(StartupEventArgs startup)
        {
            activeDataController = new ActiveStartMenuDataController(systemStateController);
            savedDataController = new SavedStartMenuDataController(systemStateController);
            
            activeProgramsViewController = new ActiveStartMenuViewController(activeDataController, systemStateController);
            savedProgramsViewController = new SavedStartMenuViewController(savedDataController, systemStateController);

            MainWindow = new MainWindow();
            activeProgramsView = ((MainWindow) MainWindow).ActiveProgramShortcutsView;
            savedProgramsView = ((MainWindow) MainWindow).SavedProgramShortcutsView;
            
            activeProgramsView.Controller = activeProgramsViewController;
            savedProgramsView.Controller = savedProgramsViewController;
            
            MainWindow.Show();
        }
    }
}
