using System.Threading;
using System.Windows.Input;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Data;
using StartMenuProtector.View;
using StartMenuProtectorTest.Data;

namespace StartMenuProtectorTest
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public static class StartMenuItemTest
    {
        [Test]
        public static void StartMenuItemShouldHaveExpectedAttributesWhenSelected()
        {
            var startMenuItem = new StartMenuItem();
            
            startMenuItem.Select(null, null);
            
            Assert.AreEqual(StartMenuItem.DefaultSelectionTextColor, startMenuItem.TextBlock.Foreground);
        }
        
        [Test]
        [TestCase(Key.Back, true)]
        [TestCase(Key.A, false)]
        [TestCase(Key.Delete, true)]
        public static void StartMenuItemShouldMarkFileSystemItemForExclusionWhenDeleteOrBackspaceArePressed(Key pressedKey, bool shouldResultInMarkingForExclusion)
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