using System;
using System.Web.Http.Controllers;

namespace Elders.Web.Api
{
    public class RExample : IRExample
    {
        public RExample(object example)
        {
            RType = example.GetType();
            Example = example;
        }

        public Type RType { get; set; }
        public object Example { get; set; }
    }

    public class RExample<TController> : IRExample where TController : IHttpController
    {
        public RExample(object example)
        {
            RType = example.GetType();
            Example = example;
        }

        public Type RType { get; set; }
        public object Example { get; set; }
    }
}