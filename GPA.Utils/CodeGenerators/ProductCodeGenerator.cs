namespace GPA.Utils.CodeGenerators
{
    public class ProductCodeGenerator : BaseCodeGenerator
    {
        public ProductCodeGenerator() : base("P") { }

        public override void ResetDay()
        {
            if (_currentDay != DateTime.UtcNow.Day)
            {
                _sequenceNumber = 0;
                _currentDay = DateTime.UtcNow.Day;
            }
        }
    }
}
