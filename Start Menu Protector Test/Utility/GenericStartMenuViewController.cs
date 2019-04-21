using StartMenuProtector.Control;

namespace StartMenuProtectorTest.Utility
{
    public class GenericStartMenuViewController : StartMenuViewController
    {
        public GenericStartMenuViewController(StartMenuDataService startMenuDataService, SystemStateService systemStateService) 
            : base(startMenuDataService, systemStateService)
        {
        }
        
        public override void SaveCurrentShortcuts() { }
    }
}