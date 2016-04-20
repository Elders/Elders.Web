using System.Net;

namespace Elders.Web.Api.RExamples
{
    public class StatusRExample : RExample
    {
        public StatusRExample(HttpStatusCode statusCode, object example) : base(example)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}
