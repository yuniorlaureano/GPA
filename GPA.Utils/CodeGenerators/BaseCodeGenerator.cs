namespace GPA.Utils.CodeGenerators
{
    public abstract class BaseCodeGenerator
    {
        protected int _sequenceNumber = 0;
        private readonly object _lock = new();
        protected readonly string _prefix;
        protected int _currentDay = DateTime.UtcNow.Day;

        public BaseCodeGenerator(string prefix)
        {
            _prefix = prefix;
        }

        public abstract void ResetDay();

        public string GenerateCode()
        {
            lock (_lock)
            {
                ResetDay();

                _sequenceNumber++;
                string datePart = DateTime.UtcNow.ToString("yyyyMMdd-HHmm");
                return $"{_prefix}-{datePart}-{_sequenceNumber}";
            }
        }
    }
}
