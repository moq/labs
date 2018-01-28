Imports System.Runtime.InteropServices
Imports Moq

Namespace Test

    Public Class Foo

        Public Sub Test()
            Dim parser = Mock.Of(Of IParser)

            parser.Setup(Of TryParse)(AddressOf parser.TryParse) _
                .Returns(Function(input As String, ByRef result As DateTimeOffset) DateTimeOffset.TryParse(input, result))

            Dim expected = DateTimeOffset.Now
            Dim value = expected.ToString("O")
            Dim actual As DateTimeOffset

            Debug.Assert(parser.TryParse(value, actual))
            Debug.Assert(actual = expected)

        End Sub

        Public Sub Test2()
            Dim target = Mock.Of(Of IEnvironment)

            Dim expected = DateTimeOffset.Now
            Dim value = expected.ToString("O")
            Dim actual As DateTimeOffset

            target.Setup(Of TryParse)(Function() AddressOf target.Parser.TryParse) _
                .Returns(Function(input As String, ByRef result As DateTimeOffset) DateTimeOffset.TryParse(input, result))

            Debug.Assert(target.Parser.TryParse(value, actual))
            Debug.Assert(actual = expected)

        End Sub

        Delegate Function TryParse(input As String, ByRef result As DateTimeOffset) As Boolean
    End Class

End Namespace