using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.View;


/* Todo: Refactor StartMenuItem into two separate subclasses. One for directories, one for files */
/* Todo: Fix first loaded start menu items view only showing directories */
/* Todo: Items in saved view should not change color when dragged */
/* Todo: Clicking on the "User" or "System" view reloads all items, but clicking on Active, Saved, etc. tabs does not */
/* Todo: Active view does not refresh its contents from the file system after save */

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
            
            activeProgramsViewController = new StartMenuViewController(activeDataController, systemStateController);
            savedProgramsViewController = new StartMenuViewController(savedDataController, systemStateController);

            MainWindow = new MainWindow();
            activeProgramsView = ((MainWindow) MainWindow).ActiveProgramShortcutsView;
            savedProgramsView = ((MainWindow) MainWindow).SavedProgramShortcutsView;
            
            activeProgramsView.Controller = activeProgramsViewController;
            savedProgramsView.Controller = savedProgramsViewController;
            
            MainWindow.Show();
        }
    }
}
