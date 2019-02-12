using System;
using System.IO;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive.Comb
{
    /// <summary>
    /// A simple comb that is dumb: You need to tell it,
    /// how to create items.
    /// </summary>
    public class SimpleComb : IHoneyComb
    {
        private readonly IScalar<string> name;
        private readonly Func<string, ICell> cell;
        private readonly Func<string, ICell, IXocument> xocument;

        /// <summary>
        /// A simple comb that is dumb: You need to tell it,
        /// how to create cells and how to create a xocument.
        /// </summary>
        public SimpleComb(string name, Func<string, ICell> cell, Func<string, ICell, IXocument> xocument) : this(new ScalarOf<string>(name), cell, xocument)
        { }

        /// <summary>
        /// A simple comb that is dumb: You need to tell it,
        /// how to create cells and how to create a xocument.
        /// </summary>
        public SimpleComb(IScalar<string> name, Func<string, ICell> cell, Func<string, ICell, IXocument> xocument)
        {
            this.name = name;
            this.cell = cell;
            this.xocument = xocument;
        }

        public ICell Cell(string name)
        {
            return this.cell(this.name.Value() + Path.DirectorySeparatorChar + name);
        }

        public string Name()
        {
            return this.name.Value();
        }

        public IXocument Xocument(string name)
        {
            return this.xocument(name, this.Cell(name));
        }
    }
}
