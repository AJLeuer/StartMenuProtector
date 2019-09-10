using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StartMenuProtector.Configuration;
using StartMenuProtector.Data;
using static StartMenuProtector.Control.ApplicationStateManager;
using static StartMenuProtector.Util.LogManager;


namespace StartMenuProtector.Control
{
	public class SavedDataService : StartMenuDataService
	{
		public override Dictionary<StartMenuShortcutsLocation, IDirectory> StartMenuItemsStorage { get; set; } = new Dictionary<StartMenuShortcutsLocation, IDirectory>
		{
			{ StartMenuShortcutsLocation.System, FilePaths.SavedSystemStartMenuItems },
			{ StartMenuShortcutsLocation.User,   FilePaths.SavedUserStartMenuItems }
		};

		public override object StartMenuItemsStorageAccessLock { get; } = new object();

		public SavedDataService( SystemStateService systemStateService, IApplicationStateManager applicationStateManager)
			: base(systemStateService, applicationStateManager)
		{
		}

		public override async Task SaveStartMenuItems(IEnumerable<IFileSystemItem> startMenuItems, StartMenuShortcutsLocation location)
		{
			ClearStartMenuItems(location);

			lock (StartMenuItemsStorageAccessLock)
			{
				IDirectory startMenuItemsDirectory = StartMenuItemsStorage[location];

				foreach (var startMenuItem in startMenuItems)
				{
					startMenuItem.Copy(startMenuItemsDirectory);
				}

				Log($"Saved {location.ToString()} start menu items.");
			}

			RefreshStartMenuItems(location);

			ApplicationState currentState = await ApplicationStateManager.RetrieveApplicationState();
			currentState.CurrentSavedStartMenuStates.UserStateCreated[location] = true;
			await ApplicationStateManager.UpdateApplicationState(currentState);
		}

		public override async Task MoveFileSystemItems(IFileSystemItem destinationItem, params IFileSystemItem[] itemsRequestingMove)
		{
			/* Do nothing */
			await Task.Run(() => { });
		}
	}
}