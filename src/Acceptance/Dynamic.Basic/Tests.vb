Option Strict On
Option Infer On
Imports System.Runtime.InteropServices
Imports Dynamic.Basic.Model
Imports Moq
Imports Xunit

Public Class Tests
    <Fact>
    Public Sub StubProperties()
        Dim calc = Mock.[Of](Of ICalculator)()
        calc.Mode = CalculatorMode.Scientific
        Assert.Equal(CalculatorMode.Scientific, calc.Mode)
        calc.Mode = CalculatorMode.Standard
        Assert.Equal(CalculatorMode.Standard, calc.Mode)
    End Sub

    <Fact(Skip:="Fluent recursive without setups not supported yet.")>
    Public Sub Recusive()
        Dim calc = Mock.[Of](Of ICalculator)()
        calc.Memory.Recall().Returns(5)
        Assert.Equal(5, calc.Memory.Recall())
    End Sub

    <Fact>
    Public Sub RecusiveSetupScope()
        Dim calc = Mock.[Of](Of ICalculator)()

        Using calc.Setup()
            calc.Memory.Recall().Returns(5)
        End Using

        Assert.Equal(5, calc.Memory.Recall())
    End Sub

    <Fact>
    Public Sub RecusiveSetup()
        Dim calc = Mock.[Of](Of ICalculator, IDisposable)()
        calc.Setup(Function(m) m.Memory.Recall()).Returns(5)
        Assert.Equal(5, calc.Memory.Recall())
        Assert.IsAssignableFrom(Of IDisposable)(calc)
    End Sub

End Class
