using System;

namespace Elders.Web.Api
{
    public interface IRExample
    {
        Type RType { get; set; }
        object Example { get; set; }
    }
}