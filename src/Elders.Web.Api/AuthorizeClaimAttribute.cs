using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace Elders.Web.Api
{
    /// <summary>
    ///  Specifies that a data field value is required. Only users with 'admin' scope can set it.
    ///  If the accessing identity has 'owner' scope the field value is prepopulated automagically from the 'sub' claim.
    /// </summary>
    public class AuthorizeClaimAttribute : ValidationAttribute
    {

        public override bool RequiresValidationContext { get { return true; } }

        public List<string> ClaimTypes { get; private set; }
        public AuthorizeClaimAttribute(params string[] claimTypes)
        {
            ClaimTypes = new List<string>(claimTypes);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var scopes = ClaimsPrincipal.Current.Claims.Where(x => x.Type == "scope").ToList();
            var hasAdminScope = scopes.Any(x => x.Value == "admin");
            var hasOwnerScope = scopes.Any(x => x.Value == "owner");
            if (hasAdminScope)
                return AdminValidationResult(value, validationContext);
            else if (hasOwnerScope)
                return OwnerValidationResult(value, validationContext);
            else
                return new ValidationResult("An 'admin' or 'owner' scope is required for this request. Owner scope is usually issued for ROClients and Admin scope is usually issued for B2B Clients. For further details contact your administrator");

        }

        private ValidationResult OwnerValidationResult(object value, ValidationContext validationContext)
        {

            var claim = ClaimsPrincipal.Current.Claims.Where(x => ClaimTypes.Any(y => y == x.Type)).FirstOrDefault();
            if (claim == null)
                return new ValidationResult(string.Format("The issued token does not contain '{0}'", string.Join(" | ", ClaimTypes)));
            var modelType = validationContext.ObjectInstance.GetType();
            var member = modelType.GetProperty(validationContext.MemberName);
            if (member.PropertyType == typeof(Guid))
            {
                var parsed = Guid.Parse(claim.Value);
                if (!default(Guid).Equals(value) && !parsed.Equals(value))
                    return new ValidationResult(string.Format("The parameter value does not match the claim value. Parameter '{0}' : '{1}' | Claim '{2}' : '{3}'", validationContext.MemberName, value, ClaimTypes, claim.Value));
                member.SetValue(validationContext.ObjectInstance, Guid.Parse(claim.Value));
            }
            else if (member.PropertyType == typeof(List<Guid>))
            {
                var values = claim.Value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => Guid.Parse(x)).ToList();
                member.SetValue(validationContext.ObjectInstance, values);
            }
            else if (member.PropertyType == typeof(string))
            {
                if (ReferenceEquals(null, value) == false && !claim.Value.Equals(value))
                    return new ValidationResult(string.Format("The parameter value does not match the claim value. Parameter '{0}' : '{1}' | Claim '{2}' : '{3}'", validationContext.MemberName, value, ClaimTypes, claim.Value));
                member.SetValue(validationContext.ObjectInstance, claim.Value);
            }
            else
                throw new NotImplementedException("Unkown type. Please describe the property type");
            return ValidationResult.Success;
        }

        private ValidationResult AdminValidationResult(object value, ValidationContext validationContext)
        {
            var claim = ClaimsPrincipal.Current.Claims.Where(x => ClaimTypes.Any(y => y == x.Type)).FirstOrDefault();
            if (claim != null)
            {
                var modelType = validationContext.ObjectInstance.GetType();
                var member = modelType.GetProperty(validationContext.MemberName);
                if (member.PropertyType == typeof(Guid))
                {
                    var parsed = Guid.Parse(claim.Value);
                    member.SetValue(validationContext.ObjectInstance, Guid.Parse(claim.Value));
                }
                else if (member.PropertyType == typeof(string))
                {
                    member.SetValue(validationContext.ObjectInstance, claim.Value);
                }
                else
                    throw new NotImplementedException("Unkown type. Please describe the property type");
            }
            else if (ReferenceEquals(null, value) || value.Equals(GetDefaultValue(value.GetType())))
            {
                return new ValidationResult(String.Format("The {0} property is required", validationContext.MemberName));
            }
            return ValidationResult.Success;

        }

        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }

    public class FromToken : System.Web.Http.ModelBinding.ModelBinderAttribute
    {
        public List<string> ClaimTypes { get; private set; }
        public FromToken(params string[] claimTypes)
        {
            ClaimTypes = new List<string>(claimTypes);
        }

        public override IEnumerable<System.Web.Http.ValueProviders.ValueProviderFactory> GetValueProviderFactories(System.Web.Http.HttpConfiguration configuration)
        {
            yield return new CompositeValueProviderFactory(configuration, ClaimTypes);
        }


    }
    public class CompositeValueProviderFactory : System.Web.Http.ValueProviders.ValueProviderFactory
    {
        private readonly System.Web.Http.HttpConfiguration configuration;
        public List<string> ClaimTypes { get; private set; }

        List<System.Web.Http.ValueProviders.ValueProviderFactory> ValueProviderFactories = new List<System.Web.Http.ValueProviders.ValueProviderFactory>();

        public CompositeValueProviderFactory(System.Web.Http.HttpConfiguration configuration, List<string> claimTypes)
        {
            ValueProviderFactories.AddRange(new FromUriAttribute().GetValueProviderFactories(configuration));
            this.configuration = configuration;
            ClaimTypes = claimTypes;
        }

        public override System.Web.Http.ValueProviders.IValueProvider GetValueProvider(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            return new CustomValueProvider(ValueProviderFactories, actionContext, ClaimTypes);
        }
    }

    public class CustomValueProvider : System.Web.Http.ValueProviders.IValueProvider
    {
        private readonly System.Web.Http.Controllers.HttpActionContext actionContext;
        public List<string> ClaimTypes { get; private set; }
        List<System.Web.Http.ValueProviders.ValueProviderFactory> valueProviderFactories = new List<System.Web.Http.ValueProviders.ValueProviderFactory>();
        public CustomValueProvider(
            List<System.Web.Http.ValueProviders.ValueProviderFactory> valueProviderFactories,
            System.Web.Http.Controllers.HttpActionContext actionContext,
            List<string> claimTypes)
        {
            this.actionContext = actionContext;
            this.valueProviderFactories = valueProviderFactories;
            ClaimTypes = claimTypes;
        }

        public bool ContainsPrefix(string prefix)
        {
            return true;
        }

        public System.Web.Http.ValueProviders.ValueProviderResult GetValue(string key)
        {
            var scopes = ClaimsPrincipal.Current.Claims.Where(x => x.Type == "scope").ToList();
            var hasOwnerScope = scopes.Any(x => x.Value == "owner");
            var hasAdminScope = scopes.Any(x => x.Value == "admin");
            if (!hasAdminScope && hasOwnerScope)
            {
                var subjectClaim = ClaimsPrincipal.Current.Claims.Where(x => ClaimTypes.Any(y => y == x.Type)).FirstOrDefault();

                return new System.Web.Http.ValueProviders.ValueProviderResult(subjectClaim.Value, subjectClaim.Value, CultureInfo.InvariantCulture);
            }
            if (hasAdminScope)
            {
                var subjectClaim = ClaimsPrincipal.Current.Claims.Where(x => ClaimTypes.Any(y => y == x.Type)).FirstOrDefault();
                if (subjectClaim != null)
                    return new System.Web.Http.ValueProviders.ValueProviderResult(subjectClaim.Value, subjectClaim.Value, CultureInfo.InvariantCulture);
                foreach (var item in valueProviderFactories)
                {

                    var provider = item.GetValueProvider(actionContext);
                    var value = provider.GetValue(key);
                    if (value != null)
                        return value;
                }

            }
            if (!hasAdminScope && !hasOwnerScope)
            {
                var message = "An 'admin' or 'owner' scope is required for this request. Owner scope is usually issued for ROClients and Admin scope is usually issued for B2B Clients. For further details contact your administrator";
                var responseMessage = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.NotAcceptable, new ResponseResult(message));
                actionContext.Response = responseMessage;
            }
            return null;

        }
    }

}
