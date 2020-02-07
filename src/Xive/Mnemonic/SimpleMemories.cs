﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;
using Xive.Hive;
using Xive.Props;
using Xive.Xocument;

namespace Xive.Cache
{
    public sealed class SimpleMemories : IMemories
    {
        private readonly IMemory<XNode> xmlMem;
        private readonly IMemory<MemoryStream> dataMem;
        private readonly ConcurrentDictionary<string, IProps> propsMem;

        public SimpleMemories() : this(new XmlRam(), new DataRam())
        { }

        public SimpleMemories(IMemory<XNode> xmlMem, IMemory<MemoryStream> dataMem)
        {
            this.xmlMem = xmlMem;
            this.dataMem = dataMem;
            this.propsMem = new ConcurrentDictionary<string, IProps>();
        }

        public IProps Props(string scope, string id)
        {
            return
                this.propsMem.GetOrAdd(
                    $"{scope}/{id}",
                    key =>
                    new XocumentProps(
                        new MemorizedXocument($"{scope}/hq/catalog.xml", this),
                        id
                    )
                );
        }

        public IMemory<XNode> XML()
        {
            return this.xmlMem;
        }

        public IMemory<MemoryStream> Data()
        {
            return this.dataMem;
        }
    }
}
