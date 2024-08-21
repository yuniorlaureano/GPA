namespace GPA.Utils.Exceptions
{
    public class AttachmentNotFoundException : Exception
    {
        public AttachmentNotFoundException() : base() { }
        public AttachmentNotFoundException(string message) : base(message) { }
    }
}
