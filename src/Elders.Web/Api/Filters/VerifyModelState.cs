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
