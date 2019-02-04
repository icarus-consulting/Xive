namespace Xive
{
    /// <summary>
    /// A farm, containing multiple hives.
    /// Every hive should represent a seperate category.
    /// For example: "Todo", "Reminder", "Appointment".
    /// </summary>
    public interface IFarm
    {
        /// <summary>
        /// Acess a specific hive.
        /// Example: 
        /// Farm(maybe for: An App "Organizer") -> Hive(maybe: Calendar) -> n Comb(maybe: Appointment) -> n Cells(maybe: subscribers.xml)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IHive Hive(string name);
    }
}
