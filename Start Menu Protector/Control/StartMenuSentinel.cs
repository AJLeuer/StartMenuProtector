using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Optional;
using Optional.Unsafe;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using static StartMenuProtector.Util.Util;
using static StartMenuProtector.Configuration.Globals;
using static StartMenuProtector.Configuration.Config;
using Directory = StartMenuProtector.Data.Directory;
using static StartMenuProtector.Util.LogManager;


namespace StartMenuProtector.Control 
{
    public class StartMenuSentinel 
    {
        private RunningState applicationState = RunningState.Disabled;

        public RunningState ApplicationState
        {
            get
            {
                return applicationState;
            }
            private set
            {
                if (value == RunningState.Disabled)
                {
                    Disable();
                }
                
                applicationState = value;
                
                if (value == RunningState.Disabled)
                {
                    ContinueRunFlag.Set();
                }
            }
        }
        
        public RunningState UserSelectedState { get; private set; } = RunningState.Enabled;
        
        private readonly AutoResetEvent ContinueRunFlag = new AutoResetEvent (false);

        public bool Enabled
        {
            get { return (ApplicationState == RunningState.Enabled) && (UserSelectedState == RunningState.Enabled); }
        }

        private Thread Thread { get; set; }
        
        public SystemStateService SystemStateService { private get; set; }
        public SavedDataService SavedDataService { private get; set; }
        public QuarantineDataService QuarantineDataService { private get; set; }


        public readonly Dictionary<StartMenuShortcutsLocation, ICollection<IFileSystemItem>> ItemsToRestore = new Dictionary<StartMenuShortcutsLocation, ICollection<IFileSystemItem>>
        {
            { StartMenuShortcutsLocation.User,   new HashSet<IFileSystemItem>() },
            { StartMenuShortcutsLocation.System, new HashSet<IFileSystemItem>() }
        };
        
        public readonly Dictionary<StartMenuShortcutsLocation, ICollection<IFileSystemItem>> ItemsToQuarantine = new Dictionary<StartMenuShortcutsLocation, ICollection<IFileSystemItem>>
        {
            { StartMenuShortcutsLocation.User,   new HashSet<IFileSystemItem>() },
            { StartMenuShortcutsLocation.System, new HashSet<IFileSystemItem>() }
        };
        
        public StartMenuSentinel(SystemStateService systemStateService, SavedDataService savedDataService, QuarantineDataService quarantineDataService)
        {
            this.SystemStateService = systemStateService;
            this.SavedDataService = savedDataService;
            this.QuarantineDataService = quarantineDataService;
        }

        public void Start()
        {
            ApplicationState = RunningState.Enabled;
            Thread = new Thread(Run);
            Thread.Start();
        }
        
        public void Stop()
        {
            ApplicationState = RunningState.Disabled;
            ContinueRunFlag.Set();
            Thread.Join();
        }

        public void Enable()
        {
            lock (UserSelectedState)
            {
                this.UserSelectedState = RunningState.Enabled;
            }
            
            ContinueRunFlag.Set();
        }
        
        public void Disable()
        {
            lock (UserSelectedState)
            {
                this.UserSelectedState = RunningState.Disabled;
            }
        }
        
        private void Run()
        {
            while (ApplicationState == RunningState.Enabled)
            {
                while (UserSelectedState == RunningState.Enabled)
                {
                    lock (UserSelectedState)
                    {
                        try
                        {
                            MonitorStartMenuState();
                        }
                        catch (Exception exception)
                        {
                            Log($"Application encountered the following exception: {exception}");
                            Log($"Stack trace for exception: {exception.StackTrace}");
                        }
                    }
                    
                    //the vast majority of the time this particular call to WaitOne() just functions
                    //as a Sleep(). It's only in the case of an application shutdown that we'll actually
                    //get a signal
                    ContinueRunFlag.WaitOne(TimeSpan.FromSeconds(ProtectorRunIntervalSeconds));
                }

                if ((ApplicationState == RunningState.Enabled) && (UserSelectedState == RunningState.Disabled))
                {
                    ContinueRunFlag.WaitOne();
                }
            }
        }

        private void MonitorStartMenuState()
        {
            foreach (StartMenuShortcutsLocation location in GetEnumValues<StartMenuShortcutsLocation>())
            {
                var (unexpected, missing) = CheckForDivergencesFromUsersSavedStartMenuState(location);

                FilterOutItemsWithTheSameName(location, unexpectedItems: unexpected, missingItems: missing);
                FilterOutShortcutsMovedOutOfPosition(unexpectedItems: unexpected, missingItems: missing);

                ItemsToQuarantine[location].AddAll(unexpected);
                ItemsToRestore[location]   .AddAll(missing);
                
                RestoreExpectedStartMenuItems(location);
                QuarantineUnrecognizedStartMenuItems(location);
            }
        }

        private (ICollection<RelocatableItem> unexpected, ICollection<RelocatableItem> missing) CheckForDivergencesFromUsersSavedStartMenuState(StartMenuShortcutsLocation location)
        {
            ICollection<RelocatableItem> unexpected = new HashSet<RelocatableItem>();
                
            ICollection<RelocatableItem> absent     = new HashSet<RelocatableItem>();

            Option<IDirectory> appDataSavedStartMenuContents = SavedDataService.GetStartMenuContentDirectoryMainSubdirectory(location).Result;

            if (appDataSavedStartMenuContents.HasValue)
            {
                SystemStateService.OSEnvironmentStartMenuItems[location].RefreshContents();
                Directory currentStartMenuItemsDirectoryState = SystemStateService.OSEnvironmentStartMenuItems[location];
                IDirectory expectedStartMenuStateDirectoryState = appDataSavedStartMenuContents.ValueOrFailure();

                (unexpected, absent) = Directory.FindDivergences(sourceOfTruth: expectedStartMenuStateDirectoryState, test: currentStartMenuItemsDirectoryState);
            }
            
            return (unexpected, absent);
        }


        public void FilterOutItemsWithTheSameName(StartMenuShortcutsLocation location, ICollection<RelocatableItem> unexpectedItems, ICollection<RelocatableItem> missingItems)
        {
            var allUnexpectedItems = unexpectedItems.ToArray();
            var allMissingItems = missingItems.ToArray(); 
            
            foreach (RelocatableItem unexpectedItem in allUnexpectedItems)
            {
                foreach (RelocatableItem missingItem in allMissingItems)
                {
                    if (missingItem.Name == unexpectedItem.Name)
                    {
                        //we'll move the unexpected item to take the place of the missing item, since it's target might
                        //be a newer version of the original executable
                        string missingItemPath = missingItem.ParentDirectoryPath;
                        
                        Option<IFileSystemItem> itemToRestore = unexpectedItem.Move(missingItemPath);
                        missingItems.Remove(missingItem);
                        
                        missingItems.Remove(unexpectedItem);
                        unexpectedItems.Remove(unexpectedItem);
                        
                        if (itemToRestore.HasValue)
                        {
                            ItemsToRestore[location].Add(itemToRestore.ValueOrFailure());
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Reconciles unexpectedItems with missing items by looking for items that are shortcuts and getting their target file. Any unexpectedItem
        /// with the same target file as a missingItem is assumed to be that item. When two such matching items are found, they are removed from their
        /// respective lists.
        /// </summary>
        /// <param name="unexpectedItems"></param>
        /// <param name="missingItems"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void FilterOutShortcutsMovedOutOfPosition(ICollection<RelocatableItem> unexpectedItems, ICollection<RelocatableItem> missingItems)
        {
            Action filterOutShortcutItemsMovedOutOfPosition = () =>
            {
                var allUnexpectedItems = unexpectedItems.ToArray();
                var allMissingItems = missingItems.ToArray();
            
                foreach (RelocatableItem unexpectedItem in allUnexpectedItems)
                {
                    if ((unexpectedItem.UnderlyingItem is IFile unexpectedFile) && (unexpectedFile.GetShortcutTarget().HasValue))
                    {
                        FileSystemItem unexpectedFileShortcutTarget = unexpectedFile.GetShortcutTarget().ValueOrFailure();
                    
                        foreach (RelocatableItem missingItem in allMissingItems)
                        {
                            if ((missingItem.UnderlyingItem is IFile missingFile) && (missingFile.GetShortcutTarget().HasValue))
                            {
                                FileSystemItem missingFileShortcutTarget = missingFile.GetShortcutTarget().ValueOrFailure();

                                if (unexpectedFileShortcutTarget.Path == missingFileShortcutTarget.Path)
                                {
                                    unexpectedItems.Remove(unexpectedItem);
                                    missingItems.Remove(missingItem);
                                    break;
                                }
                            }
                        }
                    }
                }
            } ;

            StartSTATask(filterOutShortcutItemsMovedOutOfPosition).Wait();
        }
        
        
        private void RestoreExpectedStartMenuItems(StartMenuShortcutsLocation location)
        {
            foreach (IFileSystemItem item in ItemsToRestore[location])
            {
                IFileSystemItem itemToRestore;
                
                if (item is RelocatableItem relocatableItem)
                {
                    itemToRestore = relocatableItem.UnderlyingItem;
                }
                else
                {
                    itemToRestore = item;
                }
                
                String relativePath = itemToRestore.Path.Substring(GetSavedStartMenuItemsPath(location).Length + 1);
                String restoredPath = Path.Combine(EnvironmentStartMenuItemsPath[location], relativePath);
                restoredPath        = Path.GetDirectoryName(restoredPath); //gets parent's directory
                
                itemToRestore.Copy(restoredPath);
                Log($"Restored an item: Item restored: {itemToRestore.Name}. Restored to location: {restoredPath}.");
            }
            
            ItemsToRestore[location].Clear();

            string GetSavedStartMenuItemsPath(StartMenuShortcutsLocation startMenuShortcutsLocation)
            {
                return (SavedDataService.StartMenuItemsStorage[startMenuShortcutsLocation].Path + @"\Start Menu");
            }
        }
        
        private void QuarantineUnrecognizedStartMenuItems(StartMenuShortcutsLocation location)
        {
            QuarantineDataService.MoveFileSystemItems(QuarantineDataService.StartMenuItemsStorage[location], ItemsToQuarantine[location].ToArray()).Wait();
            
            ItemsToQuarantine[location].Clear();
        }


        private static ICollection<IFileSystemItem> ExtractFlatListOfItems(ICollection<RelocatableItem> items)
        {
            ICollection<IFileSystemItem> flatContents = new HashSet<IFileSystemItem>();

            foreach (var item in items)
            {
                if (item.UnderlyingItem is IFile file)
                {
                    flatContents.Add(file);
                }
                else if (item.UnderlyingItem is IDirectory directory)
                {
                    flatContents.AddAll(directory.GetFlatContents());
                }
            }

            return flatContents;
        }
    }
}
