using System.Xml.Linq;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Xocument
{
    /// <summary>
    /// A xocument which is stored in a cell.
    /// </summary>
    public sealed class CellXocument : XocumentEnvelope
    {
        public CellXocument(ICell cell, string name) : this(cell, name, "0.0.1")
        { }

        /// <summary>
        /// A xocument which is stored in a cell.
        /// </summary>
        public CellXocument(ICell cell, string name, string version) : base(new ScalarOf<IXocument>(() =>
            {
                return
                    new SimpleXocument(
                        new ScalarOf<XNode>(() =>
                        {
                            if (name.ToLower().EndsWith(".xml"))
                            {
                                name = name.Substring(0, name.Length - 4);
                            }
                            byte[] content = cell.Content();
                            if(content.Length == 0)
                            {
                                cell.Update(
                                    new InputOf(
                                        new XDocument(
                                            new XElement(name, new XAttribute("version", version.ToString()))
                                        ).ToString()
                                    )
                                );
                                content = cell.Content();
                            }

                            return
                                XDocument.Parse(
                                    new TextOf(
                                        new InputOf(content)
                                    ).AsString()
                                );
                        }),
                        xnode =>
                        {
                            cell.Update(new InputOf(xnode.ToString()));
                        }
                    );
            })
        )
        { }
    }
}
