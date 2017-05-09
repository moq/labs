using System;
using System.Collections.Generic;

namespace Moq.Proxy
{
    public abstract class CalculatorBase
    {
#pragma warning disable CS0067
        public virtual event EventHandler TurnedOn;
#pragma warning restore CS0067

        public abstract bool IsOn { get; }

        public abstract void TurnOn();
    }

    public class Calculator : ICalculator, IDisposable
    {
        Dictionary<string, int> memory = new Dictionary<string, int>();

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
                    memory.Remove(name);
                else
                    Store(name, value.Value);
            }
        }

        public virtual void Clear(string name) => memory.Remove(name ?? "null");

        public virtual int? Recall(string name) => (memory.TryGetValue(name ?? "null", out int i)) ? i : default(int?);

        public virtual void Store(string name, int value) => memory[name ?? "null"] = value;

        public virtual bool TryAdd(ref int x, ref int y, out int z)
        {
            z = x + y;
            return true;
        }

        public void Dispose() => memory.Clear();
    }
}