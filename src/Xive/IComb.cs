namespace Xive
{
    /// <summary>
    /// A comb, containing multiple cells.
    /// </summary>
    public interface IComb
    {
        /// <summary>
        /// Unique name of the comb.
        /// </summary>
        /// <returns>The comb's name</returns>
        string Name();

        /// <summary>
        /// Get a cell by its unique name.
        /// It is not needed to seperately create a cell - just acquire it, 
        /// the Comb will build it if necessary.
        /// </summary>
        /// <param name="name">Unique name of the cell</param>
        /// <returns>The cell</returns>
        ICell Cell(string name);
    }
}
