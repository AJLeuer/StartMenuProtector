using System;
using System.Collections.Generic;
using System.IO;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using static StartMenuProtector.Configuration.FilePaths;
using Directory = StartMenuProtector.Data.Directory;

namespace StartMenuProtector.Control
{
	public enum StartMenuProtectorViewType
	{
		Active,
		Saved,
		Quarantine,
		Excluded
	}

	public enum StartMenuShortcutsLocation
	{
		System,
		User
	}

	public class SystemStateService 
	{
		private Dictionary<StartMenuShortcutsLocation, Directory> osEnvironmentStartMenuItems = null;
		public readonly object OSEnvironmentStartMenuItemsLock = new object();

		public virtual Dictionary<StartMenuShortcutsLocation, Directory> OSEnvironmentStartMenuItems
		{
			get
			{
				lock (OSEnvironmentStartMenuItemsLock)
				{
					if (osEnvironmentStartMenuItems == null)
					{
						LoadSystemAndUserStartMenuItemsFromOSEnvironment();
					}

					return osEnvironmentStartMenuItems;
				}
			}
		}

		public SavedDataService SavedDataService { get; set; }

		public void RestoreStartMenuItem(IFileSystemItem item, StartMenuShortcutsLocation location)
		{
			lock (SavedDataService.StartMenuItemsStorageAccessLock)
			{
				lock (OSEnvironmentStartMenuItemsLock)
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
					String restoredPath = Path.Combine(StartMenuItemsPath[location], relativePath);
					restoredPath        = Path.GetDirectoryName(restoredPath); //gets parent's directory

					itemToRestore.Copy(restoredPath);

					LogManager.Log($"Restored an item: Item restored: {itemToRestore.Name}. Restored to location: {restoredPath}.");
				}
			}

			string GetSavedStartMenuItemsPath(StartMenuShortcutsLocation startMenuShortcutsLocation)
			{
				return (SavedDataService.StartMenuItemsStorage[startMenuShortcutsLocation].Path + @"\Start Menu");
			}
		}

		private void LoadSystemAndUserStartMenuItemsFromOSEnvironment()
		{
			var systemStartMenuItems = new Directory(StartMenuItemsPath[StartMenuShortcutsLocation.System]);
			var userStartMenuItems = new Directory(StartMenuItemsPath[StartMenuShortcutsLocation.User]);

			var startMenuItems = new Dictionary<StartMenuShortcutsLocation, Directory>
			{
				{ StartMenuShortcutsLocation.System, systemStartMenuItems },
				{ StartMenuShortcutsLocation.User, userStartMenuItems }
			};

			osEnvironmentStartMenuItems = startMenuItems;
		}
	}
}