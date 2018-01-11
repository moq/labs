using Xunit;

namespace Moq.Tests.Recursive
{
    public class RecursiveMocks
    {

        [Fact(Skip = "Pending support for recursive mocks")]
        public void CanSetupRecursiveMock()
        {
            var mock = Mock.Of<IRecursiveRoot>();

            mock.Branch.Leaf.Name = "foo";

            mock.Setup(m => m.Branch.Leaf.Name).Returns("foo");

            Assert.Equal("foo", mock.Branch.Leaf.Name);
        }
    }

    public interface IRecursiveRoot
    {
        IRecursiveBranch Branch { get; }
    }

    public interface IRecursiveBranch
    {
        IRecursiveLeaf Leaf { get; }
    }

    public interface IRecursiveLeaf
    {
        string Name { get; set; }
    }
}
