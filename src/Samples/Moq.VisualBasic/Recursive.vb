Imports Sample
Imports Moq

Public Class Recursive

    Public Sub TestRecursive()
        Dim m As CalculatorBase = Mock.Of(Of CalculatorBase, IDisposable)

        m.Setup(Function(x) x.Memory.Recall()).Returns(5)

    End Sub

End Class