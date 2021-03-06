using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using static StartMenuProtector.Util.Util;


namespace StartMenuProtector.Control
{
	public abstract class StartMenuDataService
	{
		public SystemStateService SystemStateService { get; set; }
		public IApplicationStateManager ApplicationStateManager { get; set; }

		public abstract Dictionary<StartMenuShortcutsLocation, IDirectory> StartMenuItemsStorage { get; set; }
		public abstract Object StartMenuItemsStorageAccessLock { get; }

		public StartMenuDataService(SystemStateService systemStateService, IApplicationStateManager applicationStateManager)
		{
			this.SystemStateService = systemStateService;
			this.ApplicationStateManager = applicationStateManager;
		}

		public virtual async Task<IDirectory> GetStartMenuContentDirectory(StartMenuShortcutsLocation location)
		{
			IDirectory startMenuContents = await Task.Run(() =>
			{
				/* In the the saved data service, unlike the active one, we don't clear the old contents in AppData(since that
				 happens only when saving new contents, and we also don't load from the OS environments start menu state, since 
				 our state is determined entirely by the user. So the saved data service uses this default implementation, the 
				 active overrides it */
				Dictionary<StartMenuShortcutsLocation, IDirectory> startMenuContentsFromAppData = LoadStartMenuContentsFromAppDataDiskStorageToMemory().Result;
				return startMenuContentsFromAppData[location];
			});

			return startMenuContents;
		}

		public abstract Task SaveStartMenuItems(IEnumerable<IFileSystemItem> startMenuItems, StartMenuShortcutsLocation location);

		protected async Task<Dictionary<StartMenuShortcutsLocation, IDirectory>> LoadStartMenuContentsFromAppDataDiskStorageToMemory()
		{
			await Task.Run(RefreshAllStartMenuItems);

			return StartMenuItemsStorage;
		}

		public abstract Task MoveFileSystemItems(IFileSystemItem destinationItem, params IFileSystemItem[] itemsRequestingMove);

		public void RefreshStartMenuItems(StartMenuShortcutsLocation location)
		{
			lock (StartMenuItemsStorageAccessLock)
			{
				IDirectory startMenuItemsDirectory = StartMenuItemsStorage[location];
				startMenuItemsDirectory.RefreshContents();
			}
		}

		public void RefreshAllStartMenuItems()
		{
			GetEnumValues<StartMenuShortcutsLocation>().ForEach(RefreshStartMenuItems);
		}

		protected void ClearStartMenuItems(StartMenuShortcutsLocation location)
		{
			bool clearSuccessful = false;

			lock (StartMenuItemsStorageAccessLock)
			{
				while (clearSuccessful != true)
				{
					try
					{
						IDirectory startMenuItemsDirectory = StartMenuItemsStorage[location];
						startMenuItemsDirectory.DeleteContents();
						clearSuccessful = true;
					}
					catch (IOException)
					{
						LogManager.Log("Unable to clear start menu items, retrying");
					}
				}
			}
		}

		protected async Task ClearAllStartMenuItems()
		{
			await Task.Run(() =>
			{
				ClearStartMenuItems(StartMenuShortcutsLocation.System);
				ClearStartMenuItems(StartMenuShortcutsLocation.User);
			});
		}

		protected IDirectory FindRootStartMenuItemsStorageDirectoryForItem(IFileSystemItem item)
		{
			if (StartMenuItemsStorage[StartMenuShortcutsLocation.System].Contains(item))
			{
				return StartMenuItemsStorage[StartMenuShortcutsLocation.System];
			}
			else if (StartMenuItemsStorage[StartMenuShortcutsLocation.User].Contains(item))
			{
				return StartMenuItemsStorage[StartMenuShortcutsLocation.User];
			}
			else
			{
				throw new ArgumentException("File system item not found in Saved Start Menu items");
			}
		}
	}
}