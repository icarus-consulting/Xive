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

namespace Xive
{
    /// <summary>
    /// A comb, containing multiple cells.
    /// </summary>
    public interface IHoneyComb
    {
        /// <summary>
        /// Unique name of the comb.
        /// </summary>
        /// <returns>The comb's name</returns>
        string Name();

        /// <summary>
        /// Props for this capsule.
        /// </summary>
        /// <returns></returns>
        //IProps Props();

        /// <summary>
        /// Get cell content as a Xocument by its unique name.
        /// It is not needed to seperately create a cell - just acquire it, 
        /// the Comb will build it if necessary.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IXocument Xocument(string name);

        /// <summary>
        /// Get a cell by its unique name.
        /// It is not needed to seperately create a cell - just acquire it, 
        /// the Comb will build it if necessary.
        /// </summary>
        /// <param name="name">Unique name of the cell</param>
        /// <returns>The cell</returns>
        ICell Cell(string name);

        /// <summary>
        /// Get props.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IProps Props();
    }
}
