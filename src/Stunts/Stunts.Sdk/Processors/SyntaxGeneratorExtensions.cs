using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Stunts.Processors
{
    /// <summary>
    /// Language agnostic helper methods for code generation.
    /// </summary>
    static class SyntaxGeneratorExtensions
    {
        /// <summary>
        /// Inspects a property to determine if supports read/write.
        /// </summary>
        public static (bool canRead, bool canWrite) InspectProperty(this SyntaxGenerator generator, SyntaxNode property)
        {
            return (
                generator.GetAccessor(property, DeclarationKind.GetAccessor) != null,
                generator.GetAccessor(property, DeclarationKind.SetAccessor) != null
                );
        }

        /// <summary>
        /// Replaces a method's body by invoking the behavior pipeline.
        /// </summary>
        public static SyntaxNode ImplementMethod(this SyntaxGenerator generator, SyntaxNode method, SyntaxNode returnType)
        {
            if (returnType != null)
            {
                return generator.WithStatements(method, new[]
                {
                    generator.ReturnStatement(generator.ExecutePipeline(returnType, generator.GetParameters(method)))
                });
            }

            return generator.WithStatements(method, new[]
            {
                generator.ExecutePipeline(returnType, generator.GetParameters(method))
            });
        }

        /// <summary>
        /// Replaces the implementation of a method with ref/out parameters by invoking the behavior pipeline.
        /// </summary>
        public static SyntaxNode ImplementMethod(this SyntaxGenerator generator,
            SyntaxNode method, SyntaxNode returnType, SyntaxNode[] outParams, SyntaxNode[] refOutParams)
        {
            var statements = outParams.Select(x => generator.AssignmentStatement(
                generator.IdentifierName(generator.GetName(x)),
                generator.DefaultExpression(generator.GetType(x))))
                .ToList();

            statements.Add(generator.LocalDeclarationStatement(
                generator.IdentifierName(nameof(IMethodReturn)),
                "returns",
                generator.ExecutePipeline(null, generator.GetParameters(method))));

            statements.AddRange(refOutParams.Select(x =>
                generator.AssignmentStatement(
                    generator.IdentifierName(generator.GetName(x)),
                    generator.CastExpression(
                        generator.GetType(x),
                        generator.ElementAccessExpression(
                            generator.MemberAccessExpression(
                                generator.IdentifierName("returns"),
                                nameof(IMethodReturn.Outputs)),
                            generator.LiteralExpression(generator.GetName(x))
                        )
                    )
                )
            ));

            if (returnType != null)
            {
                statements.Add(generator.ReturnStatement(
                    generator.CastExpression(
                        returnType,
                        generator.MemberAccessExpression(
                            generator.IdentifierName("returns"),
                            nameof(IMethodReturn.ReturnValue)))));
            }

            return generator.WithStatements(method, statements);
        }

        /// <summary>
        /// Creates the <c>pipeline.Execute</c> method invocation.
        /// </summary>
        public static SyntaxNode ExecutePipeline(this SyntaxGenerator generator, SyntaxNode returnType, IEnumerable<SyntaxNode> parameters)
        {
            var execute = (returnType == null) ?
                generator.IdentifierName("Execute") :
                generator.GenericName("Execute", returnType);

            return generator.InvocationExpression(
                    generator.MemberAccessExpression(
                        generator.IdentifierName("pipeline"),
                        execute),
                    CreateMethodInvocation(generator, parameters)
                );
        }

        /// <summary>
        /// Creates the instance of the <see cref="MethodInvocation"/> passed to the behavior pipeline.
        /// </summary>
        static SyntaxNode CreateMethodInvocation(SyntaxGenerator generator, IEnumerable<SyntaxNode> parameters) =>
            generator.ObjectCreationExpression(
                generator.IdentifierName(nameof(MethodInvocation)),
                new[]
                {
                    generator.ThisExpression(),
                    generator.InvocationExpression(
                        generator.MemberAccessExpression(
                            generator.IdentifierName(nameof(MethodBase)),
                            nameof(MethodBase.GetCurrentMethod))),
                }
                .Concat(parameters.Select(x => generator.Argument(generator.IdentifierName(generator.GetName(x))))));
    }
}
