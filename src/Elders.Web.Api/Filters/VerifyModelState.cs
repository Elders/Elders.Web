using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;

namespace Elders.Web.Api.Filters
{
    public class VerifyModelState : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.Request.Method == HttpMethod.Post || actionContext.Request.Method == HttpMethod.Put)
            {
                var contentLength = actionContext.Request.Content.Headers.ContentLength;
                if (contentLength.HasValue == false || contentLength.Value == 0)
                {
                    var errorResult = new ResponseResult<string>("", "Missing body!");
                    var e = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, errorResult);
                    actionContext.Response = e;
                    return;
                }
            }

            var modelState = actionContext.ModelState;
            if (!modelState.IsValid)
            {
                var errorResult = new ResponseResult<ModelStateDictionary>(modelState, modelState.GetErrorMessages());
                var response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, errorResult);
                actionContext.Response = response;
            }
        }
    }
}
