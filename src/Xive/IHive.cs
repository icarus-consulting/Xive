using System.Collections.Generic;

namespace Xive
{
    /// <summary>
    /// A hive, containing multiple combs.
    /// </summary>
    public interface IHive
    {

        string Name();

        /// <summary>
        /// The headquarter of the hive.
        /// Headquarters contain control informations for the hive, 
        /// which do not belong to a specific comb.
        /// 
        /// Example: 
        /// 1 Hive(maybe: Calendar) -> n Comb(maybe: Appointment)
        /// Since there are multiple combs, the Headquarter would contain information about
        /// which combs exist. Headquarter 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IHoneyComb HQ();

        /// <summary>
        /// Find a specific comb by xpath.
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        IEnumerable<IHoneyComb> Combs(string xpath);
    }
}
