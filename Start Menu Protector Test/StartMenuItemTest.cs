using System.Threading;
using NUnit.Framework;
using StartMenuProtector.View;

namespace StartMenuProtectorTest
{
    public static class StartMenuItemTest
    {
        [Test, Apartment(ApartmentState.STA)]
        public static void StartMenuItemShouldHaveExpectedAttributesWhenSelected()
        {
            var startMenuItem = new StartMenuItem();
            
            startMenuItem.Selected();
            
            Assert.IsTrue(startMenuItem.IsFocused);
            Assert.AreEqual(StartMenuItem.DefaultSelectionTextColor, startMenuItem.TextBlock.Foreground);
        }
    }
}