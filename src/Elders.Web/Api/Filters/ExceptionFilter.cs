using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Owin;
using Elders.Web.Api;

namespace Elders.Web.Api.Filters
{
    public class ErrorConverter : JsonConverter
    {
        public static log4net.ILog log = log4net.LogManager.GetLogger(typeof(ErrorConverter));

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var casted = value as HttpError;
            var ctx = HttpContext.Current.Request.GetOwinContext();


            var response = new ResponseResult(casted.Message + casted.MessageDetail);

            log.Error("[RequestError]" + GetString(casted, ctx));

            var jObject = JObject.FromObject(response, serializer);
            jObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, objectType);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HttpError);
        }

        private string GetString(HttpError error, IOwinContext ctx)
        {
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append(ctx.Request.Uri.AbsoluteUri);
            sb.Append(Environment.NewLine);
            sb.Append(ctx.Request.Method);
            foreach (var item in ctx.Request.Headers)
            {
                foreach (var value in item.Value)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(item.Key + ": " + value);
                }
            }

            return sb.ToString();
        }
    }

    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public static log4net.ILog log = log4net.LogManager.GetLogger(typeof(ExceptionFilter));

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var request = actionExecutedContext.Request.ToString();
            var exception = actionExecutedContext.Exception;
            HttpResponseMessage errorResponse;

            if (!actionExecutedContext.ActionContext.ModelState.IsValid)
                errorResponse = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseResult(actionExecutedContext.ActionContext.ModelState.GetErrorMessages()));
            else
                errorResponse = actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseResult(exception.AsString()));

            log.Error("[RequestError]" + actionExecutedContext.Request.AsString(actionExecutedContext.ActionContext.ModelState), exception);
            actionExecutedContext.Response = errorResponse;
        }
    }

    public static class HttpActionExecutedContextAsString
    {
        public static string AsString(this HttpRequestMessage request, ModelStateDictionary modelState = null)
        {
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append(request.RequestUri.AbsoluteUri);
            sb.Append(Environment.NewLine);
            sb.Append(request.Method);
            foreach (var item in request.Headers)
            {
                foreach (var value in item.Value)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(item.Key + ": " + value);
                }
            }
            if (modelState != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(JsonConvert.SerializeObject(modelState));
            }

            return sb.ToString();
        }
    }

    public static class ExceptionExtensions
    {
        public static string AsString(this Exception exception)
        {
            return Format(exception, true, true);
        }

        private static String Format(Exception exception, bool needFileLineInfo, bool needMessage)
        {
            String message = (needMessage ? exception.Message : null);
            String result;

            if (message == null || message.Length <= 0)
                result = exception.GetType().FullName;
            else
                result = exception.GetType().FullName + ": " + exception.Message;

            if (exception.InnerException != null)
                result = result + " ---> " + exception.InnerException.AsString() + Environment.NewLine + "   --- End of inner exception stack trace ---";

            string stackTrace = exception.StackTrace;
            if (!String.IsNullOrEmpty(stackTrace))
                result += Environment.NewLine + stackTrace;

            return result;
        }
    }
}
