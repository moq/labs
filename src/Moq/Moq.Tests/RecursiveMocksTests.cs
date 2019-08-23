using Xunit;
using static Moq.Syntax;

namespace Moq.Tests.Recursive
{
    public class RecursiveMocksTests
    {
        [Fact]
        public void CanSetupRecursiveMockProperty()
        {
            var mock = Mock.Of<IRecursiveRoot>(MockBehavior.Loose);

            mock.Setup(m => m.Branch.Leaf.Name).Returns("foo");

            Assert.Equal("foo", mock.Branch.Leaf.Name);
        }

        [Fact]
        public void CanSetupRecursiveMockMethod()
        {
            var mock = Mock.Of<IRecursiveRoot>(MockBehavior.Loose);
            
            mock.Setup(m => m.Branch.GetLeaf(1).Name).Returns("foo");

            Assert.Equal("foo", mock.Branch.GetLeaf(1).Name);
            Assert.Null(mock.Branch.GetLeaf(0));
        }

        [Fact]
        public void CanSetupRecursiveMockMethodInSetupScope()
        {
            var mock = Mock.Of<IRecursiveRoot>(MockBehavior.Loose);

            using (Setup())
            {
                mock.Branch.GetLeaf(1).Name.Returns("foo");
            }

            Assert.Equal("foo", mock.Branch.GetLeaf(1).Name);
            Assert.Null(mock.Branch.GetLeaf(0));
        }
    }

    public interface IRecursiveRoot
    {
        IRecursiveBranch Branch { get; }
    }

    public interface IRecursiveBranch
    {
        IRecursiveLeaf Leaf { get; }

        IRecursiveLeaf GetLeaf(int index);
    }

    public interface IRecursiveLeaf
    {
        string Name { get; set; }
    }
}
