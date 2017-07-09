using System;

namespace Moq.Proxy
{
    public abstract class CalculatorBase
    {
        public abstract event EventHandler TurnedOn;

        public abstract bool IsOn { get; }

        public abstract CalculatorMode Mode { get; set; }

        public abstract int Add(int x, int y);

        public abstract int Add(int x, int y, int z);

        public abstract bool TryAdd(ref int x, ref int y, out int z);

        public abstract void TurnOn();

        public abstract int? this[string name] { get; set; }

        public abstract void Store(string name, int value);

        public abstract int? Recall(string name);

        public abstract void Clear(string name);
    }
}