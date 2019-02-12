using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Yaapii.Atoms.List;
using Yaapii.Xambly;
using Yaapii.Xml;

#pragma warning disable MaxVariablesCount // Four fields maximum
#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Test
{
    /// <summary>
    /// A fake xocument.
    /// </summary>
    public sealed class FkXocument : IXocument
    {
        private readonly Action<IEnumerable<IDirective>> modify;
        private readonly Func<string, IList<IXML>> nodes;
        private readonly Func<string, string, string> value;
        private readonly Func<string, IList<string>> values;
        private readonly Func<XNode> node;

        /// <summary>
        /// A fake xocument.
        /// </summary>
        public FkXocument() : this(
            directives => { },
            xpath => new ListOf<IXML>(),
            (xpath, def) => String.Empty,
            xpath => new ListOf<string>(),
            () => new XDocument()
        )
        { }

        /// <summary>
        /// A fake xocument.
        /// </summary>
        public FkXocument(Func<XNode> node) : this(
            directives => { },
            xpath => new ListOf<IXML>(),
            (xpath, def) => String.Empty,
            xpath => new ListOf<string>(),
            node
        )
        { }

        /// <summary>
        /// A fake xocument.
        /// </summary>
        public FkXocument(Action<IEnumerable<IDirective>> modify) : this(
            modify,
            xpath => new ListOf<IXML>(),
            (xpath, def) => String.Empty,
            xpath => new ListOf<string>(),
            () => new XDocument()
        )
        { }

        /// <summary>
        /// A fake xocument.
        /// </summary>
        public FkXocument(
            Action<IEnumerable<IDirective>> modify,
            Func<string, IList<IXML>> nodes,
            Func<string, string, string> value,
            Func<string, IList<string>> values,
            Func<XNode> node
        )
        {
            this.modify = modify;
            this.nodes = nodes;
            this.value = value;
            this.values = values;
            this.node = node;
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            this.modify(dirs);
        }

        public XNode Node()
        {
            return this.node();
        }

        public IList<IXML> Nodes(string xpath)
        {
            return this.nodes(xpath);
        }

        public string Value(string xpath, string def)
        {
            return this.value(xpath, def);
        }

        public IList<string> Values(string xpath)
        {
            return this.values(xpath);
        }

        public void Dispose()
        {

        }
    }
}
