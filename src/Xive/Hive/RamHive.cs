using Xive;
using Xive.Hive;
using Xive.Mnemonic;
using Yaapii.Atoms.Scalar;

/// <summary>
/// A hive which stores contents in RAM.
/// </summary>
public sealed class RamHive : HiveEnvelope
{
    private readonly string scope;

    /// <summary>
    /// A hive which stores contents in RAM.
    /// </summary>
    public RamHive(string scope) : this(scope, new RamMemories())
    { }

    /// <summary>
    /// A hive which stores contents in RAM.
    /// </summary>
    internal RamHive(string scope, IMemories mem) : base(
        new Solid<IHive>(() => 
            new MemorizedHive(scope, mem)
        )
    )
    { }
}
