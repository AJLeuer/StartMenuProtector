using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtector.View;
using StartMenuProtectorTest.Utility;
using static StartMenuProtectorTest.Utility.StartMenuViewControllerTestSetupUtility;

namespace StartMenuProtectorTest.Test
{
    public static class StartMenuViewControllerTest
    {
        public static SystemStateService MockSystemStateService = new Mock<SystemStateService>().Object;
        public static Mock<ActiveStartMenuDataService> ActiveDataServiceMock;
        public static Mock<SavedStartMenuDataService>  SavedDataServiceMock;
        public static Mock<IStartMenuItem> StartMenuItemMock = new Mock<IStartMenuItem>();
        public static Mock<MockableEnhancedDirectoryInfo> DirectoryMock = new Mock<MockableEnhancedDirectoryInfo>();
        
        

        [SetUp]
        public static void Setup()
        {
            ActiveDataServiceMock = new Mock<ActiveStartMenuDataService>(MockSystemStateService);
            SavedDataServiceMock = new Mock<SavedStartMenuDataService>(MockSystemStateService);

            ActiveDataServiceMock.Setup(
                (self) => self.GetStartMenuContents(It.IsAny<StartMenuShortcutsLocation>()))
                .Returns((StartMenuShortcutsLocation location) => { return CreateStubbedStartMenuContentsRetrievalTask(view: StartMenuProtectorViewType.Active, location: location); });

            ActiveDataServiceMock
                .Setup((self) => self.SaveStartMenuItems(It.IsAny<StartMenuShortcutsLocation>(), It.IsAny<IEnumerable<FileSystemInfo>>()));

            ActiveDataServiceMock
                .Setup((self) => self.HandleRequestToMoveFileSystemItems(It.IsAny<EnhancedFileSystemInfo>(), It.IsAny<EnhancedFileSystemInfo>()))
                .Returns(Task.Run(()=> { }));
            
            ActiveDataServiceMock
                .Setup((self) => self.GetStartMenuContentsFromAppDataCache(It.IsAny<StartMenuShortcutsLocation>()))
                .Returns((StartMenuShortcutsLocation location) => { return CreateStubbedStartMenuContentsRetrievalTask(view: StartMenuProtectorViewType.Saved, location: location); });
            
            SavedDataServiceMock.Setup(
                    (self) => self.GetStartMenuContents(It.IsAny<StartMenuShortcutsLocation>()))
                .Returns((StartMenuShortcutsLocation location) => { return CreateStubbedStartMenuContentsRetrievalTask(view: StartMenuProtectorViewType.Saved, location: location); });

            SavedDataServiceMock
                .Setup((self) => self.SaveStartMenuItems(It.IsAny<StartMenuShortcutsLocation>(), It.IsAny<IEnumerable<FileSystemInfo>>()));

            StartMenuItemMock.Setup(
                    (self) => self.File)
                .Returns(DirectoryMock.Object);
        }
        
        [Test]
        public static void ShouldSetCurrentShortcutsToUsersWhenUserIsSelected()
        {
            var activeViewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            var savedViewController  = new SavedStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            activeViewController.StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.System;
            savedViewController.StartMenuStartMenuShortcutsLocation  = StartMenuShortcutsLocation.User;
            
            activeViewController.UpdateCurrentShortcuts().Wait();
            savedViewController.UpdateCurrentShortcuts().Wait();

            CollectionAssert.AreEquivalent(ActiveSystemStartMenuItems, activeViewController.StartMenuContents);
            CollectionAssert.AreEquivalent(SavedUserStartMenuItems,    savedViewController.StartMenuContents);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerShouldSaveUserShortcuts()
        {
            var viewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentStartMenuItems();
            
            SavedDataServiceMock.Verify((self) => self.SaveStartMenuItems(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Once());
        }


        [Test]
        public static void SavedStartMenuViewControllerShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            var viewController = new SavedStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentStartMenuItems();
            
            ActiveDataServiceMock.Verify((self) => self.SaveStartMenuItems(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Never);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerShouldRequestDataServiceToLoadLatestStartMenuItemsFromOSEnvironmentWhenUserHasNotMadeChanges()
        {
            var viewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            viewController.CurrentContentState = ActiveStartMenuViewController.ContentState.MirroringOSEnvironment;

            viewController.UpdateCurrentShortcuts().Wait();

            ActiveDataServiceMock.Verify((self) => self.GetStartMenuContents(It.IsAny<StartMenuShortcutsLocation>()), Times.Once);
            ActiveDataServiceMock.Verify((self) => self.GetStartMenuContentsFromAppDataCache(It.IsAny<StartMenuShortcutsLocation>()), Times.Never);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerShouldRequestDataServiceToLoadStartMenuItemsFromAppDataCacheWhenUserHasMadeChanges()
        {
            var viewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            viewController.CurrentContentState = ActiveStartMenuViewController.ContentState.UserChangesPresent;

            viewController.UpdateCurrentShortcuts().Wait();
            
            ActiveDataServiceMock.Verify((self) => self.GetStartMenuContents(It.IsAny<StartMenuShortcutsLocation>()), Times.Never);
            ActiveDataServiceMock.Verify((self) => self.GetStartMenuContentsFromAppDataCache(It.IsAny<StartMenuShortcutsLocation>()), Times.Once);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerHandleRequestToMoveStartMenuItemShouldSetContentStateToUserChangesPresent()
        {
            var viewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            viewController.CurrentContentState = ActiveStartMenuViewController.ContentState.MirroringOSEnvironment;

            viewController.HandleRequestToMoveStartMenuItem(StartMenuItemMock.Object, StartMenuItemMock.Object).Wait();
                
            Assert.AreEqual(ActiveStartMenuViewController.ContentState.UserChangesPresent, viewController.CurrentContentState);
        }
        
        [Test]
        public static void SavingShouldRevertActiveStartMenuViewControllerCurrentContentStateToMirroringOSEnvironment()
        {
            var viewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            viewController.CurrentContentState = ActiveStartMenuViewController.ContentState.UserChangesPresent;

            viewController.SaveCurrentStartMenuItems();
                
            Assert.AreEqual(ActiveStartMenuViewController.ContentState.MirroringOSEnvironment, viewController.CurrentContentState);
        }
    }
}