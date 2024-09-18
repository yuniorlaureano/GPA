namespace GPA.Utils.CodeGenerators
{
    public class InvoiceCodeGenerator : BaseCodeGenerator
    {
        public InvoiceCodeGenerator() : base("I") { }

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
