using System;
using static Moq.Syntax;

namespace Moq.CodeAnalysis.UnitTests.MOQ001.Fixed
{
    public class MultipleDiagnosticSameMethod
    {
        public void Test()
        {
            var mock = Mock.Of<IServiceProvider>();

            using (Setup())
            {
                mock.GetService(typeof(IFormatProvider)).Returns(default(IFormatProvider));
            }

            using (Setup())
            {
                mock.GetService(typeof(IFormattable)).Returns(default(IFormattable));
            }
        }
    }
}
