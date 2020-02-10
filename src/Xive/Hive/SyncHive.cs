////MIT License

////Copyright (c) 2019 ICARUS Consulting GmbH

////Permission is hereby granted, free of charge, to any person obtaining a copy
////of this software and associated documentation files (the "Software"), to deal
////in the Software without restriction, including without limitation the rights
////to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
////copies of the Software, and to permit persons to whom the Software is
////furnished to do so, subject to the following conditions:

////The above copyright notice and this permission notice shall be included in all
////copies or substantial portions of the Software.

////THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
////IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
////FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
////AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
////LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
////OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
////SOFTWARE.

//using System;
//using System.Collections.Generic;
//using System.Threading;
//using Xive.Comb;
//using Yaapii.Atoms.Enumerable;

//namespace Xive.Hive
//{
//    /// <summary>
//    /// A hive that accesses cells systemwide exclusively.
//    /// </summary>
//    public sealed class SyncHive : IHive
//    {
//        private readonly IHive hive;
//        private readonly ISyncValve syncValve;

//        /// <summary>
//        /// A hive that accesses cells systemwide exclusively.
//        /// </summary>
//        public SyncHive(IHive hive) : this(hive, new SyncGate())
//        { }

//        /// <summary>
//        /// A hive that accesses cells processwide exclusively.
//        /// </summary>
//        public SyncHive(IHive hive, ISyncValve syncValve)
//        {
//            this.hive = hive;
//            this.syncValve = syncValve;
//        }

//        public IEnumerable<IHoneyComb> Combs(string xpath)
//        {
//            return
//                new Mapped<IHoneyComb, IHoneyComb>(
//                    (comb) => new SyncComb(comb, this.syncValve),
//                    this.hive.Combs(xpath, catalog => new SyncCatalog(hive, catalog, this.syncValve))
//                );
//        }

//        public IEnumerable<IHoneyComb> Combs(string xpath, Func<ICatalog, ICatalog> catalogWrap)
//        {
//            return
//                new Mapped<IHoneyComb, IHoneyComb>(
//                    (comb) => new SyncComb(comb, this.syncValve),
//                    this.hive.Combs(
//                        xpath,
//                        catalog => new SyncCatalog(hive, catalogWrap(catalog), this.syncValve)
//                    )
//                );
//        }

//        public IHoneyComb HQ()
//        {
//            return new SyncComb(hive.HQ(), this.syncValve);
//        }

//        public IHive Shifted(string scope)
//        {
//            return new SyncHive(this.hive.Shifted(scope), this.syncValve);
//        }

//        public string Scope()
//        {
//            return this.hive.Scope();
//        }

//    }
//}
