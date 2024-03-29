﻿//MIT License

//Copyright (c) 2022 ICARUS Consulting GmbH

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
using Yaapii.Atoms;

namespace Xive.Test
{
    /// <summary>
    /// A cell that reports when its methods are called.
    /// </summary>
    public sealed class VerboseCell : ICell
    {
        private readonly Action reportContent;
        private readonly Action reportUpdate;
        private readonly Action reportDispose;
        private readonly ICell origin;

        /// <summary>
        /// A cell that reports when its methods are called.
        /// </summary>
        public VerboseCell(ICell origin, Action content, Action update, Action dispose)
        {
            this.reportContent = content;
            this.reportUpdate = update;
            this.reportDispose = dispose;
            this.origin = origin;
        }

        public byte[] Content()
        {
            this.reportContent();
            return this.Content();
        }

        public void Dispose()
        {
            this.reportDispose();
            this.origin.Dispose();
        }

        public string Name()
        {
            return this.origin.Name();
        }

        public void Update(IInput content)
        {
            this.reportUpdate();
            this.origin.Update(content);
        }
    }
}
