using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtectorTest.Utility;

namespace StartMenuProtectorTest.Test
{
    public static class StartMenuViewControllerTest
    {
        public static SystemStateService MockSystemStateService = new Mock<SystemStateService>().Object;
        public static Mock<StartMenuDataService> DataServiceMock;
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
            DataServiceMock = new Mock<StartMenuDataService>(MockSystemStateService);
            
            SystemProgramsMock.Setup(
                    (self) => self.Contents)
                .Returns(new EnhancedFileSystemInfo[]{});
            
            UserProgramsMock.Setup(
                    (self) => self.Contents)
                .Returns(new EnhancedFileSystemInfo[]{});
            
            DataServiceMock.Setup(
                (self) => self.StartMenuShortcuts)
                .Returns(ActiveProgramShortcuts);

            DataServiceMock
                .Setup((self) => self.SaveProgramShortcuts(It.IsAny<StartMenuShortcutsLocation>(), It.IsAny<IEnumerable<FileSystemInfo>>()));
        }
        
        [Test]
        public static void ShouldSetCurrentShortcutsToUsersWhenUserIsSelected()
        {
            var mockDataService = DataServiceMock.Object;
            var viewController = new GenericStartMenuViewController(mockDataService, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.UpdateCurrentShortcuts(StartMenuShortcutsLocation.System);

            Assert.AreEqual(ActiveProgramShortcuts[StartMenuShortcutsLocation.System], viewController.CurrentShortcutsDirectory);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerShouldSaveUserShortcuts()
        {
            var mockDataService = DataServiceMock.Object;
            var viewController = new ActiveStartMenuViewController(mockDataService, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentShortcuts();
            
            DataServiceMock.Verify((self) => self.SaveProgramShortcuts(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Once());
        }


        [Test]
        public static void SavedStartMenuViewControllerShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            var mockDataService = DataServiceMock.Object;
            var viewController = new SavedStartMenuViewController(mockDataService, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentShortcuts();
            
            DataServiceMock.Verify((self) => self.SaveProgramShortcuts(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Never);
        }
    }
}