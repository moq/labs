using System;
using Moq;
using Sample;
using Xunit;

public class Tests
{
    [Fact]
    public void StubProperties()
    {
        var calc = Mock.Of<ICalculator>();

        calc.Mode = CalculatorMode.Scientific;

        Assert.Equal(CalculatorMode.Scientific, calc.Mode);

        calc.Mode = CalculatorMode.Standard;

        Assert.Equal(CalculatorMode.Standard, calc.Mode);
    }

    [Fact(Skip = "Fluent recursive without setups not supported yet.")]
    public void Recusive()
    {
        var calc = Mock.Of<ICalculator>();

        calc.Memory.Recall().Returns(5);

        Assert.Equal(5, calc.Memory.Recall());
    }

    [Fact]
    public void RecusiveSetupScope()
    {
        var calc = Mock.Of<ICalculator>();

        using (calc.Setup())
        {
            calc.Memory.Recall().Returns(5);
        }

        Assert.Equal(5, calc.Memory.Recall());
    }

    [Fact]
    public void RecusiveSetupBase()
    {
        var calc = Mock.Of<CalculatorBase, IDisposable>();
        calc.Setup(m => m.Memory.Recall()).Returns(5);

        Assert.Equal(5, calc.Memory.Recall());
        Assert.IsAssignableFrom<IDisposable>(calc);
    }

    [Fact]
    public void RecusiveSetup()
    {
        var calc = Mock.Of<ICalculator, IDisposable>();
        calc.Setup(m => m.Memory.Recall()).Returns(5);

        Assert.Equal(5, calc.Memory.Recall());
        Assert.IsAssignableFrom<IDisposable>(calc);
    }

    [Fact]
    public void DelegateOut()
    {
        var mock = Mock.Of<IParser>();

        mock.Setup<TryParse>(mock.TryParse)
            .Returns((string input, out DateTimeOffset date) => DateTimeOffset.TryParse(input, out date));

        var expected = DateTimeOffset.Now;
        var value = expected.ToString("O");

        Assert.True(mock.TryParse(value, out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RecursiveDelegateOut()
    {
        var mock = Mock.Of<IEnvironment>();

        var expected = DateTimeOffset.Now;
        var value = expected.ToString("O");

        mock.Setup<TryParse>(() => mock.Parser.TryParse)
            .Returns((string input, out DateTimeOffset date) => DateTimeOffset.TryParse(value, out date));

        Assert.True(mock.Parser.TryParse(value, out var actual));
        Assert.Equal(expected, actual);
    }

    delegate bool TryParse(string input, out DateTimeOffset date);

}

public interface IEnvironment
{
    IParser Parser { get; }
}

public interface IParser
{
    bool TryParse(string input, out DateTimeOffset date);
}
