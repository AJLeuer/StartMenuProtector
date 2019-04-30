using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Data;

namespace StartMenuProtectorTest.Test
{
    public static class FileSystemItemTest
    {
        public static Mock<IDirectory> OriginalDirectoryMock = new Mock<IDirectory>(); // /Directory1A
        public static Mock<IDirectory> TestDirectoryMock     = new Mock<IDirectory>(); // /Directory1A

        public static Mock<IDirectory> SubDirectoryInBothOriginalVersionMock = new Mock<IDirectory>(); // /SubDirectory1A
        public static Mock<IDirectory> SubDirectoryInBothTestVersionMock     = new Mock<IDirectory>(); // /SubDirectory1A
        public static Mock<IDirectory> SubDirectoryInOriginalOnlyMock        = new Mock<IDirectory>(); // /SubDirectory2
        public static Mock<IDirectory> SubDirectoryInTestOnlyMock            = new Mock<IDirectory>(); // /SubDirectoryB

        public static Mock<IFile> FileMock               = new Mock<IFile>(); // /File1A
        public static Mock<IFile> FileOnlyInOriginalMock = new Mock<IFile>(); // /File2
        public static Mock<IFile> FileOnlyInTestMock     = new Mock<IFile>(); // /FileB

        private static void SetupDirectoryMockWithContents(Mock<IDirectory> directoryMock, params IFileSystemItem[] items)
        {
            var files = new List<IFile>();
            var directories = new List<IDirectory>();

            foreach (var item in items)
            {
                if (item is IFile file)
                {
                    files.Add(file);
                }
                else if (item is IDirectory directory)
                {
                    directories.Add(directory);
                }
            }
            
            directoryMock.Setup((IDirectory self) => self.Contents)   .Returns(items.ToList());
            directoryMock.Setup((IDirectory self) => self.Directories).Returns(directories);
            directoryMock.Setup((IDirectory self) => self.Files)      .Returns(files);
        }
        
        [SetUp]
        public static void Setup()
        {
            FileMock              .Setup((IFile self) => self.Path).Returns("/Directory1A/SubDirectory1A/File1A");
            FileOnlyInOriginalMock.Setup((IFile self) => self.Path).Returns("/Directory1A/SubDirectory1A/File2");
            FileOnlyInTestMock    .Setup((IFile self) => self.Path).Returns("/Directory1A/SubDirectory1A/FileB");
            
            SetupDirectoryMockWithContents(SubDirectoryInBothOriginalVersionMock, FileMock.Object, FileOnlyInOriginalMock.Object);
            SetupDirectoryMockWithContents(SubDirectoryInBothTestVersionMock,     FileMock.Object, FileOnlyInTestMock.Object);

            SubDirectoryInBothOriginalVersionMock.Setup((IDirectory self) => self.Path).Returns("/Directory1A/SubDirectory1A/");
            SubDirectoryInBothTestVersionMock    .Setup((IDirectory self) => self.Path).Returns("/Directory1A/SubDirectory1A/");

            SubDirectoryInOriginalOnlyMock.Setup((IDirectory self) => self.Path).Returns("/Directory1A/SubDirectory2/");
            SubDirectoryInTestOnlyMock    .Setup((IDirectory self) => self.Path).Returns("/Directory1A/SubDirectoryB/");

            SetupDirectoryMockWithContents(OriginalDirectoryMock, SubDirectoryInBothOriginalVersionMock.Object, SubDirectoryInOriginalOnlyMock.Object);
            SetupDirectoryMockWithContents(TestDirectoryMock,     SubDirectoryInBothTestVersionMock.Object,     SubDirectoryInTestOnlyMock.Object);

            OriginalDirectoryMock.Setup((IDirectory self) => self.Path).Returns("/Directory1A/");
            TestDirectoryMock    .Setup((IDirectory self) => self.Path).Returns("/Directory1A/");
            
            OriginalDirectoryMock                .Setup((IDirectory self) => self.Equals(It.IsAny<IFileSystemItem>())).Returns((IFileSystemItem other) => FileSystemItem.AreEqual(OriginalDirectoryMock.Object, other));
            TestDirectoryMock                    .Setup((IDirectory self) => self.Equals(It.IsAny<IFileSystemItem>())).Returns((IFileSystemItem other) => FileSystemItem.AreEqual(TestDirectoryMock.Object, other));
            SubDirectoryInBothOriginalVersionMock.Setup((IDirectory self) => self.Equals(It.IsAny<IFileSystemItem>())).Returns((IFileSystemItem other) => FileSystemItem.AreEqual(SubDirectoryInBothOriginalVersionMock.Object, other));
            SubDirectoryInBothTestVersionMock    .Setup((IDirectory self) => self.Equals(It.IsAny<IFileSystemItem>())).Returns((IFileSystemItem other) => FileSystemItem.AreEqual(SubDirectoryInBothTestVersionMock.Object, other));
            SubDirectoryInOriginalOnlyMock       .Setup((IDirectory self) => self.Equals(It.IsAny<IFileSystemItem>())).Returns((IFileSystemItem other) => FileSystemItem.AreEqual(SubDirectoryInOriginalOnlyMock.Object, other));
            SubDirectoryInTestOnlyMock           .Setup((IDirectory self) => self.Equals(It.IsAny<IFileSystemItem>())).Returns((IFileSystemItem other) => FileSystemItem.AreEqual(SubDirectoryInTestOnlyMock.Object, other));
            FileMock                             .Setup((IFile self)      => self.Equals(It.IsAny<IFileSystemItem>())).Returns((IFileSystemItem other) => FileSystemItem.AreEqual(FileMock.Object, other));
            FileOnlyInOriginalMock               .Setup((IFile self)      => self.Equals(It.IsAny<IFileSystemItem>())).Returns((IFileSystemItem other) => FileSystemItem.AreEqual(FileOnlyInOriginalMock.Object, other));
            FileOnlyInTestMock                   .Setup((IFile self)      => self.Equals(It.IsAny<IFileSystemItem>())).Returns((IFileSystemItem other) => FileSystemItem.AreEqual(FileOnlyInTestMock.Object, other));
        }

        [Test]
        public static void FindDivergencesShouldFindAnyRemovedItems()
        {
            (_, ICollection<RelocatableItem> removed) = Directory.FindDivergences(sourceOfTruth: OriginalDirectoryMock.Object, test: TestDirectoryMock.Object);
            
            Assert.That(removed, Has.Exactly(1).Matches(
                (RelocatableItem item) => (item.OriginalPath == SubDirectoryInOriginalOnlyMock.Object.Path)));
            
            Assert.That(removed, Has.Exactly(1).Matches(
                (RelocatableItem item) => (item.OriginalPath == FileOnlyInOriginalMock.Object.Path)));
        }
        
        [Test]
        public static void FindDivergencesShouldFindAnyAddedItems()
        {
            (ICollection<RelocatableItem> added, _) = Directory.FindDivergences(sourceOfTruth: OriginalDirectoryMock.Object, test: TestDirectoryMock.Object);
            
            Assert.That(added, Has.Exactly(1).Matches(
                (RelocatableItem item) => (item.OriginalPath == SubDirectoryInTestOnlyMock.Object.Path)));
            
            Assert.That(added, Has.Exactly(1).Matches(
                (RelocatableItem item) => (item.OriginalPath == FileOnlyInTestMock.Object.Path)));
        }
    }
}