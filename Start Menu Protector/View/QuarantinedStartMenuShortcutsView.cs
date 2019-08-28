using System.Windows;

namespace StartMenuProtector.View
{
    public class QuarantinedStartMenuShortcutsView : StartMenuShortcutsView
    {
        public QuarantinedStartMenuShortcutsView() :
            base()
        {
            SetupRestoreButton();
        }

        private void SetupRestoreButton()
        {
            PrimaryActionButton.Content = "Restore";
            PrimaryActionButton.IsEnabled = true;
            PrimaryActionButton.Visibility = Visibility.Visible;
        }
    }
}