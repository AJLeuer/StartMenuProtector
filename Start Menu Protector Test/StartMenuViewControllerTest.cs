using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtectorTest.Utility;

namespace StartMenuProtectorTest
{
    public static class StartMenuViewControllerTest
    {
        public static SystemStateController MockSystemStateController = new Mock<SystemStateController>().Object;
        public static Mock<StartMenuDataController> DataControllerMock;
        public static Mock<MockableEnhancedDirectoryInfo> SystemProgramsMock = new Mock<MockableEnhancedDirectoryInfo>(); 
        public static Mock<MockableEnhancedDirectoryInfo> UserProgramsMock = new Mock<MockableEnhancedDirectoryInfo>();
        
        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ActiveProgramShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
            {
                {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
                {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
            };

        [SetUp]
        public static void Setup()
        {
            DataControllerMock = new Mock<StartMenuDataController>(MockSystemStateController);
            
            SystemProgramsMock.Setup(
                    (self) => self.Contents)
                .Returns(new EnhancedFileSystemInfo[]{});
            
            UserProgramsMock.Setup(
                    (self) => self.Contents)
                .Returns(new EnhancedFileSystemInfo[]{});
            
            DataControllerMock.Setup(
                (self) => self.StartMenuShortcuts)
                .Returns(ActiveProgramShortcuts);

            DataControllerMock
                .Setup((self) => self.SaveProgramShortcuts(It.IsAny<StartMenuShortcutsLocation>(), It.IsAny<IEnumerable<FileSystemInfo>>()));
        }
        
        [Test]
        public static void ShouldSetCurrentShortcutsToUsersWhenUserIsSelected()
        {
            var mockDataController = DataControllerMock.Object;
            var viewController = new GenericStartMenuViewController(mockDataController, MockSystemStateController) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.UpdateCurrentShortcuts(StartMenuShortcutsLocation.System);

            Assert.AreEqual(ActiveProgramShortcuts[StartMenuShortcutsLocation.System], viewController.CurrentShortcutsDirectory);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerShouldSaveUserShortcuts()
        {
            var mockDataController = DataControllerMock.Object;
            var viewController = new ActiveStartMenuViewController(mockDataController, MockSystemStateController) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentShortcuts();
            
            DataControllerMock.Verify((self) => self.SaveProgramShortcuts(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Once());
        }


        [Test]
        public static void SavedStartMenuViewControllerShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            var mockDataController = DataControllerMock.Object;
            var viewController = new SavedStartMenuViewController(mockDataController, MockSystemStateController) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentShortcuts();
            
            DataControllerMock.Verify((self) => self.SaveProgramShortcuts(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Never);
        }
    }
}