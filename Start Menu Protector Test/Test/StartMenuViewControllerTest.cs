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
        public static Mock<ActiveStartMenuDataService>    ActiveDataServiceMock;
        public static Mock<SavedStartMenuDataService>     SavedDataServiceMock;
        public static Mock<MockableEnhancedDirectoryInfo> SystemProgramsMock = new Mock<MockableEnhancedDirectoryInfo>(); 
        public static Mock<MockableEnhancedDirectoryInfo> UserProgramsMock = new Mock<MockableEnhancedDirectoryInfo>();
        
        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ActiveProgramShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
            {
                {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
                {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
            };
        
        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> SavedProgramShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
        {
            {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
            {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
        };

        [SetUp]
        public static void Setup()
        {
            ActiveDataServiceMock = new Mock<ActiveStartMenuDataService>(MockSystemStateService);
            SavedDataServiceMock = new Mock<SavedStartMenuDataService>(MockSystemStateService);
            
            SystemProgramsMock.Setup(
                    (self) => self.Contents)
                .Returns(new EnhancedFileSystemInfo[]{});
            
            UserProgramsMock.Setup(
                    (self) => self.Contents)
                .Returns(new EnhancedFileSystemInfo[]{});
            
            ActiveDataServiceMock.Setup(
                (self) => self.StartMenuShortcuts)
                .Returns(ActiveProgramShortcuts);

            ActiveDataServiceMock
                .Setup((self) => self.SaveStartMenuItems(It.IsAny<StartMenuShortcutsLocation>(), It.IsAny<IEnumerable<FileSystemInfo>>()));
        
            SavedDataServiceMock.Setup(
                (self) => self.StartMenuShortcuts)
                .Returns(SavedProgramShortcuts);

            SavedDataServiceMock
                .Setup((self) => self.SaveStartMenuItems(It.IsAny<StartMenuShortcutsLocation>(), It.IsAny<IEnumerable<FileSystemInfo>>()));
        }
        
        [Test]
        public static void ShouldSetCurrentShortcutsToUsersWhenUserIsSelected()
        {
            var activeViewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            var savedViewController  = new SavedStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
                
            activeViewController.UpdateCurrentShortcuts(StartMenuShortcutsLocation.System);
            savedViewController.UpdateCurrentShortcuts(StartMenuShortcutsLocation.User);

            Assert.AreEqual(ActiveProgramShortcuts[StartMenuShortcutsLocation.System], activeViewController.CurrentShortcutsDirectory);
            Assert.AreEqual(ActiveProgramShortcuts[StartMenuShortcutsLocation.User],   savedViewController.CurrentShortcutsDirectory);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerShouldSaveUserShortcuts()
        {
            var viewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentShortcuts();
            
            SavedDataServiceMock.Verify((self) => self.SaveStartMenuItems(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Once());
        }


        [Test]
        public static void SavedStartMenuViewControllerShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            var mockDataService = ActiveDataServiceMock.Object;
            var viewController = new SavedStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentShortcuts();
            
            ActiveDataServiceMock.Verify((self) => self.SaveStartMenuItems(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Never);
        }
    }
}