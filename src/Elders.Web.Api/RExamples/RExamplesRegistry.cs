using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace Elders.Web.Api
{
    public static class RExamplesRegistry
    {
        static Dictionary<Type, IProvideRExamples> rexamples = new Dictionary<Type, IProvideRExamples>();

        public static void CollectRExamples(Assembly assemblyContainingRExamples)
        {
            var exampleProviders = assemblyContainingRExamples.ExportedTypes.Where(x => x.GetInterfaces().Any(i => typeof(IProvideRExamplesFor<IHttpController>).IsAssignableFrom(i)));

            foreach (var provider in exampleProviders)
            {
                var controller = provider.GetInterfaces().Where(x => typeof(IProvideRExamplesFor<IHttpController>).IsAssignableFrom(x)).Single().GenericTypeArguments.Single();
                if (rexamples.ContainsKey(controller) == false)
                    rexamples.Add(controller, Activator.CreateInstance(provider) as IProvideRExamples);
            }
        }

        public static IEnumerable<IRExample> GetExamples(ApiDescription apiDescription)
        {
            IProvideRExamples examplesProvider;
            if (rexamples.TryGetValue(apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType, out examplesProvider))
                return examplesProvider.GetRExamples();
            return Enumerable.Empty<RExample>();
        }
    }
}
