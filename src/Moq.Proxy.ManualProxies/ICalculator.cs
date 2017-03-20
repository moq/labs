using System;

namespace Moq.Proxy
{
    public interface ICalculator
    {
        event EventHandler TurnedOn;

        bool IsOn { get; }

        CalculatorMode Mode { get; set; }

        int Add(int x, int y);

        int Add(int x, int y, int z);

        bool TryAdd(ref int x, ref int y, out int z);

        void TurnOn();

        int? this[string name] { get; set; }

        void Store(string name, int value);

        int? Recall(string name);

        void Clear(string name);
    }
}