using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;

namespace StartMenuProtectorTest
{
    public static class StartMenuViewControllerTest
    {
        [Test]
        public static void ShouldSetCurrentShortcutsToUsersWhenUserIsSelected()
        {
            var controller = new StartMenuViewController {CurrentShortcutsDirectory = SystemState.ActiveStartMenuShortcuts[StartMenuShortcutsLocation.User]};

            controller.UpdateCurrentShortcuts(StartMenuShortcutsLocation.System);

            Assert.AreEqual(SystemState.ActiveStartMenuShortcuts[StartMenuShortcutsLocation.System],
                controller.CurrentShortcutsDirectory);
        }
    }
}