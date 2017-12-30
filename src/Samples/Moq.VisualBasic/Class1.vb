Namespace Sample.CSharp

    Public Class Class1

        Public Sub Test()
            Dim formatter As ICustomFormatter = Mock.Of(Of ICustomFormatter, IDisposable)
            Dim foo = Mock.Of(Of IFoo)
            Dim bar = Mock.Of(Of IBar)
            Dim value = "foo"

            foo.Id _
               .Callback(Sub() value = "before") _
               .Returns(Function() value) _
               .Callback(Sub() value = "after") _
               .Returns(Function() value)

            Console.WriteLine(foo.Id)
            Console.WriteLine(foo.Id)
        End Sub

    End Class
End Namespace

Public Interface IBar

    Sub DoBar()

End Interface

Public Interface IFoo

    Sub [Do]()

    Property Id As String

    Property Title As String

End Interface
