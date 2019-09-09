using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtector.View;
using StartMenuProtectorTest.Utility;
using static StartMenuProtectorTest.Utility.StartMenuViewControllerTestSetup;

namespace StartMenuProtectorTest.Test 
{
    public static class StartMenuViewControllerTest
    {
        public static SystemStateService MockSystemStateService;
        public static IApplicationStateManager MockApplicationStateManager;
        public static Mock<ActiveDataService> ActiveDataServiceMock;
        public static Mock<SavedDataService>  SavedDataServiceMock;
        public static Mock<IStartMenuItemView> StartMenuItemMock;
        public static Mock<MockableStartMenuDirectory> DirectoryMock;
        
        

        [SetUp]
        public static void Setup()
        {
            MockSystemStateService = new Mock<SystemStateService>().Object;
            MockApplicationStateManager = new Mock<IApplicationStateManager>().Object;
            ActiveDataServiceMock = new Mock<ActiveDataService>(MockSystemStateService, MockApplicationStateManager);
            SavedDataServiceMock = new Mock<SavedDataService>(MockSystemStateService, MockApplicationStateManager);
            StartMenuItemMock = new Mock<IStartMenuItemView>();
            DirectoryMock = new Mock<MockableStartMenuDirectory>();

            ActiveDataServiceMock.Setup(
                (self) => self.GetStartMenuContentDirectory(It.IsAny<StartMenuShortcutsLocation>()))
                .Returns((StartMenuShortcutsLocation location) => { return CreateStubbedStartMenuContentsRetrievalTask(view: StartMenuProtectorViewType.Active, location: location); });

            ActiveDataServiceMock
                .Setup((self) => self.SaveStartMenuItems(It.IsAny<IEnumerable<IFileSystemItem>>(), It.IsAny<StartMenuShortcutsLocation>()));

            ActiveDataServiceMock
                .Setup((self) => self.MoveFileSystemItems(It.IsAny<FileSystemItem>(), It.IsAny<FileSystemItem>()))
                .Returns(Task.Run(()=> { }));
            
            ActiveDataServiceMock
                .Setup((self) => self.GetStartMenuContentsFromAppDataCache(It.IsAny<StartMenuShortcutsLocation>()))
                .Returns((StartMenuShortcutsLocation location) => { return CreateStubbedStartMenuContentsRetrievalTask(view: StartMenuProtectorViewType.Saved, location: location); });
            
            SavedDataServiceMock.Setup(
                    (self) => self.GetStartMenuContentDirectory(It.IsAny<StartMenuShortcutsLocation>()))
                .Returns((StartMenuShortcutsLocation location) => { return CreateStubbedStartMenuContentsRetrievalTask(view: StartMenuProtectorViewType.Saved, location: location); });

            SavedDataServiceMock
                .Setup((self) => self.SaveStartMenuItems(It.IsAny<IEnumerable<IFileSystemItem>>(), It.IsAny<StartMenuShortcutsLocation>()));

            StartMenuItemMock.Setup(
                    (self) => self.File)
                .Returns(DirectoryMock.Object);
        }

        [Test]
        public static void ActiveStartMenuViewControllerShouldSaveUserShortcuts()
        {
            var viewController = new ActiveViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.ExecutePrimaryInteractionAction();
            
            SavedDataServiceMock.Verify((self) => self.SaveStartMenuItems(viewController.StartMenuContents, viewController.StartMenuStartMenuShortcutsLocation), Times.Once());
        }
        
        [Test]
        public static void SavedStartMenuViewControllerShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            var viewController = new SavedViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.ExecutePrimaryInteractionAction();
            
            ActiveDataServiceMock.Verify((self) => self.SaveStartMenuItems(viewController.StartMenuContents, viewController.StartMenuStartMenuShortcutsLocation), Times.Never);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerShouldRequestDataServiceToLoadLatestStartMenuItemsFromOSEnvironmentWhenUserHasNotMadeChanges()
        {
            var viewController = new ActiveViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            viewController.CurrentContentState = ActiveViewController.ContentState.MirroringOSEnvironment;

            viewController.UpdateCurrentShortcuts().Wait();

            ActiveDataServiceMock.Verify((self) => self.GetStartMenuContentDirectory(It.IsAny<StartMenuShortcutsLocation>()), Times.Once);
            ActiveDataServiceMock.Verify((self) => self.GetStartMenuContentsFromAppDataCache(It.IsAny<StartMenuShortcutsLocation>()), Times.Never);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerShouldRequestDataServiceToLoadStartMenuItemsFromAppDataCacheWhenUserHasMadeChanges()
        {
            var viewController = new ActiveViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            viewController.CurrentContentState = ActiveViewController.ContentState.UserChangesPresent;

            viewController.UpdateCurrentShortcuts().Wait();
            
            ActiveDataServiceMock.Verify((self) => self.GetStartMenuContentDirectory(It.IsAny<StartMenuShortcutsLocation>()), Times.Never);
            ActiveDataServiceMock.Verify((self) => self.GetStartMenuContentsFromAppDataCache(It.IsAny<StartMenuShortcutsLocation>()), Times.Once);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerHandleRequestToMoveStartMenuItemShouldSetContentStateToUserChangesPresent()
        {
            var viewController = new ActiveViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            viewController.CurrentContentState = ActiveViewController.ContentState.MirroringOSEnvironment;

            viewController.HandleRequestToMoveStartMenuItem(StartMenuItemMock.Object, StartMenuItemMock.Object).Wait();
                
            Assert.AreEqual(ActiveViewController.ContentState.UserChangesPresent, viewController.CurrentContentState);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerHandleRequestToExcludeStartMenuItemShouldSetContentStateToUserChangesPresent()
        {
            var viewController = new ActiveViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            viewController.CurrentContentState = ActiveViewController.ContentState.MirroringOSEnvironment;

            viewController.HandleRequestToExcludeStartMenuItem();
                
            Assert.AreEqual(ActiveViewController.ContentState.UserChangesPresent, viewController.CurrentContentState);
        }
        
        [Test]
        public static void SavingShouldRevertActiveStartMenuViewControllerCurrentContentStateToMirroringOSEnvironment()
        {
            var viewController = new ActiveViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            viewController.CurrentContentState = ActiveViewController.ContentState.UserChangesPresent;

            viewController.ExecutePrimaryInteractionAction();
                
            Assert.AreEqual(ActiveViewController.ContentState.MirroringOSEnvironment, viewController.CurrentContentState);
        }
    }
}