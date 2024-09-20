namespace GPA.Tests
{
    public class SemethingTest
    {
        [Fact()]
        public void Test()
        {
            var g = new CodeGenerator();
            var code1 = g.GenerateCode();
            var code2 = g.GenerateCode();
            var code3 = g.GenerateCode();
            Assert.NotNull(code1);
        }

        //var code =  new BaseCodeGenerator().GenerateCode();
    }



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
                string sequencePart = _sequenceNumber.ToString("D6");
                return $"{_prefix}-{datePart}-{_sequenceNumber}";
            }
        }
    }

    public class CodeGenerator : BaseCodeGenerator
    {
        public CodeGenerator() : base("INV") { }

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
