using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Stunts.Tests
{
    public class StuntGeneratorEndToEnd
    {
        readonly ITestOutputHelper output;

        public StuntGeneratorEndToEnd(ITestOutputHelper output) => this.output = output;

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task WhenTypeIsInterface(string language)
        {
            var dynamic = new DynamicStunt(language);
            var target = await dynamic.GenerateAsync<ICalculator>();
            var intercepted = false;

            target.AddBehavior((method, next) => { intercepted = true; return next()(method, next); });
            target.AddBehavior(new DefaultValueBehavior());

            target.Add(1, 1);
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            target.Clear("foo");
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            Assert.False(target.IsOn);
            Assert.True(intercepted, "Failed to intercept property getter");

            intercepted = false;
            Assert.Equal(default(CalculatorMode), target.Mode);
            Assert.True(intercepted, "Failed to intercept property getter");

            intercepted = false;
            target.Mode = CalculatorMode.Scientific;
            Assert.True(intercepted, "Failed to intercept property setter");

            intercepted = false;
            target.Recall("foo");
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            target.Store("foo", 1);
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept regular method");

            var x = 0;
            var y = 0;
            var z = 0;

            intercepted = false;
            target.TryAdd(ref x, ref y, out z);
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept ref/out method");

            intercepted = false;
            target.TurnedOn += (s, e) => { };
            Assert.True(intercepted, "Failed to intercept event add");

            intercepted = false;
            target.TurnedOn -= (s, e) => { };
            Assert.True(intercepted, "Failed to intercept event remove");

            intercepted = false;
            target.TurnOn();
            Assert.True(intercepted, "Failed to intercept regular method");
        }

        // TODO: why does this fail for VB? 
        //[InlineData(LanguageNames.VisualBasic)]
        [InlineData(LanguageNames.CSharp)]
        [Theory]
        public async Task WhenTypeIsAbstract(string language)
        {
            var dynamic = new DynamicStunt(language);
            var target = await dynamic.GenerateAsync<CalculatorBase>();
            var intercepted = false;

            target.AddBehavior((method, next) => { intercepted = true; return next()(method, next); });
            target.AddBehavior(new DefaultValueBehavior());

            intercepted = false;
            Assert.False(target.IsOn);
            Assert.True(intercepted);

            intercepted = false;
            target.TurnOn();
            Assert.True(intercepted);

            intercepted = false;
            target.TurnedOn += (s, e) => { };
            if (language == LanguageNames.VisualBasic)
                // Intercepting events doesn't work in VB
                Assert.False(intercepted, "Visual Basic can't intercept virtual events");
            else
                Assert.True(intercepted);

            intercepted = false;
            target.TurnedOn -= (s, e) => { };
            if (language == LanguageNames.VisualBasic)
                // Intercepting events doesn't work in VB
                Assert.False(intercepted, "Visual Basic can't intercept virtual events");
            else
                Assert.True(intercepted);
        }

        // TODO: why does this fail for VB? 
        // [InlineData(LanguageNames.VisualBasic)]
        [InlineData(LanguageNames.CSharp)]
        [Theory]
        public async Task WhenTypeHasVirtualMembers(string language)
        {
            var dynamic = new DynamicStunt(language);
            var target = await dynamic.GenerateAsync<Calculator>();
            var intercepted = false;

            target.AddBehavior((method, next) => { intercepted = true; return next()(method, next); });
            target.AddBehavior(new DefaultValueBehavior());

            target.Add(1, 1);
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            target.Clear("foo");
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            Assert.False(target.IsOn);
            Assert.True(intercepted, "Failed to intercept property getter");

            intercepted = false;
            target.Mode = CalculatorMode.Scientific;
            Assert.True(intercepted, "Failed to intercept property setter");

            intercepted = false;
            Assert.Equal(default(CalculatorMode), target.Mode);
            Assert.True(intercepted, "Failed to intercept property getter");

            intercepted = false;
            target.Recall("foo");
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            target.Store("foo", 1);
            Assert.True(intercepted, "Failed to intercept regular method");

            var x = 0;
            var y = 0;
            var z = 0;

            intercepted = false;
            target.TryAdd(ref x, ref y, out z);
            Assert.True(intercepted, "Failed to intercept ref/out method");

            intercepted = false;
            target.TurnedOn += (s, e) => { };
            if (language == LanguageNames.VisualBasic)
                // Intercepting events doesn't work in VB
                Assert.False(intercepted, "Visual Basic can't intercept virtual events");
            else
                Assert.True(intercepted, "Failed to intercept event add");

            intercepted = false;
            target.TurnedOn -= (s, e) => { };
            if (language == LanguageNames.VisualBasic)
                // Intercepting events doesn't work in VB
                Assert.False(intercepted, "Visual Basic can't intercept virtual events");
            else
                Assert.True(intercepted, "Failed to intercept event remove");

            intercepted = false;
            target.TurnOn();
            Assert.True(intercepted, "Failed to intercept regular method");
        }
    }
}
