﻿//MIT License

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
using System.Collections.Generic;
using Yaapii.Atoms;

namespace Xive.Hive
{
    /// <summary>
    /// Envelope for simple hive building.
    /// </summary>
    public abstract class HiveEnvelope : IHive
    {
        private readonly IScalar<IHive> hive;

        /// <summary>
        /// Envelope for simple hive building.
        /// </summary>
        public HiveEnvelope(IScalar<IHive> hive)
        {
            this.hive = hive;
        }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            return this.hive.Value().Combs(xpath);
        }

        public IEnumerable<IHoneyComb> Combs(string xpath, Func<ICatalog, ICatalog> catalog)
        {
            return this.hive.Value().Combs(xpath, catalog);
        }

        public IHoneyComb HQ()
        {
            return this.hive.Value().HQ();
        }

        public string Scope()
        {
            return this.hive.Value().Scope();
        }

        public IHive Shifted(string scope)
        {
            return this.hive.Value().Shifted(scope);
        }
    }
}
