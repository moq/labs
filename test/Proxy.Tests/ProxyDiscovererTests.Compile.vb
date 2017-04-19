Imports System
Imports Moq

Public Class Tests
    Public Sub WhenMockingFormatterThenCanInvokeIt()
        Dim mock1 As ICustomFormatter = Mock.Of(Of ICustomFormatter)
        Dim mock2 As ICustomFormatter = Mock.Of(Of ICustomFormatter)

        Dim result1 As String = mock1.Format("Hello {0}", "World", Nothing)
        Dim result2 As String = mock2.Format("Hello {0}", "World", Nothing)
    End Sub
End Class
