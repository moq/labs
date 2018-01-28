Imports System.Runtime.InteropServices

Public Interface IEnvironment
    ReadOnly Property Parser As IParser
End Interface

Public Interface IParser
    Function TryParse(ByVal input As String, <Out> ByRef result As DateTimeOffset) As Boolean
End Interface