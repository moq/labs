using System;

public interface ICalculator
{
    event EventHandler PoweringUp;

    string Mode { get; set; }

    int Add(int a, int b);

    void PowerUp();
}