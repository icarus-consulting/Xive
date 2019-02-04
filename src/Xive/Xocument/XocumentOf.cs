using System;
using System.Collections.Generic;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;
using Yaapii.Atoms.Time;
using Yaapii.Xambly;
using Xive;
using Yaapii.Xml;

#pragma warning disable VariablesArePrivate // Fields are private

namespace Xive.Xocument
{
    public sealed class XocumentOf : IXocument
    {
        private readonly string root;
        private readonly IScalar<ICell> cell;

        /// <summary>
        /// A xocument which will be bootstrapped if it does not exist.
        /// If the cell is empty, this ctor will automatically build:
        /// <code>
        ///     &lt;root version="0.0.1" /&gt;
        /// </code>
        /// Note that this requires the content size to be read and therefore is 
        /// slightly slower than without bootstrapping.
        /// </summary>
        /// <param name="root">the root element for bootstrapping</param>
        /// <param name="version">the version for bootstrapping, default 0.0.1</param>
        /// <param name="cell">the cell to read/write</param>
        public XocumentOf(ICell cell, string root, string version = "0.0.1") : this(new StickyScalar<ICell>(() =>
            {
                if (cell.Content().Length == 0)
                {
                    cell.Update(
                            new InputOf(
                                $"<{root} version='{version.ToString()}' updated='{new DateAsText().AsString()}' />"
                            )
                        );
                }
                return cell;
            })
        )
        { }

        public XocumentOf(ICell cell) : this(
            new ScalarOf<ICell>(() => cell)
        )
        { }

        public XocumentOf(IScalar<ICell> cell)
        {
            this.cell = cell;
        }

        public IList<string> Values(string xpath)
        {
            IList<string> result = new List<string>();
            var content = new InputOf(cell.Value().Content());
            if(content.Stream().Length > 0)
            {
                var xml = new XMLQuery(content);
                result = xml.Values(xpath);
            }
            return result;
        }

        public string Value(string xpath, string def)
        {
            var values = this.Values(xpath);
            if (values.Count > 1)
            {
                throw new ArgumentException($"Too many values ({values.Count}) from XPath '{xpath}'");
            }

            string val;
            if (values.Count == 0)
            {
                val = def;
            }
            else
            {
                val = values[0];
            }

            return val;
        }

        public IList<IXML> Nodes(string xpath)
        {
            IList<IXML> result = new List<IXML>();
            var content = new InputOf(cell.Value().Content());
            if (content.Stream().Length > 0)
            {
                var xml = new XMLQuery(new TextOf(new InputOf(cell.Value().Content())).AsString());
                result = xml.Nodes(xpath);
            }
            return result;
        }


        public void Modify(IEnumerable<IDirective> dirs)
        {
            var before = new XMLQuery(new TextOf(new InputOf(cell.Value().Content())));
            var node = before.Node();
            new Xambler(dirs).ApplyQuietly(node);
            var after = new XMLQuery(node);
            cell.Value().Update(new InputOf(after.ToString()));
        }

        public override string ToString()
        {
            return
                new TextOf(
                    cell.Value().Content()
                ).AsString();
        }
    }
}
