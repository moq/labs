using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Sample;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Moq.Tests.RefOut
{
    public class RefOutTests
    {
        ITestOutputHelper output;

        public RefOutTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void CanUseRefOut()
        {
            var mock = Mock.Of<ICalculator>();
            int x = 10;
            int y = 20;
            int z;

            mock.TryAdd(ref x, ref y, out z)
                .Returns(true);

            Assert.True(mock.TryAdd(ref x, ref y, out z));
        }

        [Fact]
        public void CanSetRefOutReturns()
        {
            var mock = Mock.Of<ICalculator>();
            int x = 10;
            int y = 20;
            int z;

            mock.TryAdd(ref x, ref y, out z)
                .Returns(c =>
                {
                    c[2] = (int)c[0] + (int)c[1];
                    c[0] = 15;
                    c[1] = 25;
                    return true;
                });

            Assert.True(mock.TryAdd(ref x, ref y, out z));
            Assert.Equal(15, x);
            Assert.Equal(25, y);
            Assert.Equal(30, z);
        }

        [Fact]
        public void CanSetTypedOut()
        {
            var mock = Mock.Of<ICalculator>();

            mock.Setup<TryAdd>(mock.TryAdd)
                .Returns((ref int x, ref int y, out int z) => (z = x + y) == z);

            var x1 = 10;
            var y1 = 20;
            int z1;

            Assert.True(mock.TryAdd(ref x1, ref y1, out z1));
            Assert.Equal(30, z1);
        }

        [Fact]
        public void CanSetTypedOutInRecursiveMock()
        {
            var mock = Mock.Of<IRefOutParent>();
            var expected = DateTimeOffset.Now;
            var value = expected.ToString("O");

            mock.Setup<TryParse>(() => mock.RefOut.TryParse)
                .Returns((string input, out DateTimeOffset date) => DateTimeOffset.TryParse(value, out date));

            Assert.True(mock.RefOut.TryParse(value, out var actual));
            Assert.Equal(expected, actual);
        }

        delegate bool TryParse(string input, out DateTimeOffset date);

        delegate bool TryAdd(ref int x, ref int y, out int z);

        [Fact]
        public async Task AddsCSharpDelegate()
        {
            var initial = @"namespace Test
{
    using System;
    using Moq;

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IParser>();

            mock.Setup(mock.TryParse);
        }
    }
}";
            var expected = @"namespace Test
{
    using System;
    using Moq;

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IParser>();

            mock.Setup<TryParse>(mock.TryParse)
                .Returns((string input, out DateTimeOffset date) => throw null);
        }

        delegate bool TryParse(string input, out DateTimeOffset date);
    }
}";

            await ApplyCodeFixAsync(LanguageNames.CSharp, initial, expected);
        }

        [Fact]
        public async Task ReusesCSharpDelegate()
        {
            var initial = @"namespace Test
{
    using System;
    using Moq;

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IParser>();

            mock.Setup(mock.TryParse);
        }

        delegate bool TryParse(string input, out DateTimeOffset date);
    }
}";
            var expected = @"namespace Test
{
    using System;
    using Moq;

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IParser>();

            mock.Setup<TryParse>(mock.TryParse)
                .Returns((string input, out DateTimeOffset date) => throw null);
        }

        delegate bool TryParse(string input, out DateTimeOffset date);
    }
}";

            await ApplyCodeFixAsync(LanguageNames.CSharp, initial, expected);
        }

        [Fact]
        public async Task AddsCSharpDelegateWithFullName()
        {
            var initial = @"namespace Test
{
    using System;
    using Moq;

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IParser>();

            mock.Setup(mock.TryParse);
        }

        delegate bool TryParse(string input, out bool result);
    }
}";
            var expected = @"namespace Test
{
    using System;
    using Moq;

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IParser>();

            mock.Setup<IParserTryParse>(mock.TryParse)
                .Returns((string input, out DateTimeOffset date) => throw null);
        }

        delegate bool IParserTryParse(string input, out DateTimeOffset date);

        delegate bool TryParse(string input, out bool result);
    }
}";

            await ApplyCodeFixAsync(LanguageNames.CSharp, initial, expected);
        }

        [Fact]
        public async Task AddsSetupAndPreservesReturns()
        {
            var initial = @"namespace Test
{
    using System;
    using Moq;

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IParser>();

            mock.Setup(mock.TryParse)
                .Returns((Func<ISetup<TryParse>>)null);
        }
    }
}";
            var expected = @"namespace Test
{
    using System;
    using Moq;

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IParser>();

            mock.Setup<TryParse>(mock.TryParse)
                .Returns((Func<ISetup<TryParse>>)null);
        }

        delegate bool TryParse(string input, out DateTimeOffset date);
    }
}";

            await ApplyCodeFixAsync(LanguageNames.CSharp, initial, expected);
        }

        [Fact]
        public async Task AddsCSharpDelegateForRecursiveMock()
        {
            var initial = @"namespace Test
{
    using System;
    using Moq;

    public interface IEnvironment
    {
        IParser Parser { get; }
    }

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IEnvironment>();

            mock.Setup(mock.Parser.TryParse);
        }
    }
}";
            var expected = @"namespace Test
{
    using System;
    using Moq;

    public interface IEnvironment
    {
        IParser Parser { get; }
    }

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IEnvironment>();

            mock.Setup<TryParse>(() => mock.Parser.TryParse)
                .Returns((string input, out DateTimeOffset date) => throw null);
        }

        delegate bool TryParse(string input, out DateTimeOffset date);
    }
}";

            await ApplyCodeFixAsync(LanguageNames.CSharp, initial, expected);
        }

        [Fact]
        public async Task AddsCSharpDelegateForRecursiveMockLambda()
        {
            var initial = @"namespace Test
{
    using System;
    using Moq;

    public interface IEnvironment
    {
        IParser Parser { get; }
    }

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IEnvironment>();

            mock.Setup(() => mock.Parser.TryParse);
        }
    }
}";
            var expected = @"namespace Test
{
    using System;
    using Moq;

    public interface IEnvironment
    {
        IParser Parser { get; }
    }

    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }

    public class Foo 
    {
        public void Test()
        {
            var mock = Mock.Of<IEnvironment>();

            mock.Setup<TryParse>(() => mock.Parser.TryParse)
                .Returns((string input, out DateTimeOffset date) => throw null);
        }

        delegate bool TryParse(string input, out DateTimeOffset date);
    }
}";

            await ApplyCodeFixAsync(LanguageNames.CSharp, initial, expected);
        }

        [Fact]
        public async Task AddsVisualBasicDelegate()
        {
            var initial = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test

    Interface IParser

        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean

    End Interface

    Public Class Foo

        Public Sub Test()
            Dim parser = Mock.Of(Of IParser)()

            parser.Setup(AddressOf parser.TryParse)
        End Sub
    End Class

End Namespace
";
            var expected = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test

    Interface IParser

        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean

    End Interface

    Public Class Foo

        Public Sub Test()
            Dim parser = Mock.Of(Of IParser)()

            parser.Setup(Of TryParse)(AddressOf parser.TryParse) _
                .Returns(Function(input As String, ByRef result As DateTimeOffset)
                             Throw New NotImplementedException
                         End Function)
        End Sub

        Delegate Function TryParse(input As String, ByRef result As DateTimeOffset) As Boolean
    End Class

End Namespace
";

            await ApplyCodeFixAsync(LanguageNames.VisualBasic, initial, expected);
        }

        [Fact]
        public async Task AddsVisualBasicDelegateForRecursiveMock()
        {
            var initial = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test
    Public Interface IEnvironment
        ReadOnly Property Parser As IParser
    End Interface

    Interface IParser
        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean
    End Interface

    Public Class Foo
        Public Sub Test()
            Dim target = Mock.Of(Of IEnvironment)()

            target.Setup(AddressOf target.Parser.TryParse)
        End Sub
    End Class
End Namespace
";
            var expected = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test
    Public Interface IEnvironment
        ReadOnly Property Parser As IParser
    End Interface

    Interface IParser
        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean
    End Interface

    Public Class Foo
        Public Sub Test()
            Dim target = Mock.Of(Of IEnvironment)()

            target.Setup(Of TryParse)(Function() AddressOf target.Parser.TryParse) _
                .Returns(Function(input As String, ByRef result As DateTimeOffset)
                             Throw New NotImplementedException
                         End Function)
        End Sub

        Delegate Function TryParse(input As String, ByRef result As DateTimeOffset) As Boolean
    End Class
End Namespace
";

            await ApplyCodeFixAsync(LanguageNames.VisualBasic, initial, expected);
        }

        [Fact]
        public async Task AddsVisualBasicDelegateForRecursiveMockFunction()
        {
            var initial = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test
    Public Interface IEnvironment
        ReadOnly Property Parser As IParser
    End Interface

    Interface IParser
        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean
    End Interface

    Public Class Foo
        Public Sub Test()
            Dim target = Mock.Of(Of IEnvironment)()

            target.Setup(Function() AddressOf target.Parser.TryParse)
        End Sub
    End Class
End Namespace
";
            var expected = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test
    Public Interface IEnvironment
        ReadOnly Property Parser As IParser
    End Interface

    Interface IParser
        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean
    End Interface

    Public Class Foo
        Public Sub Test()
            Dim target = Mock.Of(Of IEnvironment)()

            target.Setup(Of TryParse)(Function() AddressOf target.Parser.TryParse) _
                .Returns(Function(input As String, ByRef result As DateTimeOffset)
                             Throw New NotImplementedException
                         End Function)
        End Sub

        Delegate Function TryParse(input As String, ByRef result As DateTimeOffset) As Boolean
    End Class
End Namespace
";

            await ApplyCodeFixAsync(LanguageNames.VisualBasic, initial, expected);
        }

        [Fact]
        public async Task ReusesVisualBasicDelegate()
        {
            var initial = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test

    Interface IParser

        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean

    End Interface

    Public Class Foo

        Public Sub Test()
            Dim parser = Mock.Of(Of IParser)()

            parser.Setup(AddressOf parser.TryParse)
        End Sub

        Delegate Function TryParse(input As String, ByRef result As DateTimeOffset) As Boolean
    End Class

End Namespace
";
            var expected = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test

    Interface IParser

        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean

    End Interface

    Public Class Foo

        Public Sub Test()
            Dim parser = Mock.Of(Of IParser)()

            parser.Setup(Of TryParse)(AddressOf parser.TryParse) _
                .Returns(Function(input As String, ByRef result As DateTimeOffset)
                             Throw New NotImplementedException
                         End Function)
        End Sub

        Delegate Function TryParse(input As String, ByRef result As DateTimeOffset) As Boolean
    End Class

End Namespace
";

            await ApplyCodeFixAsync(LanguageNames.VisualBasic, initial, expected);
        }

        [Fact]
        public async Task AddsVisualBasicDelegateWithFullName()
        {
            var initial = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test

    Interface IParser

        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean

    End Interface

    Public Class Foo

        Public Sub Test()
            Dim parser = Mock.Of(Of IParser)()

            parser.Setup(AddressOf parser.TryParse)
        End Sub

        Delegate Function TryParse(input As String, ByRef result As Boolean) As Boolean
    End Class

End Namespace
";
            var expected = @"Imports System
Imports System.Runtime.InteropServices
Imports Moq

Namespace Test

    Interface IParser

        Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean

    End Interface

    Public Class Foo

        Public Sub Test()
            Dim parser = Mock.Of(Of IParser)()

            parser.Setup(Of IParserTryParse)(AddressOf parser.TryParse) _
                .Returns(Function(input As String, ByRef result As DateTimeOffset)
                             Throw New NotImplementedException
                         End Function)
        End Sub

        Delegate Function IParserTryParse(input As String, ByRef result As DateTimeOffset) As Boolean
        Delegate Function TryParse(input As String, ByRef result As Boolean) As Boolean
    End Class

End Namespace
";

            await ApplyCodeFixAsync(LanguageNames.VisualBasic, initial, expected);
        }

        async Task ApplyCodeFixAsync(string language, string initial, string expected, bool trace = false)
        {
            var provider = new CustomDelegateCodeFix();
            var (workspace, project) = CreateWorkspaceAndProject(language, includeMockApi: true);
            var code = initial;
            var doc = project.AddDocument("code.cs", SourceText.From(code));
            var compilation = await doc.Project.GetCompilationAsync(TimeoutToken(4));
            var diagnostic = compilation.GetDiagnostics(TimeoutToken(5))
                .First(d => provider.FixableDiagnosticIds.Any(fixable => d.Id == fixable));

            var actions = new List<CodeAction>();
            var context = new CodeFixContext(doc, diagnostic, (a, d) => actions.Add(a), TimeoutToken(5));
            await provider.RegisterCodeFixesAsync(context);

            var changed = actions
                .SelectMany(x => x.GetOperationsAsync(TimeoutToken(2)).Result)
                .OfType<ApplyChangesOperation>()
                .First()
                .ChangedSolution
                .GetDocument(doc.Id);

            var expectedDoc = project.AddDocument("expected.cs", SourceText.From(expected));
            var expectedRoot = await expectedDoc.GetSyntaxRootAsync();
            var expectedNode = Formatter.Format(expectedRoot, expectedDoc.Project.Solution.Workspace);
            expected = expectedNode.GetText().ToString();

            var actualRoot = await changed.GetSyntaxRootAsync();
            var actual = Formatter.Format(actualRoot, changed.Project.Solution.Workspace).GetText();

            if (trace)
                output.WriteLine(actual.ToString());
            else if (Debugger.IsAttached)
                output.WriteLine(actual.ToString());

            SyntaxTree actualTree;
            SyntaxTree expectedTree;

            if (language == LanguageNames.CSharp)
            {
                actualTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(actual);
                expectedTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(expected);
            }
            else
            {
                actualTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(actual);
                expectedTree = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(expected);
            }

            Assert.True(actualTree.IsEquivalentTo(expectedTree, true), $@"Expected: 
{expected}
---------------------------------------------------------------------------------
Actual:
{actual}");

            compilation = await changed.Project.GetCompilationAsync(TimeoutToken(4));
            var diagnostics = compilation.GetDiagnostics(TimeoutToken(5))
                .Where(d => provider.FixableDiagnosticIds.Any(fixable => d.Id == fixable)).ToArray();

            Assert.True(diagnostics.Length == 0, 
                string.Join(Environment.NewLine, diagnostics.Select(d => d.GetMessage())));
        }
    }

    public interface IRefOutParent
    {
        IRefOut RefOut { get; }
    }

    public interface IRefOut
    {
        bool TryParse(string input, out DateTimeOffset date);
    }
}
