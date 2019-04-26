using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtectorTest.Utility;

namespace StartMenuProtectorTest.Test
{
    public static class StartMenuDataServiceTest
    {
        public static Mock<SystemStateService> SystemStateControllerMock = new Mock<SystemStateService>();
        public static SystemStateService MockSystemStateService = SystemStateControllerMock.Object;
        public static Mock<MockableEnhancedDirectoryInfo> SystemProgramsMock = new Mock<MockableEnhancedDirectoryInfo>(); 
        public static Mock<MockableEnhancedDirectoryInfo> UserProgramsMock = new Mock<MockableEnhancedDirectoryInfo>();
        public static Mock<MockableEnhancedFileInfo> FileToBeSavedMock;
        public static ICollection<FileSystemInfo> FilesToSave;
        public static Mock<MockableEnhancedFileInfo> FileToMoveMock;
        public static Mock<MockableEnhancedDirectoryInfo> DestinationDirectoryMock;
        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ActiveStartMenuShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
        {
            {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
            {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
        };
        
        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> SavedStartMenuShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
        {
            {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
            {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
        };

        [SetUp]
        public static void Setup()
        {
            FileToBeSavedMock = new Mock<MockableEnhancedFileInfo>();
            
            FilesToSave = new List<FileSystemInfo>
            {
                FileToBeSavedMock.Object,
                FileToBeSavedMock.Object,
                FileToBeSavedMock.Object
            };
            
            var startMenuShortcutsFromDisk = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
            {
                {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
                {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
            };

            FileToMoveMock = new Mock<MockableEnhancedFileInfo>();
            DestinationDirectoryMock = new Mock<MockableEnhancedDirectoryInfo>();
            
            SystemProgramsMock.Setup(
                    (self) => self.FullName)
                .Returns("");
            
            UserProgramsMock.Setup(
                    (self) => self.FullName)
                .Returns("");
            
            SystemProgramsMock.Setup(
                    (self) => self.Self)
                .Returns((DirectoryInfo) null);
            
            UserProgramsMock.Setup(
                    (self) => self.Self)
                .Returns((DirectoryInfo) null);            
            
            UserProgramsMock.Setup(
                    (self) => self.RefreshContents())
                .Returns(new List<EnhancedFileSystemInfo>());

            SystemProgramsMock.Setup((self) => self.DeleteContents());

            UserProgramsMock.Setup((self) => self.DeleteContents());

            FileToBeSavedMock.Setup(
                (self) => self.Copy(It.IsAny<EnhancedDirectoryInfo>()));

            SystemStateControllerMock.Setup(
                    (self) => self.LoadSystemAndUserStartMenuProgramShortcutsFromDisk())
                .Returns(startMenuShortcutsFromDisk);

            FileToMoveMock
                .Setup((MockableEnhancedFileInfo self) => self.Move(It.IsAny<EnhancedDirectoryInfo>()));
        }
        
        [Test]
        public static void ActiveStartMenuDataServiceShouldClearOldActiveStartMenuDataWhenStartMenuContentsAreRetrieved()
        {
            StartMenuDataService service = new ActiveStartMenuDataService(MockSystemStateService) { StartMenuItemsStorage = ActiveStartMenuShortcuts};
            
            service.GetStartMenuContents(StartMenuShortcutsLocation.System).Wait();

            SystemProgramsMock.Verify((self) => self.DeleteContents(), Times.Once);
            UserProgramsMock.Verify((self) => self.DeleteContents(), Times.Once);
        }

        [Test]
        public static void ActiveStartMenuDataServiceShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            StartMenuDataService service = new ActiveStartMenuDataService(MockSystemStateService) { StartMenuItemsStorage = ActiveStartMenuShortcuts};

            service.SaveStartMenuItems(StartMenuShortcutsLocation.System, FilesToSave);
            
            FileToBeSavedMock.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Never);
        }

        [Test]
        public static void SavedStartMenuDataServiceShouldSaveAllRequestedStartMenuShortcuts()
        {
            StartMenuDataService service = new SavedStartMenuDataService(MockSystemStateService) { StartMenuItemsStorage = SavedStartMenuShortcuts};

            service.SaveStartMenuItems(StartMenuShortcutsLocation.System, FilesToSave);
            
            FileToBeSavedMock.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Exactly(3));
        }
        
        [Test]
        public static void ShouldSaveStartMenuShortcutsFromSpecifiedLocationOnly()
        {
            var service = new SavedStartMenuDataService(MockSystemStateService) { StartMenuItemsStorage = SavedStartMenuShortcuts};

            service.SaveStartMenuItems(StartMenuShortcutsLocation.User, FilesToSave);
            
            FileToBeSavedMock.Verify((self) => self.Copy(UserProgramsMock.Object), Times.AtLeastOnce);
            FileToBeSavedMock.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Never);
        }
        
        
        [Test]
        public static void SavedStartMenuDataServiceShouldRefreshStartMenuItemsStorageAfterSaving()
        {
            StartMenuDataService service = new SavedStartMenuDataService(MockSystemStateService){ StartMenuItemsStorage = SavedStartMenuShortcuts};
            
            service.SaveStartMenuItems(StartMenuShortcutsLocation.User, FilesToSave);
            
            UserProgramsMock.Verify((self) => self.RefreshContents());
        }

        [Test]
        public static void ActiveStartMenuDataServiceShouldMoveItemsWhenRequestedToMoveFileSystemItems()
        {
            StartMenuDataService service = new ActiveStartMenuDataService(MockSystemStateService) { StartMenuItemsStorage = ActiveStartMenuShortcuts};

            Task fileMoveTask = service.HandleRequestToMoveFileSystemItems(itemRequestingMove: FileToMoveMock.Object, destinationItem: DestinationDirectoryMock.Object);
            fileMoveTask.Wait();
            
            FileToMoveMock.Verify((self) => self.Move(DestinationDirectoryMock.Object));
        }
        
        [Test]
        public static void SavedStartMenuDataServiceShouldDoNothingWhenRequestedToMoveFileSystemItems()
        {
            StartMenuDataService service = new SavedStartMenuDataService(MockSystemStateService){ StartMenuItemsStorage = SavedStartMenuShortcuts};

            Task fileMoveTask = service.HandleRequestToMoveFileSystemItems(itemRequestingMove: FileToMoveMock.Object, destinationItem: DestinationDirectoryMock.Object);
            fileMoveTask.Wait();

            FileToMoveMock.Verify((self) => self.Move(DestinationDirectoryMock.Object), Times.Never);
        }
    }
}