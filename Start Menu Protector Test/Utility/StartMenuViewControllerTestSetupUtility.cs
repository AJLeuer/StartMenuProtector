using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Moq;
using StartMenuProtector.Control;

namespace StartMenuProtectorTest.Utility
{
    public static class StartMenuViewControllerTestSetupUtility
    {
        public static readonly Mock<MockableFile> SystemStartMenuItemMock = new Mock<MockableFile>(); 
        public static readonly Mock<MockableFile> UserStartMenuItemMock   = new Mock<MockableFile>();

        public static readonly ICollection<FileSystemInfo> ActiveSystemStartMenuItems = new List<FileSystemInfo> { SystemStartMenuItemMock.Object, SystemStartMenuItemMock.Object, SystemStartMenuItemMock.Object };
        public static readonly ICollection<FileSystemInfo> ActiveUserStartMenuItems   = new List<FileSystemInfo> { UserStartMenuItemMock.Object,   UserStartMenuItemMock.Object,   UserStartMenuItemMock.Object   };
        public static readonly ICollection<FileSystemInfo> SavedSystemStartMenuItems  = new List<FileSystemInfo> { SystemStartMenuItemMock.Object, SystemStartMenuItemMock.Object, SystemStartMenuItemMock.Object };
        public static readonly ICollection<FileSystemInfo> SavedUserStartMenuItems    = new List<FileSystemInfo> { UserStartMenuItemMock.Object,   UserStartMenuItemMock.Object,   UserStartMenuItemMock.Object   };

        public static Task<ICollection<FileSystemInfo>> CreateStubbedStartMenuContentsRetrievalTask(StartMenuProtectorViewType view, StartMenuShortcutsLocation location)
        {
            switch (view) 
            {
                case StartMenuProtectorViewType.Active:
                {
                    if (location == StartMenuShortcutsLocation.System)
                    {
                        return Task.Run(() =>
                        {
                            return ActiveSystemStartMenuItems;
                        });
                    }
                    else /* if (location == StartMenuShortcutsLocation.User) */
                    {
                        return Task.Run(() =>
                        {
                            return ActiveUserStartMenuItems;
                        });
                    }
                }
                case StartMenuProtectorViewType.Saved:
                {
                    if (location == StartMenuShortcutsLocation.System)
                    {
                        return Task.Run(() =>
                        {
                            return SavedSystemStartMenuItems;
                        });
                    }
                    else /* if (location == StartMenuShortcutsLocation.User) */
                    {
                        return Task.Run(() =>
                        {
                            return SavedUserStartMenuItems;
                        });
                    }
                }
                case StartMenuProtectorViewType.Quarantine:
                {
                    throw new InvalidEnumArgumentException("Quarantine view not implemented yet");
                }
                case StartMenuProtectorViewType.Excluded:
                {
                    throw new InvalidEnumArgumentException("Excluded view not implemented yet");
                }
                default:
                {
                    throw new InvalidEnumArgumentException();
                }
            }
        }
    }
}