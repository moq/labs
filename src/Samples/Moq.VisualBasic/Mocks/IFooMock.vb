﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports Moq.VisualBasic
Imports System.Threading
Imports Moq.Sdk
Imports System
Imports System.Collections.ObjectModel
Imports System.Reflection
Imports Stunts
Imports System.Runtime.CompilerServices

Namespace Global.Mocks

    Public Partial Class IFooMock
        Implements IFoo, IStunt, IMocked

        ReadOnly pipeline As BehaviorPipeline = New BehaviorPipeline()
        Dim _mock As IMock

        <CompilerGenerated>
        ReadOnly Property Behaviors As ObservableCollection(Of IStuntBehavior) Implements IStunt.Behaviors
            Get
                Return pipeline.Behaviors
            End Get
        End Property

        ReadOnly Property Mock As IMock Implements IMocked.Mock
            Get
                Return LazyInitializer.EnsureInitialized(_mock, (Function() New MockInfo(Me)))
            End Get
        End Property

        <CompilerGenerated>
        Public Property Id As String Implements IFoo.Id
            Get
                Return pipeline.Execute(Of String)(New MethodInvocation(Me, MethodBase.GetCurrentMethod()))
            End Get
            Set(value As String)
                pipeline.Execute(New MethodInvocation(Me, MethodBase.GetCurrentMethod(), value))
            End Set
        End Property

        <CompilerGenerated>
        Public Property Title As String Implements IFoo.Title
            Get
                Return pipeline.Execute(Of String)(New MethodInvocation(Me, MethodBase.GetCurrentMethod()))
            End Get
            Set(value As String)
                pipeline.Execute(New MethodInvocation(Me, MethodBase.GetCurrentMethod(), value))
            End Set
        End Property

        <CompilerGenerated>
        Public Sub [Do]() Implements IFoo.Do
            pipeline.Execute(New MethodInvocation(Me, MethodBase.GetCurrentMethod()))
        End Sub

        <CompilerGenerated>
        Public Overrides Function ToString() As String
            Return pipeline.Execute(Of String
            )(New MethodInvocation(Me, MethodBase.GetCurrentMethod()))
        End Function

        <CompilerGenerated>
        Public Overrides Function Equals(obj As Object) As Boolean
            Return pipeline.Execute(Of Boolean
            )(New MethodInvocation(Me, MethodBase.GetCurrentMethod(), obj))
        End Function

        <CompilerGenerated>
        Public Overrides Function GetHashCode() As Integer
            Return pipeline.Execute(Of Integer
            )(New MethodInvocation(Me, MethodBase.GetCurrentMethod()))
        End Function
    End Class
End Namespace
