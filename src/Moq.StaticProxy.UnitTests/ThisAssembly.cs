[assembly: Xunit.CollectionBehavior(MaxParallelThreads = -1)]

/// <summary>
/// Makes ThisAssembly public so it can be used in dynamically 
/// compiled scenarios too.
/// </summary>
public partial class ThisAssembly
{
}
