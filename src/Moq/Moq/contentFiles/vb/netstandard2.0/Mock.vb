Option Strict On

Imports System.Reflection
Imports Moq.Sdk

Namespace Global.Moq

    Partial Friend Class Mock

        Private Shared Function Create(Of T)(ByVal behavior As MockBehavior, ByVal constructorArgs As Object(), ParamArray interfaces As Type()) As T
            Dim mocked = DirectCast(MockFactory.[Default].CreateMock(GetType(Mock).GetTypeInfo().Assembly, GetType(T), interfaces, constructorArgs), IMocked)

            mocked.SetBehavior(behavior)

            Return DirectCast(mocked, T)
        End Function

    End Class

End Namespace