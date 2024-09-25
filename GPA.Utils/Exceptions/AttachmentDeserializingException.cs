using System.Net;

namespace GPA.Utils.Exceptions
{
    public class AttachmentDeserializingException : Exception, IGPAException
    {

        public AttachmentDeserializingException() : base() { }
        public AttachmentDeserializingException(string message) : base(message) { }

        public HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    }
}
