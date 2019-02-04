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
    public class SimpleComb : IComb
    {
        private readonly IScalar<string> root;
        private readonly IScalar<string> name;
        private readonly Func<string, ICell> cell;

        /// <summary>
        /// A simple comb that is dumb: You need to tell it,
        /// how to create cells.
        /// </summary>
        public SimpleComb(string name, Func<string, ICell> cell):this("", name, cell)
        { }

        /// <summary>
        /// A simple comb that is dumb: You need to tell it,
        /// how to create cells.
        /// </summary>
        public SimpleComb(string root, string name, Func<string,ICell> cell) : this(new ScalarOf<string>(Path.Combine(root, name)), new ScalarOf<string>(name), cell)
        { }
        
        /// <summary>
        /// A simple comb that is dumb: You need to tell it,
        /// how to create cells.
        /// </summary>
        public SimpleComb(IScalar<string> root, IScalar<string> name, Func<string, ICell> cell)
        {
            this.root = root;
            this.name = name;
            this.cell = cell;
        }

        public ICell Cell(string name)
        {
            return this.cell.Invoke(this.root.Value() + Path.DirectorySeparatorChar + name);
        }

        public string Name()
        {
            return this.name.Value();
        }
    }
}
