namespace Sample
{
    public abstract class CalculatorMemoryBase : ICalculatorMemory
    {
        public abstract void Add(int value);

        public abstract void Clear();

        public abstract int Recall();

        public abstract void Subtract(int value);
    }
}
