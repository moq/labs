using System;
using System.Linq;
using System.Linq.Expressions;
using Moq.Sdk;

namespace Moq.Linq
{
    public class MockSetupsBuilder : ExpressionVisitor
    {
        private readonly IMocked mock;
        private ParameterExpression parameter;

        public MockSetupsBuilder(IMocked mock)
        {
            this.mock = mock;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var lambda = (LambdaExpression)node;
            if (lambda.Parameters.Count != 1)
                throw new Exception(); // TODO change type of exception and set a proper message

            parameter = lambda.Parameters[0];
            return base.VisitLambda(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node != null)
            {
                if (node.NodeType == ExpressionType.Equal)
                {
                    // Account for the inverted assignment/querying like "false == foo.IsValid" scenario
                    if (node.Left.NodeType == ExpressionType.Constant)
                    {
                        // Invert left & right nodes in this case.
                        Setup(node.Right, node.Left);
                        return node;
                    }

                    // Perform straight conversion where the right-hand side will be the setup return value.
                    Setup(node.Left, node.Right);
                    return node;
                }

                if (node.NodeType != ExpressionType.AndAlso && node.NodeType != ExpressionType.And)
                    // TODO Set a proper message
                    throw new NotSupportedException(); //string.Format(CultureInfo.CurrentCulture, "Resources.LinqBinaryOperatorNotSupported", node.ToStringFixed()));
            }

            return base.VisitBinary(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node != null && node.Type == typeof(bool))
            {
                Setup(node, Expression.Constant(true));
                return node;
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node != null && node.Type == typeof(bool))
            {
                Setup(node, Expression.Constant(true));
                return node;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node != null && node.NodeType == ExpressionType.Not)
            {
                Setup(node.Operand, Expression.Constant(false));
                return node;
            }

            return base.VisitUnary(node);
        }

        private void Setup(Expression function, Expression returnValue)
        {
            var result = Expression.Lambda(function, parameter).Compile().DynamicInvoke(mock);
            var value = Expression.Lambda(returnValue).Compile().DynamicInvoke();
            result.Returns(value);
        }
    }
}
