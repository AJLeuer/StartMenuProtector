using System.Windows;

namespace StartMenuProtector.View
{
    public class ActiveStartMenuShortcutsView : StartMenuShortcutsView
    {
        public ActiveStartMenuShortcutsView() :
            base()
        {
            SetupSaveButton();
        }

        private void SetupSaveButton()
        {
            PrimaryActionButton.Content = "Save";
            PrimaryActionButton.IsEnabled = true;
            PrimaryActionButton.Visibility = Visibility.Visible;
        }
    }
}