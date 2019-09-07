using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Moq;
using NUnit.Framework;
using StartMenuProtector.View;
using StartMenuProtector.ViewModel;

namespace StartMenuProtectorTest.Test
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public static class StartMenuItemViewTest
    {
        [Test]
        public static void ShouldHaveExpectedAttributesWhenSelected()
        {
            var startMenuItemView = new StartMenuItemView();
            
            startMenuItemView.Select(null, null);
            
            Assert.AreEqual(StartMenuItemView.DefaultSelectionTextColor, startMenuItemView.TextBlock.Foreground);
        }
        
        [Test]
        [TestCase(Key.Back, true)]
        [TestCase(Key.A, false)]
        [TestCase(Key.Delete, true)]
        public static void ShouldMarkFileSystemItemForExclusionWhenDeleteOrBackspaceArePressed(Key pressedKey, bool shouldResultInMarkingForExclusion)
        {
            var keyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0, pressedKey);
            
            var fileMock = new Mock<IStartMenuItem>();
            fileMock.Setup((IStartMenuItem self) => self.PrettyName).Returns("Firefox");
            fileMock.SetupProperty((IStartMenuItem self) => self.MarkedForExclusion);
            IStartMenuItem mockFile = fileMock.Object;
            
            
            var startMenuItemView = new StartMenuItemView { File = mockFile, MarkExcludedCompletedHandler = (StartMenuItemView item) => {} };
            
            startMenuItemView.ProcessKeyboardInput(null, keyEvent);

            Assert.AreEqual(shouldResultInMarkingForExclusion, mockFile.MarkedForExclusion);
        }

        [Test]
        public static void ShouldDisplayAsSelectedWhenOwnFileIsSelected()
        {
            var startMenuFileMock = new Mock<IStartMenuItem>();
            var startMenuItemView = new StartMenuItemView { File = startMenuFileMock.Object };
            startMenuItemView.Selected = false;
                
            startMenuFileMock.Raise((IStartMenuItem self) => self.Selected += null, new RoutedEventArgs());
            
            Assert.IsTrue(startMenuItemView.Selected);
        }
        
        [Test]
        public static void ShouldNotDisplayAsSelectedWhenOwnFileIsDeselected()
        {
            var startMenuFileMock = new Mock<IStartMenuItem>();
            var startMenuItemView = new StartMenuItemView { File = startMenuFileMock.Object };
            startMenuItemView.Selected = true;
            
            startMenuFileMock.Raise((IStartMenuItem self) => self.Deselected += null, new RoutedEventArgs());
            
            Assert.IsFalse(startMenuItemView.Selected);
        }
    }
}