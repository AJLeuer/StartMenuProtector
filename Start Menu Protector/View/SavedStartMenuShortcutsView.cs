namespace StartMenuProtector.View
{
    public class SavedStartMenuShortcutsView : StartMenuShortcutsView
    {
        public SavedStartMenuShortcutsView() :
            base()
        {
            SaveButton.IsEnabled = false;
            SaveButton.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}