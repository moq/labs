using System;
using System.ComponentModel;

public interface ICalculator
{
    event PropertyChangedEventHandler PropertyChanged;
    event EventHandler<int> Progress;
    event EventHandler PoweringUp;

    string Mode { get; set; }

    int Add(int a, int b);

    void PowerUp();
}