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
        public FileFarm(string root, Func<string, IComb, ICatalog> hiveCatalog) : this(
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
