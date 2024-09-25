using System.Net;

namespace GPA.Utils.Exceptions
{
    public interface IGPAException
    {
        /// <summary>
        /// Status code used when the exception middleware is processing the exception
        /// </summary>
        HttpStatusCode StatusCode { get; }
    }
}
