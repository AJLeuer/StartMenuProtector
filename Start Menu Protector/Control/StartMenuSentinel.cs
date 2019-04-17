using System.Threading;

namespace StartMenuProtector.Control
{
    public class StartMenuSentinel
    {
        public StartMenuSentinel()
        {
            new Thread(this.Run).Start();
        }

        private void Run()
        {
            
        }
    }
}
