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
