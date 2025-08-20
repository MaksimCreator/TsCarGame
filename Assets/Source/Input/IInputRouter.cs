using System;

public interface IInputRouter : IControl,IUpdateble
{
    event Action<float> OnAccaleration;
    event Action<float> OnRotate;
    event Action<float> onDownSpace;
    event Action onReset;
}