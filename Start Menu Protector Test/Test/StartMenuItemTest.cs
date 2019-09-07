using Moq;
using NUnit.Framework;
using StartMenuProtector.Data;
using StartMenuProtector.ViewModel;
using StartMenuProtectorTest.Utility;

namespace StartMenuProtectorTest.Test
{
	public static class StartMenuItemTest
	{
		/* partially mocking the class under test (usually discouraged) in order to smooth out some of the complications
		/  that come along with filesystem IO */
		private static Mock<MockableStartMenuDirectory> DirectoryPartialMock;

		[SetUp]
		public static void Setup()
		{
			DirectoryPartialMock = GeneralTestSetup.CreateStartMenuDirectoryMockWithContents();
		}

		[Test]
		public static void DirectorySelectionShouldPropogateToAllStartMenuViewsRecursively()
		{
			StartMenuDirectory directory = DirectoryPartialMock.Object;

			directory.IsSelected = true;
			
			foreach (IFileSystemItem item in directory.GetFlatContents())
			{
				if (item is IStartMenuItem startMenuItem)
				{
					Assert.True(startMenuItem.IsSelected);
				}
			}
		}
		
		[Test]
		public static void DirectoryDeselectionShouldPropogateToAllStartMenuViewsRecursively()
		{
			StartMenuDirectory directory = DirectoryPartialMock.Object;

			directory.IsSelected = false;
			
			foreach (IFileSystemItem item in directory.GetFlatContents())
			{
				if (item is IStartMenuItem startMenuItem)
				{
					Assert.False(startMenuItem.IsSelected);
				}
			}
		}
		
		
	}
}