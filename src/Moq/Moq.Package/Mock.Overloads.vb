Imports System.Runtime.CompilerServices

Namespace Global.Moq

    Partial Friend Class Mock

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class)(ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs)
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1)(ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs, GetType(T1))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2)(ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs, GetType(T1), GetType(T2))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3)(ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs, GetType(T1), GetType(T2), GetType(T3))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4)(ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4, T5)(ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4), GetType(T5))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4, T5, T6)(ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4), GetType(T5), GetType(T6))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4, T5, T6, T7)(ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4), GetType(T5), GetType(T6), GetType(T7))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4, T5, T6, T7, T8)(ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4), GetType(T5), GetType(T6), GetType(T7), GetType(T8))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class)(ByVal behavior As MockBehavior, ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(behavior, constructorArgs)
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1)(ByVal behavior As MockBehavior, ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(behavior, constructorArgs, GetType(T1))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2)(ByVal behavior As MockBehavior, ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(behavior, constructorArgs, GetType(T1), GetType(T2))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3)(ByVal behavior As MockBehavior, ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(behavior, constructorArgs, GetType(T1), GetType(T2), GetType(T3))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4)(ByVal behavior As MockBehavior, ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(behavior, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4, T5)(ByVal behavior As MockBehavior, ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(MockBehavior.Loose, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4), GetType(T5))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4, T5, T6)(ByVal behavior As MockBehavior, ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(behavior, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4), GetType(T5), GetType(T6))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4, T5, T6, T7)(ByVal behavior As MockBehavior, ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(behavior, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4), GetType(T5), GetType(T6), GetType(T7))
        End Function

        <MockGenerator>
        <CompilerGenerated>
        Public Shared Function [Of](Of T As Class, T1, T2, T3, T4, T5, T6, T7, T8)(ByVal behavior As MockBehavior, ParamArray constructorArgs As Object()) As T
            Return Create(Of T)(behavior, constructorArgs, GetType(T1), GetType(T2), GetType(T3), GetType(T4), GetType(T5), GetType(T6), GetType(T7), GetType(T8))
        End Function

    End Class

End Namespace