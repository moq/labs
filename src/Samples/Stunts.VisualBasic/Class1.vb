Public Class Class1
    Public Sub New()
        Dim stunt1 As ICustomFormatter = Stunt.Of(Of ICustomFormatter)
        Dim stunt2 As ICustomFormatter = Stunt.Of(Of ICustomFormatter)

        Dim result1 As String = stunt1.Format("Hello {0}", "World", Nothing)
        Dim result2 As String = stunt2.Format("Hello {0}", "World", Nothing)

        Dim calculator As ICalculator = CType(Stunt.Of(Of CalculatorBase), ICalculator)

    End Sub
End Class
