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
using System.IO;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;
using Xive.Hive;

namespace Xive.Farm
{
    /// <summary>
    /// A farm that lives in a directory.
    /// </summary>
    public sealed class FileFarm : IFarm
    {
        private readonly IScalar<string> root;
        private readonly Func<string, IHive> hive;

        /// <summary>
        /// A farm that lives in a directory.
        /// </summary>
        /// <param name="root">The root directory for the farm.</param>
        public FileFarm(string root) : this(root, (hiveName, comb) => new Catalog(hiveName, comb))
        { }

        /// <summary>
        /// A farm that lives in a directory.
        /// With this ctor, you can tell the farm how its hives should build their catalogs.
        /// </summary>
        /// <param name="root">The root directory for the farm.</param>
        /// <param name="hiveCatalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        public FileFarm(string root, Func<string, IHoneyComb, ICatalog> hiveCatalog) : this(
            root,
            name => new FileHive(name, root, hiveCatalog)
        )
        { }

        /// <summary>
        /// A farm that lives in a directory.
        /// With this ctor, you can tell the farm how to build a hive.
        /// </summary>
        /// <param name="root">The root directory for the farm.</param>
        /// <param name="hive">How the farm should build a hive: (hiveName) => new SomeHive(...)</param>
        public FileFarm(string root, Func<string, IHive> hive)
        {
            this.root = 
                new ScalarOf<string>(() =>
                    {
                        lock (this.root)
                        {
                            if (!Directory.Exists(root))
                            {
                                try
                                {
                                    Directory.CreateDirectory(root);
                                }
                                catch (UnauthorizedAccessException ux)
                                {
                                    throw new InvalidOperationException($"Cannot create farm directory, because path '{root}' is not accessible.");
                                }
                                catch (IOException ex)
                                {
                                    throw new InvalidOperationException($"Cannot create farm directory: {ex.Message}");
                                }
                            }
                            return root;
                        }
                    });
            this.hive = hive;
        }

        public IHive Hive(string name)
        {
            return this.hive(name);
        }
    }
}
