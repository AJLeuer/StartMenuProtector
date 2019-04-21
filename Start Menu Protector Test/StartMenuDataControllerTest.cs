using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtectorTest.Utility;

namespace StartMenuProtectorTest
{
    public static class StartMenuDataControllerTest
    {
        public static Mock<SystemStateController> SystemStateControllerMock = new Mock<SystemStateController>();
        public static SystemStateController MockSystemStateController = SystemStateControllerMock.Object;
        public static Mock<MockableEnhancedDirectoryInfo> SystemProgramsMock = new Mock<MockableEnhancedDirectoryInfo>(); 
        public static Mock<MockableEnhancedDirectoryInfo> UserProgramsMock = new Mock<MockableEnhancedDirectoryInfo>();
        public static Mock<MockableEnhancedFileInfo> FileToBeSavedMock;
        public static ICollection<FileSystemInfo> FilesToSave;
        public static Mock<MockableEnhancedFileInfo> FileToMoveMock;
        public static Mock<MockableEnhancedDirectoryInfo> DestinationDirectoryMock;

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
            
            StartMenuDataController.SavedStartMenuShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
            {
                {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
                {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
            };
            
            StartMenuDataController.ActiveStartMenuShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
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
        public static void ActiveStartMenuDataControllerShouldClearOldActiveStartMenuDataOnStartup()
        {
            var controller = new ActiveStartMenuDataController(MockSystemStateController);
            
            Task.Run(async () => { await controller.LoadCurrentStartMenuData();}).Wait();
            
            //ActiveStartMenuController also creates a new thread to run LoadCurrentStartMenuData automatically,
            //so we may see DeleteContents() called more than once
            SystemProgramsMock.Verify((self) => self.DeleteContents(), Times.AtLeastOnce);
            UserProgramsMock.Verify((self) => self.DeleteContents(), Times.AtLeastOnce);
        }

        [Test]
        public static void ActiveStartMenuDataControllerShouldSaveAllRequestedStartMenuShortcuts()
        {
            var controller = new ActiveStartMenuDataController(MockSystemStateController);

            controller.SaveProgramShortcuts(StartMenuShortcutsLocation.System, FilesToSave);
            
            FileToBeSavedMock.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Exactly(3));
        }
        
        [Test]
        public static void ShouldSaveStartMenuShortcutsFromSpecifiedLocationOnly()
        {
            var controller = new ActiveStartMenuDataController(MockSystemStateController);

            controller.SaveProgramShortcuts(StartMenuShortcutsLocation.User, FilesToSave);
            
            FileToBeSavedMock.Verify((self) => self.Copy(UserProgramsMock.Object), Times.AtLeastOnce);
            FileToBeSavedMock.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Never);
        }
        
        [Test]
        public static void SavedStartMenuDataControllerShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            var controller = new SavedStartMenuDataController(MockSystemStateController);

            controller.SaveProgramShortcuts(StartMenuShortcutsLocation.System, FilesToSave);
            
            FileToBeSavedMock.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Never);
        }
        
        [Test]
        public static void ActiveStartMenuDataControllerShouldMoveItemsWhenRequestedToMoveFileSystemItems()
        {
            var controller = new ActiveStartMenuDataController(MockSystemStateController);

            Task fileMoveTask = controller.HandleRequestToMoveFileSystemItems(itemRequestingMove: FileToMoveMock.Object, destinationItem: DestinationDirectoryMock.Object);
            fileMoveTask.Wait();
            
            FileToMoveMock.Verify((self) => self.Move(DestinationDirectoryMock.Object));
        }
        
        [Test]
        public static void SavedStartMenuDataControllerShouldDoNothingWhenRequestedToMoveFileSystemItems()
        {
            var controller = new SavedStartMenuDataController(MockSystemStateController);

            Task fileMoveTask = controller.HandleRequestToMoveFileSystemItems(itemRequestingMove: FileToMoveMock.Object, destinationItem: DestinationDirectoryMock.Object);
            fileMoveTask.Wait();

            FileToMoveMock.Verify((self) => self.Move(DestinationDirectoryMock.Object), Times.Never);
        }
    }
}