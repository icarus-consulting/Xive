using System;
using System.Collections.Generic;
using System.Text;
using Xive.Mnemonic;
using Yaapii.Atoms.Scalar;

namespace Xive.Xocument
{
    public sealed class RamXocument : XocumentEnvelope
    {
        /// <summary>
        /// A xocument stored in memory.
        /// </summary>
        public RamXocument(string name) : base(
            new Sticky<IXocument>(() =>
                new MemorizedXocument(
                    name,
                    new RamMemories()
                )
            )
        )
        { }
    }
}
