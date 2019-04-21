using System.Windows;

namespace StartMenuProtector.View
{
    public class SavedStartMenuShortcutsView : StartMenuShortcutsView
    {
        public SavedStartMenuShortcutsView() :
            base()
        {
            SaveButton.IsEnabled = false;
            SaveButton.Visibility = Visibility.Hidden;
        }
    }
}