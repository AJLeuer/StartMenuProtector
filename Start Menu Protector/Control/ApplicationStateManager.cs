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

		public ApplicationStateManager()
		{
			CreateSavedStateFile();
		}

		private void CreateSavedStateFile()
		{
			lock (ApplicationStateStreamLock)
			{
				if (File.Exists(ApplicationStateFilePath) == false)
				{
					using var applicationStateStream = new FileStream(ApplicationStateFilePath, FileMode.Create);
					//then set default state
					string newApplicationStateSerialized = JsonConvert.SerializeObject(new ApplicationState());
					applicationStateStream.Overwrite(newApplicationStateSerialized);
				}
			}
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
		
		private void WriteApplicationState(ApplicationState applicationState)
		{
			using (var applicationStateStream = new FileStream(ApplicationStateFilePath, FileMode.Open))
			{
				ApplicationState newApplicationState = Merge(GetSavedApplicationState(applicationStateStream), applicationState);
				string newApplicationStateSerialized = JsonConvert.SerializeObject(newApplicationState);
				applicationStateStream.Overwrite(newApplicationStateSerialized);
			}
		}

		private ApplicationState GetSavedApplicationState()
		{
			ApplicationState state;

			using (var applicationStateStream = new FileStream(ApplicationStateFilePath, FileMode.Open))
			{
				state = GetSavedApplicationState(applicationStateStream);
			}

			return state;
		}

		private ApplicationState GetSavedApplicationState(Stream applicationStateStream)
		{
			ApplicationState state = JsonConvert.DeserializeObject<ApplicationState>(applicationStateStream.ConvertToString());

			return state;
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