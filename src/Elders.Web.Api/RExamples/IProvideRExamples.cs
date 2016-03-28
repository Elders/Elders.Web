using System.Collections.Generic;

namespace Elders.Web.Api
{
    public interface IProvideRExamples
    {
        IEnumerable<IRExample> GetRExamples();
    }
}