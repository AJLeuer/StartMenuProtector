using System.Threading;
using StartMenuProtector.Data;
using static StartMenuProtector.Configuration.Globals;

namespace StartMenuProtector.Control
{
    public class StartMenuSentinel
    {
        public void Start()
        {
            new Thread(this.Run).Start();
        }

        private void Run()
        {
            ArchiveProgramShortcutsDirectories();
        }
        
        public void ArchiveProgramShortcutsDirectories()
        {
            SystemState.ActiveStartMenuShortcuts[StartMenuShortcutsLocation.System].Copy(SystemProgramShortcutsBackup.Self);
            SystemState.ActiveStartMenuShortcuts[StartMenuShortcutsLocation.User].Copy(UserProgramShortcutsBackup.Self);
        }
    }
}
