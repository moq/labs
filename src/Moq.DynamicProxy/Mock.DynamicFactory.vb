Namespace Global.Moq
    Partial Friend Class Mock
        Shared Sub New()
            Sdk.MockFactory.[Default] = New Sdk.DynamicMockFactory
            OnInitialized()
        End Sub

        ''' <summary>
        ''' Invoked after the default <see cref="MockFactory.Default"/> 
        ''' is initialized.
        ''' </summary>
        Partial Private Shared Sub OnInitialized()

        End Sub
    End Class
End Namespace