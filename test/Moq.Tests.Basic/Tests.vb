Imports Moq.Sdk
Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports Moq.Proxy
Imports System.Runtime.CompilerServices
Imports System.Threading

<CompilerGeneratedAttribute>
Partial Public Class ICalculatorProxy
    Implements IMocked
    Implements IProxy

    Dim pipeline As BehaviorPipeline = New BehaviorPipeline()

    ReadOnly Property Behaviors As IList(Of IProxyBehavior) Implements IProxy.Behaviors
        Get
            Return pipeline.Behaviors
        End Get
    End Property

    Public ReadOnly Property Mock As IMock Implements IMocked.Mock
        Get
            Return LazyInitializer.EnsureInitialized(_mock, (Function() New MockInfo(pipeline.Behaviors)))
        End Get
    End Property

    Dim _mock As IMock
End Class
