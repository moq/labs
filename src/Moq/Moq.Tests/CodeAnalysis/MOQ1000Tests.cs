using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Moq.Migration;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Moq.Tests.CodeAnalysis
{
    public class MOQ1000Tests
    {
        private ITestOutputHelper output;

        public MOQ1000Tests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public async Task NewMockTReplacedWithMoqOfT()
        {
            var testCode = @"
using System;
using Moq;

public class Test
{
    public void Do()
    {
        var mock = new Mock<IServiceProvider>();
    }
}";

            var fixedCode = @"
using System;
using Moq;

public class Test
{
    public void Do()
    {
        var mock = Mock.Of<IServiceProvider>();
    }
}";
            await VerifyCodeFixAsync<MOQ1000NewMockT, MOQ1000CodeFixProvider>(
                LanguageNames.CSharp, 
                MOQ1000NewMockT.DiagnosticId,
                testCode, fixedCode);
        }

        [Fact]
        public async Task NewMockTReplacedWithMoqOfTVB()
        {
            var testCode = @"
Imports System
Imports Moq

Public Class Test
    Public Sub Execute()
        Dim obj = new Mock(Of IServiceProvider)()
    End Sub
End Class";

            var fixedCode = @"
Imports System
Imports Moq

Public Class Test
    Public Sub Execute()
        Dim obj = Mock.Of(Of IServiceProvider)()
    End Sub
End Class";

            await VerifyCodeFixAsync<MOQ1000NewMockT, MOQ1000CodeFixProvider>(
                LanguageNames.VisualBasic, 
                MOQ1000NewMockT.DiagnosticId,
                testCode, fixedCode);
        }
    }
}
