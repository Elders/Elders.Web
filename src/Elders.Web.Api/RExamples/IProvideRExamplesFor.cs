using System.Web.Http.Controllers;

namespace Elders.Web.Api
{
    public interface IProvideRExamplesFor<out TController> : IProvideRExamples where TController : IHttpController { }
}