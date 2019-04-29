using System.Threading;

namespace StartMenuProtector.Control
{
    public class StartMenuSentinel
    {
        private Thread Thread;
        
        public SystemStateService SystemStateService { private get; set; }

        public StartMenuSentinel(SystemStateService service)
        {
            this.SystemStateService = service;
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
            throw new System.NotImplementedException();
        }
    }
}
