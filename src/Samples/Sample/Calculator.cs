using System;
using System.Collections.Generic;

namespace Sample
{
    public class Calculator : ICalculator, IDisposable
    {
        CalculatorMemory memory = new CalculatorMemory();
        Dictionary<string, int> namedMemory = new Dictionary<string, int>();

        public virtual event EventHandler TurnedOn;

        public virtual bool IsOn { get; private set; }

        public virtual CalculatorMode Mode { get; set; }

        public virtual int Add(int x, int y) => x + y;

        public virtual int Add(int x, int y, int z) => x + y + z;

        public virtual void TurnOn()
        {
            TurnedOn?.Invoke(this, EventArgs.Empty);
            IsOn = true;
        }

        public virtual int? this[string name]
        {
            get => Recall(name);
            set
            {
                if (value == null)
                    namedMemory.Remove(name);
                else
                    Store(name, value.Value);
            }
        }

        public virtual void Clear(string name) => namedMemory.Remove(name ?? "null");

        public virtual int? Recall(string name) => (namedMemory.TryGetValue(name ?? "null", out int i)) ? i : default(int?);

        public virtual void Store(string name, int value) => namedMemory[name ?? "null"] = value;

        public virtual bool TryAdd(ref int x, ref int y, out int z)
        {
            z = x + y;
            return true;
        }

        public virtual void Dispose() => namedMemory.Clear();

        public virtual ICalculatorMemory Memory { get => memory; }
    }
}