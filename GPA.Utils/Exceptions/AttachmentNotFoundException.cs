using System.Net;

namespace GPA.Utils.Exceptions
{
    public class AttachmentNotFoundException : Exception, IGPAException
    {
        public AttachmentNotFoundException() : base() { }
        public AttachmentNotFoundException(string message) : base(message) { }

        public HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    }
}
