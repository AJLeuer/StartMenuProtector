using StartMenuProtector.Control;

namespace StartMenuProtectorTest.Utility
{
    public class GenericStartMenuViewController : StartMenuViewController
    {
        public GenericStartMenuViewController(StartMenuDataController startMenuDataController, SystemStateController systemStateController) 
            : base(startMenuDataController, systemStateController)
        {
        }
        
        public override void SaveCurrentShortcuts() { }
    }
}