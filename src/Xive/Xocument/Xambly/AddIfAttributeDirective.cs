using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Text;
using Yaapii.Xambly.Arg;
using Yaapii.Xambly.Cursor;

namespace Yaapii.Xambly
{
    /// <summary>
    /// ADDIFATTRIBUTE directive.
    /// Adds a node, if child element with specified content does not exist.
    /// </summary>
    public class AddIfAttributeDirective : IDirective
    {
        private readonly IArg name;
        private readonly IArg attribute;
        private readonly IArg value;

        /// <summary>
        /// ADDIFATTRIBUTE directive.
        /// Adds new node with attribute, if it's absent.
        /// </summary>
        public AddIfAttributeDirective(string node, string name, string value)
        {
            this.name = new ArgOf(node);
            this.attribute = new ArgOf(name);
            this.value = new ArgOf(value);
        }

        /// <summary>
        /// String representation.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return new FormattedText("ADDIFATTRIBUTE '{0}' '{1}'='{2}'", this.name.Raw(), this.attribute.Raw(), this.value.Raw()).AsString();
        }

        /// <summary>
        /// Execute it in the given document with current position at the given node.
        /// </summary>
        /// <param name="dom">Document</param>
        /// <param name="cursor">Nodes we're currently at</param>
        /// <param name="stack">Execution stack</param>
        /// <returns>New current nodes</returns>
        public ICursor Exec(XNode dom, ICursor cursor, IStack stack)
        {
            var targets = new List<XNode>();
            var label = this.name.Raw().ToLower();
            foreach (var node in cursor)
            {
                var kids = Children(node);
                XNode target = null;
                var len = kids.Count;
                for (int i = 0; i < len; i++)
                {
                    if (this.Matches(kids[i]))
                    {
                        {
                            target = kids[i];
                            break;
                        }
                    }
                }

                if (target == null)
                {
                    XDocument doc = null;
                    if (dom.Document == null)
                    {
                        doc = new XDocument(dom);
                    }
                    else
                    {
                        doc = dom.Document;
                    }

                    target = new XElement(this.name.Raw());
                    (node as XElement).Add(target);
                }
                targets.Add(target);
            }
            return new DomCursor(targets);
        }

        private bool Matches(XNode node)
        {
            bool matches = false;
            var xElement = node as XElement;
            if (String.Compare(xElement.Name.LocalName, this.name.Raw(), true) == 0)
            {
                foreach (XNode child in Children(node))
                {
                    var xChild = child as XElement;
                    if (xChild.Attributes().Contains(new XAttribute(this.name.Raw(), this.value.Raw())))
                    {
                        matches = true;
                        break;
                    }
                }
            }
            return matches;
        }

        private IList<XElement> Children(XNode node)
        {
            return new ListOf<XElement>((node as XElement).Elements().OfType<XElement>());
        }
    }
}
