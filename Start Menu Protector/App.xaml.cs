using System;
using System.Windows;
using System.Windows.Threading;
using StartMenuProtector.Configuration;
using StartMenuProtector.Control;
using StartMenuProtector.Util;
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
            SetupExceptionHandling();
            SetupEventHandling();
        }
        

        private void StartApplication(object sender, StartupEventArgs @event)
        {
            LogManager.Start();
            
            LogManager.Log("Starting application.");
            
            activeDataService     = new ActiveDataService(systemStateService);
            savedDataService      = new SavedDataService(systemStateService);
            quarantineDataService = new QuarantineDataService(systemStateService);

            activeStartMenuItemsViewController = new ActiveViewController(activeDataService, savedDataService, systemStateService);
            savedStartMenuItemsViewController  = new SavedViewController(activeDataService, savedDataService, systemStateService);

            sentinel                 = new StartMenuSentinel(systemStateService, savedDataService, quarantineDataService);
            MainWindow               = new StartMenuProtectorWindow(sentinel);
            sentinel.Start();
            
            activeStartMenuItemsView = ((StartMenuProtectorWindow) MainWindow).ActiveProgramShortcutsView;
            savedStartMenuItemsView  = ((StartMenuProtectorWindow) MainWindow).SavedProgramShortcutsView;

            activeStartMenuItemsView.Controller = activeStartMenuItemsViewController;
            savedStartMenuItemsView.Controller  = savedStartMenuItemsViewController;

            ConfigureApplication();

            MainWindow?.Show();
        }

        private void ConfigureApplication()
        {
            if (MainWindow != null)
            {
                MainWindow.Loaded += (object sender, RoutedEventArgs @event) =>
                {
                    MainWindow.WindowState = Config.StartupWindowState[Config.TargetBuildEnvironment];    
                };
            }
        }

        private void CloseApplication(object sender, ExitEventArgs @event)
        {
            LogManager.Log("Closing application.");

            sentinel.Stop();
            
            LogManager.Stop();
        }
        
        
        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs exceptionEvent) =>
            {
                ProcessException(exceptionEvent, (Exception) exceptionEvent.ExceptionObject);
            };

            DispatcherUnhandledException += (object sender, DispatcherUnhandledExceptionEventArgs exceptionEvent) =>
            {
                ProcessException(exceptionEvent, exceptionEvent.Exception);
            };
        }
        
        private void SetupEventHandling()
        {
            Startup += StartApplication;
            Exit += CloseApplication;
        }

        private void ProcessException<ExceptionEventType>(ExceptionEventType exceptionEvent, Exception exception) where ExceptionEventType : EventArgs
        {
            if (exceptionEvent is UnhandledExceptionEventArgs)
            {
                LogManager.Log($"Application encountered but did not handle the following exception: {exception}");
            }
            else if (exceptionEvent is DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
            {
                LogManager.Log($"Application encountered the following exception: {exception}");
                dispatcherUnhandledExceptionEventArgs.Handled = true;
            }
            
            LogManager.Log($"Stack trace for exception: {exception.StackTrace}");
        }
    }
}
