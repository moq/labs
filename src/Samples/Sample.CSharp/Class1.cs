using System;
using Moq;

namespace Sample.CSharp
{
    public class Class1
    {
        public void Test()
        {
            var mock = Mock.Of<ICustomFormatter, IDisposable>();


        }
    }
}
