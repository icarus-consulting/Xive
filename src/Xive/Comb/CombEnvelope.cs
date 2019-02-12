﻿using System;
using System.Collections.Generic;
using System.Text;
using Yaapii.Atoms;

namespace Xive.Comb
{
    /// <summary>
    /// Envelope for combs.
    /// </summary>
    public abstract class CombEnvelope : IComb
    {
        private readonly IScalar<IComb> comb;

        /// <summary>
        /// Envelope for combs.
        /// </summary>
        public CombEnvelope(IScalar<IComb> comb)
        {
            this.comb = comb;
        }

        public ICell Cell(string name)
        {
            return this.comb.Value().Cell(name);
        }

        public string Name()
        {
            return this.comb.Value().Name();
        }

        public IXocument Xocument(string name)
        {
            return this.comb.Value().Xocument(name);
        }
    }
}
