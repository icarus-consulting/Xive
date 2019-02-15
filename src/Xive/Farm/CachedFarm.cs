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

using System.Collections.Generic;
using System.Xml.Linq;
using Xive.Hive;

namespace Xive.Farm
{
    /// <summary>
    /// A farm that is cached.
    /// </summary>
    public class CachedFarm : IFarm
    {
        private readonly IFarm origin;
        private readonly IDictionary<string, byte[]> binMemory;
        private readonly IDictionary<string, XNode> xmlMemory;

        /// <summary>
        /// A farm that is cached.
        /// </summary>
        /// <param name="origin"></param>
        public CachedFarm(IFarm origin) : this(origin, new Dictionary<string,byte[]>(), new Dictionary<string,XNode>())
        { }

        /// <summary>
        /// A farm that is cached.
        /// </summary>
        public CachedFarm(IFarm origin, IDictionary<string,byte[]> binMemory, IDictionary<string,XNode> xmlMemory)
        {
            this.origin = origin;
            this.binMemory = binMemory;
            this.xmlMemory = xmlMemory;
        }

        public IHive Hive(string name)
        {
            return
                new CachedHive(
                    new Coordinate(name).AsString(),
                    this.origin.Hive(name),
                    this.binMemory,
                    this.xmlMemory
                );
        }
    }
}
