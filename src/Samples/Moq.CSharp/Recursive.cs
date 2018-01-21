using System;
using Moq;

namespace Sample.CSharp
{
    public class Recursive
    {
        CalculatorBase calculator;

        public Recursive()
        {
            calculator = Mock.Of<CalculatorBase, ICustomFormatter, IDisposable>();
        }

        public void FieldTest()
        {
            calculator.Setup(m => m.Memory.Recall()).Returns(5);
            var c = Mock.Of<ICalculator>();

            c.Setup(x => x.Store("foo", 5));
        }
    }
}