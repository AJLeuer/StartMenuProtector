using System;
using System.Threading;
using System.Windows.Input;
using System.Windows.Interop;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Data;
using StartMenuProtector.View;
using StartMenuProtectorTest.Utility;

namespace StartMenuProtectorTest.Test
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public static class StartMenuItemViewTest
    {
        [Test]
        public static void StartMenuItemViewShouldHaveExpectedAttributesWhenSelected()
        {
            var startMenuItemView = new StartMenuItemView();
            
            startMenuItemView.Select(null, null);
            
            Assert.AreEqual(StartMenuItemView.DefaultSelectionTextColor, startMenuItemView.TextBlock.Foreground);
        }
        
        [Test]
        [TestCase(Key.Back, true)]
        [TestCase(Key.A, false)]
        [TestCase(Key.Delete, true)]
        public static void StartMenuItemViewShouldMarkFileSystemItemForExclusionWhenDeleteOrBackspaceArePressed(Key pressedKey, bool shouldResultInMarkingForExclusion)
        {
            Key backspace = pressedKey;
            var keyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0, pressedKey);
            var fileMock = new Mock<MockableFile>();
            fileMock.Setup((MockableFile self) => self.PrettyName).Returns("Firefox");
            FileSystemItem mockFile = fileMock.Object;
            var startMenuItemView = new StartMenuItemView { File = mockFile, MarkExcludedCompletedHandler = (StartMenuItemView item) => {} };
            
            startMenuItemView.ProcessKeyboardInput(null, keyEvent);

            Assert.AreEqual(shouldResultInMarkingForExclusion, mockFile.MarkedForExclusion);
        }
    }
}