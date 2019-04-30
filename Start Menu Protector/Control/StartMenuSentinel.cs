using System;
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
            var (unexpected, missing) = CheckForDivergencesFromUsersSavedStartMenuState();
        }

        private  (ICollection<RelocatableItem> unexpected, ICollection<RelocatableItem> missing) CheckForDivergencesFromUsersSavedStartMenuState()
        {
            ICollection<RelocatableItem> unexpected = new RelocatableItem[] {}, absent = new RelocatableItem[] {};
            
            foreach (StartMenuShortcutsLocation location in GetEnumValues<StartMenuShortcutsLocation>())
            {
                var appDataSavedStartMenuContents = SavedStartMenuDataService.GetStartMenuContents(location).Result.GetSubdirectory("Start Menu");

                if (appDataSavedStartMenuContents.HasValue)
                {
                    SystemStateService.OSEnvironmentStartMenuItems[location].RefreshContents();
                    Directory currentStartMenuItemsDirectoryState = SystemStateService.OSEnvironmentStartMenuItems[location];
                    Directory expectedStartMenuStateDirectoryState = appDataSavedStartMenuContents.ValueOrFailure();

                    (unexpected, absent) = Directory.FindDivergences(sourceOfTruth: expectedStartMenuStateDirectoryState, test: currentStartMenuItemsDirectoryState);
                }
            }

            return (unexpected, absent);
        }

        /// <summary>
        /// For all potentiallyDisplacedItems that are shortcuts, looks through
        /// </summary>
        /// <param name="potentiallyDisplacedItems"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void PlaceShortcutsMovedOutOfPositionIntoOriginalExpectedPosition(out ICollection<RelocatableItem> potentiallyDisplacedItems)
        {
            throw new NotImplementedException();
        }
    }
}
