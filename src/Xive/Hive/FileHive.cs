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
using System.Collections.Generic;
using System.IO;
using Xive.Comb;
using Yaapii.Atoms;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Hive
{
    /// <summary>
    /// A hive that exists physically as files.
    /// </summary>
    public sealed class FileHive : IHive
    {
        private readonly string scope;
        private readonly IText root;
        private readonly Func<IHoneyComb, IHoneyComb> wrap;

        /// <summary>
        /// A hive that exists physically as files.
        /// </summary>
        public FileHive(string root) : this(
            "X", root
        )
        { }

        /// <summary>
        /// A hive that exists physically as files.
        /// </summary>
        public FileHive(string scope, string root) : this(
            scope, root, comb => comb
        )
        { }

        /// <summary>
        /// A hive that exists physically as files.
        /// With this ctor, you can tell the hive how to build its catalog.
        /// </summary>
        public FileHive(string scope, string root, Func<IHoneyComb, IHoneyComb> combWrap)
        {
            this.scope = scope;
            this.wrap = combWrap;
            this.root = new TextOf( new Solid<string>(() =>
            {
                return new Normalized(root).AsString();

            }));
            }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            return
                new Mapped<string, IHoneyComb>(
                    name => this.wrap(Comb(name)),
                    new SimpleCatalog(this.scope, HQ()).List(xpath)
                );
        }

        public IEnumerable<IHoneyComb> Combs(string xpath, Func<ICatalog, ICatalog> catalogWrap)
        {
            return
                new Mapped<string, IHoneyComb>(
                    name => this.wrap(Comb(name)),
                    catalogWrap(new SimpleCatalog(this.scope, HQ())).List(xpath)
                );
        }

        public IHoneyComb HQ()
        {
            return this.wrap(Comb("hq"));
        }

        public IHive Shifted(string scope)
        {
            return new FileHive(scope, this.root.AsString(), this.wrap);
        }

        public string Scope()
        {
            return this.scope;
        }

        private IHoneyComb Comb(string name)
        {
            return
                new FileComb(
                    Path.Combine(this.root.AsString()),
                    $"{this.scope}/{name}"
                );
        }
    }
}
