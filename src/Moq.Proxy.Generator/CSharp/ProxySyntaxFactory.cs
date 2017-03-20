using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq.Proxy.Properties;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Moq.Proxy.CSharp
{
    static class ProxySyntaxFactory
    {
        static readonly HashSet<string> usings = new HashSet<string>(new[]
        {
            "Moq.Proxy",
            "System",
            "System.Collections.Generic",
            "System.Reflection",
        });

        public static SyntaxToken ProxyClassIdentifier(params INamedTypeSymbol[] types) =>
            ValidIdentifier("ProxyOf" + string.Join("", types.OrderBy(t => t.Name).Select(t => t.Name)));

        public static SyntaxToken ValidIdentifier(string identifier)
        {
            if (!SyntaxFacts.IsValidIdentifier(identifier) ||
                SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None)
                throw new ArgumentException("identifier", Strings.InvalidIdentifier(identifier));

            return Identifier(identifier);
        }

        public static ClassDeclarationSyntax ProxyClass(SyntaxToken identifier) =>
            ClassDeclaration(identifier)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                IdentifierName("IProxy")))))
                .WithMembers(
                    List(
                        new MemberDeclarationSyntax[]{
                            FieldDeclaration(
                                VariableDeclaration(
                                    IdentifierName("BehaviorPipeline"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                            Identifier("pipeline"))
                                        .WithInitializer(
                                            EqualsValueClause(
                                                ObjectCreationExpression(
                                                    IdentifierName("BehaviorPipeline"))
                                                .WithArgumentList(
                                                    ArgumentList())))))),
                            PropertyDeclaration(
                                    GenericName(
                                        Identifier("IList"))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(
                                                IdentifierName("IProxyBehavior")))),
                                Identifier("Behaviors"))
                            .WithExplicitInterfaceSpecifier(
                                ExplicitInterfaceSpecifier(
                                    IdentifierName("IProxy")))
                            .WithExpressionBody(
                                ArrowExpressionClause(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("pipeline"),
                                        IdentifierName("Behaviors"))))
                            .WithSemicolonToken(
                                Token(SyntaxKind.SemicolonToken))}));

        public static ClassDeclarationSyntax AddProxiedTypes(this ClassDeclarationSyntax proxyClass, params INamedTypeSymbol[] types) =>
            proxyClass.AddBaseListTypes(types.OrderBy(t => t.Name).Select(i => SimpleBaseType(ToSyntax(i))).ToArray());

        public static ClassDeclarationSyntax AddImplementation(this ClassDeclarationSyntax proxyClass, ProxySyntaxInfo proxyInfo)
        {
            return proxyClass
                .AddImplicitEvents(proxyInfo)
                .AddImplicitProperties(proxyInfo)
                .AddImplicitIndexers(proxyInfo)
                .AddImplicitMethods(proxyInfo);
        }

        static ClassDeclarationSyntax AddImplicitEvents(this ClassDeclarationSyntax proxyClass, ProxySyntaxInfo proxyInfo)
        {
            // Events
            proxyClass = proxyClass
                .AddMembers(proxyInfo.ImplicitEvents.Select(e =>
                        EventDeclaration(
                            ToSyntax(e.Type),
                            Identifier(e.Name))
                        .WithPublicModifier()
                        .WithAccessorList(
                            AccessorList(
                                List(new AccessorDeclarationSyntax[] {
                                    AccessorDeclaration(
                                        SyntaxKind.AddAccessorDeclaration)
                                    .WithExpressionBody(
                                        ArrowExpressionClause(InvokePipeline(e.AddMethod.ReturnType, e.AddMethod.Parameters)))
                                    .WithSemicolon(),
                                    AccessorDeclaration(
                                        SyntaxKind.RemoveAccessorDeclaration)
                                    .WithExpressionBody(
                                        ArrowExpressionClause(InvokePipeline(e.RemoveMethod.ReturnType, e.RemoveMethod.Parameters)))
                                    .WithSemicolon()})))).ToArray());
            return proxyClass;
        }

        static ClassDeclarationSyntax AddImplicitProperties(this ClassDeclarationSyntax proxyClass, ProxySyntaxInfo proxyInfo)
        {
            foreach (var prop in proxyInfo.ImplicitProperties.Where(p => !p.IsIndexer))
            {
                var property =
                    PropertyDeclaration(
                        ToSyntax(prop.Type),
                        Identifier(prop.Name))
                    .WithPublicModifier();

                if (prop.GetMethod != null)
                {
                    property = property.AddAccessorListAccessors(
                        AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBody(
                            ArrowExpressionClause(InvokePipeline(prop.Type, prop.GetMethod.Parameters)))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken)));
                }
                if (prop.SetMethod != null)
                {
                    property = property.AddAccessorListAccessors(
                        AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration)
                        .WithExpressionBody(
                            ArrowExpressionClause(InvokePipeline(prop.Type, prop.SetMethod.Parameters)))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken)));
                }

                proxyClass = proxyClass.AddMembers(property);
            }

            return proxyClass;
        }

        static ClassDeclarationSyntax AddImplicitIndexers(this ClassDeclarationSyntax proxyClass, ProxySyntaxInfo proxyInfo)
        {
            foreach (var i in proxyInfo.ImplicitProperties.Where(p => p.IsIndexer))
            {
                var indexer = IndexerDeclaration(ToSyntax(i.Type))
                    .WithPublicModifier()
                    .WithParameterList(
                        BracketedParameterList()
                        .AddParameters(i.Parameters.Select(p =>
                            Parameter(Identifier(p.Name))
                            .WithType(ToSyntax(p.Type))).ToArray()));

                if (i.GetMethod != null)
                {
                    indexer = indexer.AddAccessorListAccessors(
                        AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBody(
                            ArrowExpressionClause(InvokePipeline(i.Type, i.GetMethod.Parameters)))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken)));
                }
                if (i.SetMethod != null)
                {
                    indexer = indexer.AddAccessorListAccessors(
                        AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration)
                        .WithExpressionBody(
                            ArrowExpressionClause(InvokePipeline(i.Type, i.SetMethod.Parameters)))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken)));
                }

                proxyClass = proxyClass.AddMembers(indexer);
            }

            return proxyClass;
        }

        static ClassDeclarationSyntax AddImplicitMethods(this ClassDeclarationSyntax proxyClass, ProxySyntaxInfo proxyInfo)
        {
            foreach (var m in proxyInfo.ImplicitMethods.Where(m => m.CanBeReferencedByName && (m.IsVirtual || m.IsAbstract)))
            {
                var method = MethodDeclaration(m.ReturnType.ToSyntax(), m.Name).WithPublicModifier();
                var outParams = new List<IParameterSymbol>();
                var refOutParams = new List<IParameterSymbol>();
                foreach (var p in m.Parameters)
                {
                    var parameter = Parameter(Identifier(p.Name));
                    switch (p.RefKind)
                    {
                        case RefKind.Ref:
                            parameter = parameter.WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)));
                            refOutParams.Add(p);
                            break;
                        case RefKind.Out:
                            parameter = parameter.WithModifiers(TokenList(Token(SyntaxKind.OutKeyword)));
                            outParams.Add(p);
                            refOutParams.Add(p);
                            break;
                        case RefKind.None:
                        default:
                            break;
                    }

                    method = method.AddParameterListParameters(parameter.WithType(ToSyntax(p.Type)));
                }

                if (outParams.Count != 0 || refOutParams.Count != 0)
                {
                    var statements = new List<StatementSyntax>(
                        outParams.Select(p => ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(p.Name),
                                DefaultExpression(p.Type.ToSyntax()))))
                    );

                    statements.Add(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                IdentifierName("var"))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                        Identifier("returns"))
                                    .WithInitializer(
                                        EqualsValueClause(InvokePipeline(null, m.Parameters)))))));

                    statements.AddRange(refOutParams.Select(p =>
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(p.Name),
                                CastExpression(
                                    p.Type.ToSyntax(),
                                    ElementAccessExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("returns"),
                                            IdentifierName("Outputs")))
                                    .WithArgumentList(
                                        BracketedArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        Literal(p.Name))))))))))
                    );

                    if (!m.ReturnsVoid)
                    {
                        statements.Add(
                            ReturnStatement(
                                CastExpression(
                                    method.ReturnType,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("returns"),
                                        IdentifierName("ReturnValue"))))
                        );
                    }

                    method = method.WithBody(Block(statements));
                }
                else
                {
                    method = method
                        .WithExpressionBody(
                            ArrowExpressionClause(InvokePipeline(m.ReturnType, m.Parameters)))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }

                proxyClass = proxyClass.AddMembers(method);
            }

            return proxyClass;
        }

        public static bool IsNullable(this ITypeSymbol symbol)
            => symbol?.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

        public static TypeSyntax ToSyntax(this ITypeSymbol symbol)
        {
            if (symbol.IsNullable())
                return NullableType(ToSyntax(((INamedTypeSymbol)symbol).TypeArguments[0]));

            switch (symbol.SpecialType)
            {
                case SpecialType.System_Void:
                    return PredefinedType(Token(SyntaxKind.VoidKeyword));
                case SpecialType.System_Object:
                    return PredefinedType(Token(SyntaxKind.ObjectKeyword));
                case SpecialType.System_String:
                    return PredefinedType(Token(SyntaxKind.StringKeyword));
                case SpecialType.System_Boolean:
                    return PredefinedType(Token(SyntaxKind.BoolKeyword));
                case SpecialType.System_Char:
                    return PredefinedType(Token(SyntaxKind.CharKeyword));
                case SpecialType.System_SByte:
                    return PredefinedType(Token(SyntaxKind.SByteKeyword));
                case SpecialType.System_Int16:
                    return PredefinedType(Token(SyntaxKind.ShortKeyword));
                case SpecialType.System_Int32:
                    return PredefinedType(Token(SyntaxKind.IntKeyword));
                case SpecialType.System_Int64:
                    return PredefinedType(Token(SyntaxKind.LongKeyword));
                case SpecialType.System_Byte:
                    return PredefinedType(Token(SyntaxKind.ByteKeyword));
                case SpecialType.System_UInt16:
                    return PredefinedType(Token(SyntaxKind.UShortKeyword));
                case SpecialType.System_UInt32:
                    return PredefinedType(Token(SyntaxKind.UIntKeyword));
                case SpecialType.System_UInt64:
                    return PredefinedType(Token(SyntaxKind.ULongKeyword));
                case SpecialType.System_Single:
                    return PredefinedType(Token(SyntaxKind.FloatKeyword));
                case SpecialType.System_Double:
                    return PredefinedType(Token(SyntaxKind.DoubleKeyword));
                case SpecialType.System_Decimal:
                default:
                    break;
            }

            if (symbol.TypeKind == TypeKind.Array)
            {
                var arrSym = (IArrayTypeSymbol)symbol;
                return ArrayType((arrSym).ElementType.ToSyntax(),
                    new SyntaxList<ArrayRankSpecifierSyntax>().AddRange(
                        Enumerable.Range(0, arrSym.Rank)
                            .Select(_ => ArrayRankSpecifier(
                                SingletonSeparatedList<ExpressionSyntax>(
                                    OmittedArraySizeExpression()))))
                );
            }

            if (symbol.ContainingNamespace == null || usings.Contains(symbol.ContainingNamespace.ToDisplayString()))
                return IdentifierName(symbol.Name);

            var name = default(NameSyntax);
            ISymbol symbolOrParent = symbol;
            var names = new Stack<SimpleNameSyntax>();
            while (symbolOrParent != null && !string.IsNullOrEmpty(symbolOrParent.Name))
            {
                names.Push(IdentifierName(symbolOrParent.Name));
                symbolOrParent = symbolOrParent.ContainingSymbol;
            }

            name = names.Pop();
            while (names.Count != 0)
            {
                name = QualifiedName(name, names.Pop());
            }

            return name;
        }

        public static ExpressionSyntax InvokePipeline(ITypeSymbol returnType, IEnumerable<IParameterSymbol> parameters)
        {
            var execute = (returnType == null || returnType.SpecialType == SpecialType.System_Void) ?
                IdentifierName("Execute") :
                (SimpleNameSyntax)GenericName(Identifier("Execute"), TypeArgumentList(SingletonSeparatedList(returnType.ToSyntax())));

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("pipeline"),
                    execute))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                CreateMethodInvocation(parameters)))));
        }

        public static ExpressionSyntax CreateMethodInvocation(IEnumerable<IParameterSymbol> parameters) =>
            ObjectCreationExpression(
                IdentifierName("MethodInvocation"))
            .WithArgumentList(ArgumentList()
                .AddArguments(Argument(ThisExpression()))
                .AddArguments(Argument(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("MethodBase"),
                            IdentifierName("GetCurrentMethod")))))
                .AddArguments(parameters.Select(p => Argument(IdentifierName(p.Name))).ToArray()));

        static EventDeclarationSyntax WithPublicModifier(this EventDeclarationSyntax syntax) =>
            syntax.WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

        static PropertyDeclarationSyntax WithPublicModifier(this PropertyDeclarationSyntax syntax) =>
            syntax.WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

        static IndexerDeclarationSyntax WithPublicModifier(this IndexerDeclarationSyntax syntax) =>
            syntax.WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

        static MethodDeclarationSyntax WithPublicModifier(this MethodDeclarationSyntax syntax) =>
            syntax.WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

        static AccessorDeclarationSyntax WithSemicolon(this AccessorDeclarationSyntax syntax) =>
            syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}
