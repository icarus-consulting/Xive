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

using System;
using Xive.Hive;

namespace Xive.Farm
{
    /// <summary>
    /// A farm that accesses hives systemwide exclusively.
    /// </summary>
    public sealed class SyncFarm : IFarm
    {
        private readonly IFarm farm;

        public SyncFarm(IFarm farm)
        {
            this.farm = farm;
        }

        /// <summary>
        /// Access a specific hive.
        /// Example: 
        /// Farm(maybe for: An App "Organizer") -> Hive(maybe: Calendar) -> n Comb(maybe: Appointment) -> n Cells(maybe: subscribers.xml)
        /// </summary>
        /// <param name="name">Name of the hive</param>
        /// <returns>The hive</returns>
        public IHive Hive(string name)
        {
            lock (farm)
            {
                return new SyncHive(farm.Hive(name));
            }
        }
    }
}
