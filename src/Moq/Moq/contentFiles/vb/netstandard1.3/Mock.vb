Imports System.Reflection
Imports Moq.Sdk
Imports Stunts

Namespace Global.Moq

    Partial Friend Class Mock

        Private Shared Function Create(Of T)(ByVal behavior As MockBehavior, ByVal constructorArgs As Object(), ParamArray interfaces As Type()) As T
            Dim mocked = DirectCast(MockFactory.[Default].CreateMock(GetType(Mock).GetTypeInfo().Assembly, GetType(T), interfaces, constructorArgs), IMocked)

            mocked.Mock.Behaviors.Add(New MockTrackingBehavior())

            If behavior = MockBehavior.Strict Then
                mocked.Mock.Behaviors.Add(New StrictMockBehavior())
            Else
                mocked.Mock.Behaviors.Add(New DefaultValueBehavior())
            End If

            mocked.Mock.Behaviors.Add(New DefaultEqualityBehavior())

            Return DirectCast(mocked, T)
        End Function

    End Class

End Namespace