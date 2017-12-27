using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Stunts;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class DynamicMockTests
    {
        [Fact(Skip = "Couldn't make Roslyn compile generated code at runtime :(")]
        public async Task WhenAddingMockBehavior_ThenCanInterceptSelectively()
        {
            var calculator = await new DynamicMock(LanguageNames.CSharp).CreateAsync<ICalculator>();
            var behavior = new MockTrackingBehavior();

            calculator.AddBehavior(behavior);
            calculator.AddBehavior((m, n) => m.CreateValueReturn(CalculatorMode.Scientific), m => m.MethodBase.Name == "get_Mode");
            calculator.AddBehavior(new DefaultValueBehavior());

            var mode = calculator.Mode;
            var add = calculator.Add(3, 2);

            Assert.Equal(CalculatorMode.Scientific, mode);
            Assert.Equal(0, add);
        }
    }
}
