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

        Delegate Function TryParse(input As String, ByRef result As DateTimeOffset) As Boolean
    End Class

End Namespace