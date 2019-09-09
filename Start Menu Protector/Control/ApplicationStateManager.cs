using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StartMenuProtector.Util;
using static StartMenuProtector.Configuration.FilePaths;
using static StartMenuProtector.Control.ApplicationStateManager;

namespace StartMenuProtector.Control
{
	public interface IApplicationStateManager
	{
		Task<ApplicationState> RetrieveApplicationState();
		Task UpdateApplicationState(ApplicationState applicationState);
	}

	public class ApplicationStateManager : IApplicationStateManager
	{
		public class ApplicationState
		{
			public class SavedStartMenuStates
			{
				public Dictionary<StartMenuShortcutsLocation, bool> UserStateCreated { get; set; } = new Dictionary<StartMenuShortcutsLocation, bool>
				{
					{ StartMenuShortcutsLocation.System, false },
					{ StartMenuShortcutsLocation.User,   false }
				};
			}
			
			public SavedStartMenuStates CurrentSavedStartMenuStates { get; set; } = new SavedStartMenuStates();
		}
		
		private readonly object ApplicationStateStreamLock = new object();

		private ApplicationState CurrentApplicationState
		{
			get { return GetSavedApplicationState(); }
			set { WriteApplicationState(value); }
		}
		
		public async Task<ApplicationState> RetrieveApplicationState()
		{
			return await Task.Run(() =>
			{
				ApplicationState applicationState;
				
				lock (ApplicationStateStreamLock)
				{
					applicationState = CurrentApplicationState;
				}

				return applicationState;
			});
		}

		public async Task UpdateApplicationState(ApplicationState applicationState)
		{
			await Task.Run(() =>
			{
				lock (ApplicationStateStreamLock)
				{
					CurrentApplicationState = applicationState;
				}
			});
		}

		private ApplicationState GetSavedApplicationState()
		{
			ApplicationState state;

			bool savedStateFileExists = File.Exists(ApplicationStateFilePath);
			FileMode fileMode = savedStateFileExists ? FileMode.Open : FileMode.Create;

			using (var applicationStateStream = new FileStream(ApplicationStateFilePath, fileMode))
			{
				if (savedStateFileExists == false)
				{
					//then set default state
					string newApplicationStateSerialized = JsonConvert.SerializeObject(new ApplicationState());
					applicationStateStream.Overwrite(newApplicationStateSerialized);
				}
				
				state = GetSavedApplicationState(applicationStateStream);
			}

			return state;
		}

		private ApplicationState GetSavedApplicationState(FileStream applicationStateStream)
		{
			ApplicationState state = JsonConvert.DeserializeObject<ApplicationState>(applicationStateStream.ConvertToString());

			return state;
		}

		private void WriteApplicationState(ApplicationState applicationState)
		{
			using (var applicationStateStream = new FileStream(ApplicationStateFilePath, FileMode.Open))
			{
				ApplicationState newApplicationState = Merge(GetSavedApplicationState(applicationStateStream), applicationState);
				string newApplicationStateSerialized = JsonConvert.SerializeObject(newApplicationState);
				applicationStateStream.Overwrite(newApplicationStateSerialized);
			}
		}

		public static ApplicationState Merge(ApplicationState currentState, ApplicationState newState)
		{
			string currentStateSerialized = JsonConvert.SerializeObject(currentState);
			string newStateSerialized = JsonConvert.SerializeObject(newState);
			
			JObject currentStateJSON = JObject.Parse(currentStateSerialized);
			JObject newStateJSON = JObject.Parse(newStateSerialized);
			
			currentStateJSON.Merge(newStateJSON, new JsonMergeSettings
			{
				MergeArrayHandling = MergeArrayHandling.Union
			});

			currentStateSerialized = currentStateJSON.ToString();

			return JsonConvert.DeserializeObject<ApplicationState>(currentStateSerialized);
		}
	}
}