using Microsoft.CodeAnalysis;

namespace Moq.Proxy
{
    public static class ITypeSymbolExtensions
    {
        public static bool IsIntrinsicType(this ITypeSymbol typeSymbol)
        {
            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Byte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                // NOTE: VB treats System.DateTime as an intrinsic, while C# does not, see "predeftype.h"
                //case SpecialType.System_DateTime:
                case SpecialType.System_Decimal:
                    return true;
                default:
                    return false;
            }
        }
    }
}
