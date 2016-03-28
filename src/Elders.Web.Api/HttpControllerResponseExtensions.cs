using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;

namespace Elders.Web.Api
{
    public static class HttpControllerResponseExtensions
    {
        public static IHttpActionResult BadRequest<T>(this ApiController self, T data)
        {
            return new ResponseMessageResult(self.Request.CreateResponse(HttpStatusCode.BadRequest, data));
        }

        public static IHttpActionResult UnsupportedMediaType<T>(this ApiController self, T data)
        {
            return new ResponseMessageResult(self.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, data));
        }

        public static IHttpActionResult Accepted<T>(this ApiController self, T data, Action<HttpResponseHeaders> headers = null)
        {
            var response = self.Request.CreateResponse(HttpStatusCode.Accepted, data);
            if (headers != null)
                headers(response.Headers);
            return new ResponseMessageResult(response);
        }

        public static IHttpActionResult NotAcceptable<T>(this ApiController self, T data)
        {
            return new ResponseMessageResult(self.Request.CreateResponse(HttpStatusCode.NotAcceptable, data));
        }

        public static IHttpActionResult Forbidden<T>(this ApiController self, T data)
        {
            return new ResponseMessageResult(self.Request.CreateResponse(HttpStatusCode.Forbidden, data));
        }
    }

    public static class HttpControllerSerializerExtensions
    {
        public static T BuildModel<T>(this ApiController self, string json)
        {
            var obj = JsonConvert.DeserializeObject<T>(json, self.Configuration.Formatters.JsonFormatter.SerializerSettings);
            self.Validate(obj);
            if (self.ModelState.IsValid == false)
                throw new HttpResponseException(self.Request.CreateResponse(HttpStatusCode.BadRequest, self.ModelState));
            return obj;
        }
    }
}
