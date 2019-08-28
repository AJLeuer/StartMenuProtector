using System.Windows;

namespace StartMenuProtector.View
{
    public class ActiveStartMenuShortcutsView : StartMenuShortcutsView
    {
        public ActiveStartMenuShortcutsView() :
            base()
        {
            PrimaryActionButton.IsEnabled = true;
            PrimaryActionButton.Visibility = Visibility.Visible;
        }
    }
}