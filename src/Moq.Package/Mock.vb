Option Strict On

Imports System
Imports System.Diagnostics.CodeAnalysis
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Moq.Sdk

Namespace Global.Moq

    <ExcludeFromCodeCoverage>
    <CompilerGenerated>
    Partial Friend Class Mock

        Private Shared Function Create(Of T As Class)(ByVal behavior As MockBehavior, ByVal constructorArgs As Object(), ParamArray interfaces As Type()) As T
            Dim mocked = DirectCast(MockFactory.[Default].CreateMock(GetType(Mock).GetTypeInfo().Assembly, GetType(T), interfaces, constructorArgs), IMocked)

            mocked.Initialize(behavior)

            Return DirectCast(mocked, T)
        End Function

    End Class

End Namespace