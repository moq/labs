Option Strict On

Imports System
Imports System.CodeDom.Compiler
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Moq.Sdk

Namespace Global.Moq

    <GeneratedCode("Moq", "5.0")>
    <CompilerGenerated>
    Partial Friend Class Mock

        Private Shared Function Create(Of T)(ByVal behavior As MockBehavior, ByVal constructorArgs As Object(), ParamArray interfaces As Type()) As T
            Dim mocked = DirectCast(MockFactory.[Default].CreateMock(GetType(Mock).GetTypeInfo().Assembly, GetType(T), interfaces, constructorArgs), IMocked)

            mocked.Initialize(behavior)

            Return DirectCast(mocked, T)
        End Function

    End Class

End Namespace