using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtectorTest.Utility;
using Directory = StartMenuProtector.Data.Directory;

namespace StartMenuProtectorTest.Test
{
    public static class StartMenuDataServiceTest
    {
        public static Mock<SystemStateService> SystemStateControllerMock = new Mock<SystemStateService>();
        public static SystemStateService MockSystemStateService = SystemStateControllerMock.Object;
        
        public static Mock<MockableDirectory> SystemProgramsMock; 
        public static Mock<MockableDirectory> UserProgramsMock;
        
        public static Mock<MockableFile> FileToBeSavedMock;
        public static ICollection<IFileSystemItem> FilesToSave;
        public static Mock<MockableFile> FileToMoveMock;
        public static Mock<MockableDirectory> DestinationDirectoryMock;
        
        public static Dictionary<StartMenuShortcutsLocation, IDirectory> ActiveStartMenuShortcuts;
        public static Dictionary<StartMenuShortcutsLocation, IDirectory> SavedStartMenuShortcuts;

        [SetUp]
        public static void Setup()
        {
            SystemProgramsMock = new Mock<MockableDirectory>();
            UserProgramsMock = new Mock<MockableDirectory>();
            FileToBeSavedMock = new Mock<MockableFile>();
            
            FilesToSave = new List<IFileSystemItem>
            {
                FileToBeSavedMock.Object,
                FileToBeSavedMock.Object,
                FileToBeSavedMock.Object
            };
            
            ActiveStartMenuShortcuts = new Dictionary<StartMenuShortcutsLocation, IDirectory>
            {
                {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
                {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
            };
            
            SavedStartMenuShortcuts = new Dictionary<StartMenuShortcutsLocation, IDirectory>
            {
                {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
                {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
            };
            
            var startMenuShortcutsFromDisk = new Dictionary<StartMenuShortcutsLocation, Directory>
            {
                {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
                {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
            };

            FileToMoveMock = new Mock<MockableFile>();
            DestinationDirectoryMock = new Mock<MockableDirectory>();
            
            SystemProgramsMock.Setup(
                    (self) => self.FullName)
                .Returns("");
            
            UserProgramsMock.Setup(
                    (self) => self.FullName)
                .Returns("");

            SystemProgramsMock.Setup(
                    (self) => self.RefreshContents())
                .Returns(new List<IFileSystemItem>());
            
            UserProgramsMock.Setup(
                    (self) => self.RefreshContents())
                .Returns(new List<IFileSystemItem>());

            SystemProgramsMock.Setup((self) => self.DeleteContents());

            UserProgramsMock.Setup((self) => self.DeleteContents());

            FileToBeSavedMock.Setup(
                (self) => self.Copy(It.IsAny<Directory>()));

            SystemStateControllerMock.Setup(
                    (self) => self.OSEnvironmentStartMenuItems)
                .Returns(startMenuShortcutsFromDisk);

            FileToMoveMock
                .Setup((MockableFile self) => self.Move(It.IsAny<Directory>()));
        }
        
        [Test]
        public static void ActiveStartMenuDataServiceShouldClearOldActiveStartMenuDataWhenStartMenuContentsAreRetrieved()
        {
            SystemProgramsMock.Setup((self) => self.Contents).Returns(new List<IFileSystemItem>());
            UserProgramsMock.Setup((self) => self.Contents).Returns(new List<IFileSystemItem>());
            
            StartMenuDataService service = new ActiveDataService(MockSystemStateService) { StartMenuItemsStorage = ActiveStartMenuShortcuts};
            
            service.GetStartMenuContentDirectory(StartMenuShortcutsLocation.System).Wait();

            SystemProgramsMock.Verify((self) => self.DeleteContents(), Times.Once);
            UserProgramsMock.Verify((self) => self.DeleteContents(), Times.Once);
        }

        [Test]
        public static void ActiveStartMenuDataServiceShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            StartMenuDataService service = new ActiveDataService(MockSystemStateService) { StartMenuItemsStorage = ActiveStartMenuShortcuts};

            service.SaveStartMenuItems(FilesToSave, StartMenuShortcutsLocation.System);
            
            FileToBeSavedMock.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Never);
        }

        [Test]
        public static void SavedStartMenuDataServiceShouldSaveAllRequestedStartMenuShortcuts()
        {
            StartMenuDataService service = new SavedDataService(MockSystemStateService) { StartMenuItemsStorage = SavedStartMenuShortcuts};

            service.SaveStartMenuItems(FilesToSave, StartMenuShortcutsLocation.System);
            
            FileToBeSavedMock.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Exactly(3));
        }
        
        [Test]
        public static void SavedStartMenuDataServiceShouldSaveStartMenuShortcutsFromSpecifiedLocationOnly()
        {
            var service = new SavedDataService(MockSystemStateService) { StartMenuItemsStorage = SavedStartMenuShortcuts};

            service.SaveStartMenuItems(FilesToSave, StartMenuShortcutsLocation.User);
            
            FileToBeSavedMock.Verify((self) => self.Copy(UserProgramsMock.Object), Times.AtLeastOnce);
            FileToBeSavedMock.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Never);
        }
        
        [Test]
        public static void SavedStartMenuDataServiceShouldClearOldStartMenuItemsBeforeSaving()
        {
            StartMenuDataService service = new SavedDataService(MockSystemStateService){ StartMenuItemsStorage = SavedStartMenuShortcuts};
            
            service.SaveStartMenuItems(FilesToSave, StartMenuShortcutsLocation.System);
            
            SystemProgramsMock.Verify((self) => self.DeleteContents());
        }
        
        
        [Test]
        public static void SavedStartMenuDataServiceShouldRefreshStartMenuItemsStorageAfterSaving()
        {
            StartMenuDataService service = new SavedDataService(MockSystemStateService){ StartMenuItemsStorage = SavedStartMenuShortcuts};
            
            service.SaveStartMenuItems(FilesToSave, StartMenuShortcutsLocation.User);
            
            UserProgramsMock.Verify((self) => self.RefreshContents());
        }

        [Test]
        public static void ActiveStartMenuDataServiceShouldMoveItemsWhenRequestedToMoveFileSystemItems()
        {
            SystemProgramsMock.Setup((self) => self.Contains(DestinationDirectoryMock.Object)).Returns(true);
            StartMenuDataService service = new ActiveDataService(MockSystemStateService) { StartMenuItemsStorage = ActiveStartMenuShortcuts};

            Task fileMoveTask = service.MoveFileSystemItems(itemsRequestingMove: FileToMoveMock.Object, destinationItem: DestinationDirectoryMock.Object);
            fileMoveTask.Wait();
            
            FileToMoveMock.Verify((self) => self.Move(DestinationDirectoryMock.Object));
        }
        
        [Test]
        public static void ActiveStartMenuDataServiceShouldRefreshStartMenuItemsStorageWhenRequestedToMoveFileSystemItems()
        {
            SystemProgramsMock.Setup((self) => self.Contains(DestinationDirectoryMock.Object)).Returns(false);
            UserProgramsMock.Setup((self) => self.Contains(DestinationDirectoryMock.Object)).Returns(true);
            StartMenuDataService service = new ActiveDataService(MockSystemStateService) { StartMenuItemsStorage = ActiveStartMenuShortcuts};

            Task fileMoveTask = service.MoveFileSystemItems(destinationItem: DestinationDirectoryMock.Object, itemsRequestingMove: FileToMoveMock.Object);
            fileMoveTask.Wait();
            
            UserProgramsMock.Verify((self) => self.RefreshContents());
        }
        
        [Test]
        public static void SavedStartMenuDataServiceShouldDoNothingWhenRequestedToMoveFileSystemItems()
        {
            StartMenuDataService service = new SavedDataService(MockSystemStateService){ StartMenuItemsStorage = SavedStartMenuShortcuts};

            Task fileMoveTask = service.MoveFileSystemItems(destinationItem: DestinationDirectoryMock.Object, itemsRequestingMove: FileToMoveMock.Object);
            fileMoveTask.Wait();

            FileToMoveMock.Verify((self) => self.Move(DestinationDirectoryMock.Object), Times.Never);
        }
    }
}