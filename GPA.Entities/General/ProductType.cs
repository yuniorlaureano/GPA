namespace GPA.Entities.General
{
    public enum ProductType : byte
    {
        /// <summary>
        /// Products ready for sale to customers
        /// </summary>
        FinishedProduct = 1,

        /// <summary>
        /// roducts used in the production of other products
        /// </summary>
        RawProduct = 2
    }
}
