using System.Threading;
using System.Windows.Input;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Data;
using StartMenuProtector.View;
using StartMenuProtectorTest.Data;

namespace StartMenuProtectorTest
{
    public static class StartMenuItemTest
    {
        [Test, Apartment(ApartmentState.STA)]
        public static void StartMenuItemShouldHaveExpectedAttributesWhenSelected()
        {
            var startMenuItem = new StartMenuItem();
            
            startMenuItem.Selected();
            
            Assert.AreEqual(StartMenuItem.DefaultSelectionTextColor, startMenuItem.TextBlock.Foreground);
        }
        
        [Test, Apartment(ApartmentState.STA)]
        [TestCase(Key.Back, true)]
        [TestCase(Key.A, false)]
        [TestCase(Key.Delete, true)]
        public static void StartMenuItemMarkFileSystemItemForExclusionWhenDeleteOrBackspaceArePressed(Key pressedKey, bool shouldResultInMarkingForExclusion)
        {
            Key backspace = pressedKey;
            var fileMock = new Mock<MockableEnhancedFileInfo>();
            fileMock.Setup((MockableEnhancedFileInfo self) => self.PrettyName).Returns("Firefox");
            EnhancedFileSystemInfo mockFile = fileMock.Object;
            var startMenuItem = new StartMenuItem { File = mockFile };
            
            startMenuItem.MarkAsRemoved(backspace);

            Assert.AreEqual(shouldResultInMarkingForExclusion, mockFile.MarkedForExclusion);
        }
    }
}