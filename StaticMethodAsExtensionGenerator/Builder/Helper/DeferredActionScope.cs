namespace zms9110750.StaticMethodAsExtensionGenerator.Builder.Helper;

class DeferredActionScope : IDisposable
{
    Stack<Action> DeferredActions { get; } = new Stack<Action>();
    public void Defer(Action action)
    {
        DeferredActions.Push(action);
    }
    public void Dispose()
    {
        while (DeferredActions.Count > 0)
        {
            DeferredActions.Pop()();
        }
    }
}
