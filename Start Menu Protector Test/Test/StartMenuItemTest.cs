using System.Threading;
using System.Windows.Input;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Data;
using StartMenuProtector.View;
using StartMenuProtectorTest.Utility;

namespace StartMenuProtectorTest.Test
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
            var fileMock = new Mock<MockableFile>();
            fileMock.Setup((MockableFile self) => self.PrettyName).Returns("Firefox");
            FileSystemItem mockFile = fileMock.Object;
            var startMenuItem = new StartMenuItem { File = mockFile, MarkedExcludedHandler = (StartMenuItem item) => {} };
            
            startMenuItem.MarkAsRemoved(backspace);

            Assert.AreEqual(shouldResultInMarkingForExclusion, mockFile.MarkedForExclusion);
        }
    }
}