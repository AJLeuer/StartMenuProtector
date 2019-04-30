using System.Collections.Generic;
using System.Threading;
using Optional.Unsafe;
using StartMenuProtector.Data;
using static StartMenuProtector.Util.Util;

namespace StartMenuProtector.Control
{
    public class StartMenuSentinel
    {
        private Thread Thread;
        
        public SystemStateService SystemStateService { private get; set; }
        public SavedStartMenuDataService SavedStartMenuDataService { private get; set; }

        public StartMenuSentinel(SystemStateService systemStateService, SavedStartMenuDataService savedStartMenuDataService)
        {
            this.SystemStateService = systemStateService;
            this.SavedStartMenuDataService = savedStartMenuDataService;
        }
        
        public void Start()
        {
            Thread = new Thread(Run);
            Thread.Start();
        }

        private void Run()
        {
            CheckForDivergencesFromUsersSavedStartMenuState();
        }

        private void CheckForDivergencesFromUsersSavedStartMenuState()
        {
            foreach (StartMenuShortcutsLocation location in GetEnumValues<StartMenuShortcutsLocation>())
            {
                var appDataSavedStartMenuContents = SavedStartMenuDataService.GetStartMenuContents(location).Result.GetSubdirectory("Start Menu");

                if (appDataSavedStartMenuContents.HasValue)
                {
                    SystemStateService.OSEnvironmentStartMenuItems[location].RefreshContents();
                    Directory currentStartMenuItemsDirectoryState = SystemStateService.OSEnvironmentStartMenuItems[location];
                    Directory expectedStartMenuStateDirectoryState = appDataSavedStartMenuContents.ValueOrFailure();
                }
            }
        }
    }
}
