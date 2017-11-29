Imports System
Imports Stunts

Public Class Tests
    Public Sub WhenFakingFormatterThenCanInvokeIt()
        Dim stunt1 As ICustomFormatter = Stunt.Of(Of ICustomFormatter)
        Dim stunt2 As ICustomFormatter = Stunt.Of(Of ICustomFormatter)

        Dim result1 As String = stunt1.Format("Hello {0}", "World", Nothing)
        Dim result2 As String = stunt2.Format("Hello {0}", "World", Nothing)
    End Sub
End Class
