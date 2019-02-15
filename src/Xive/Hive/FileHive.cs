//MIT License

//Copyright (c) 2019 ICARUS Consulting GmbH

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using Yaapii.Atoms.Scalar;
using Xive.Comb;

namespace Xive.Hive
{
    /// <summary>
    /// A hive that exists physically as files.
    /// </summary>
    public sealed class FileHive : HiveEnvelope
    {
        /// <summary>
        /// A hive that exists physically as files.
        /// </summary>
        public FileHive(string name, string root) : this(
            name, root, comb => comb
        )
        { }

        /// <summary>
        /// A hive that exists physically as files.
        /// </summary>
        public FileHive(string name, string root, Func<string, IHoneyComb, ICatalog> catalog) : this(
            name, root, comb => comb, catalog
        )
        { }

        /// <summary>
        /// A hive that exists physically as files.
        /// </summary>
        public FileHive(string name, string root, Func<IHoneyComb, IHoneyComb> combWrap) : this(
            name,
            root,
            combWrap,
            (hiveName, comb) => new Catalog(hiveName, comb)
        )
        { }

        /// <summary>
        /// A hive that exists physically as files.
        /// With this ctor, you can tell the hive how to build its catalog.
        /// </summary>
        public FileHive(string name, string root, Func<IHoneyComb, IHoneyComb> combWrap, Func<string, IHoneyComb, ICatalog> catalog) : base(
            new StickyScalar<IHive>(() =>
            {
                return
                    new SimpleHive(
                        name,
                        comb => combWrap(new FileComb($"{root}", comb)),
                        catalog
                    );
            })
        )
        { }
    }
}
