Imports Moq
Imports System.Reflection

Public Class Tests
    Public Sub WhenMockingFormatterThenCanInvokeIt()
        Dim mock1 As ICustomFormatter = Mock.Of(Of ICustomFormatter)(Assembly.GetExecutingAssembly)
        Dim mock2 As ICustomFormatter = Mock.Of(Of ICustomFormatter)(Assembly.GetExecutingAssembly)

        Dim result1 As String = mock1.Format("Hello {0}", "World", Nothing)
        Dim result2 As String = mock2.Format("Hello {0}", "World", Nothing)
    End Sub
End Class
