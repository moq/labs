using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Sample;
using Stunts;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class DynamicMockTests
    {
        [Fact]
        public async Task WhenAddingMockBehavior_ThenCanInterceptSelectively()
        {
            var calculator = await new DynamicMock(LanguageNames.CSharp).CreateAsync<ICalculator>();
            var behavior = new MockContextBehavior();

            calculator.AddBehavior(behavior);
            calculator.AddBehavior((m, n) => m.CreateValueReturn(CalculatorMode.Scientific), m => m.MethodBase.Name == "get_Mode");
            calculator.AddBehavior(new DefaultValueBehavior());

            var mode = calculator.Mode;
            var add = calculator.Add(3, 2);

            Assert.Equal(CalculatorMode.Scientific, mode);
            Assert.Equal(0, add);
        }

        [Fact]
        public async Task WhenGeneratingGenericMock_ThenImplementsGenericType()
        {
            var calculator = await new DynamicMock(LanguageNames.CSharp).CreateAsync<IRepository<PlatformID>>();

            Assert.IsAssignableFrom<IMocked>(calculator);
            Assert.IsAssignableFrom<IRepository<PlatformID>>(calculator);
        }

        [Fact]
        public async Task WhenGeneratingGenericMock_ThenImplementsMultipleGenericType()
        {
            var calculator = await new DynamicMock(LanguageNames.CSharp).CreateAsync<IEnumerable<IList<IDictionary<string, PlatformID>>>>();

            Assert.IsAssignableFrom<IMocked>(calculator);
            Assert.IsAssignableFrom<IEnumerable<IList<IDictionary<string, PlatformID>>>>(calculator);
        }
    }

    public interface IRepository<T>
    {
        int Add(T value);
        bool Delete(int id);
        T Get(int id);
    }
}