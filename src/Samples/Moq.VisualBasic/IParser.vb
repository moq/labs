Imports System.Runtime.InteropServices

Interface IParser

    Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean

End Interface