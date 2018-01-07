namespace Sample
{
    public class CalculatorMemory : ICalculatorMemory
    {
        int memory;

        public virtual void Add(int value) => memory += value;

        public virtual void Clear() => memory = 0;

        public virtual int Recall() => memory;

        public virtual void Subtract(int value) => memory -= value;
    }
}
