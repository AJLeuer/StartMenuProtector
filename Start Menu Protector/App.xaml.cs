using System.Windows;
using StartMenuProtector.Control;
using StartMenuProtector.Util;
using StartMenuProtector.View;

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
            #if DEBUG
            ConsoleManager.Show();
            #endif
            
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
