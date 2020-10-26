Option Strict On
Imports System.Runtime.InteropServices

Namespace Model

    Public Interface ICalculator
        Event TurnedOn As EventHandler
        ReadOnly Property IsOn As Boolean
        Property Mode As CalculatorMode
        Function Add(x As Integer, y As Integer) As Integer
        Function Add(x As Integer, y As Integer, z As Integer) As Integer
        Function TryAdd(ByRef x As Integer, ByRef y As Integer, <Out> ByRef z As Integer) As Boolean
        Sub TurnOn()
        Default Property Item(name As String) As Integer?
        Sub Store(name As String, value As Integer)
        Function Recall(name As String) As Integer?
        Sub Clear(name As String)
        ReadOnly Property Memory As ICalculatorMemory
    End Interface

    Public Enum CalculatorMode
        Standard
        Scientific
    End Enum

    Public Interface ICalculatorMemory
        Sub Add(value As Integer)
        Sub Subtract(value As Integer)
        Sub Clear()
        Function Recall() As Integer
    End Interface

End Namespace
