namespace Definit.QData.Model
{
    public enum QDataPropertyRoles
    {
        Key = 10,
        /// <summary>
        /// Indicates that the property is available to client for both read (query) and write (create / update)
        /// This is the default setting when no attribute is applied.
        /// </summary>
        ReadWrite = 20,
        /// <summary>
        /// Indicates that this property is read-only for client. 
        /// This property is available for query but will be ignored if present for create / update.
        /// </summary>
        Read = 21,
        Computed = 30,
        /// <summary>
        /// Not available for client at all.
        /// </summary>
        None = 90
    }

}