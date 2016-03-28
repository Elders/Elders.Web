using System;
using System.ComponentModel.DataAnnotations;

namespace Elders.Web.Api
{
    public class GeneratedAttribute : ValidationAttribute
    {
        public override bool RequiresValidationContext { get { return true; } }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var modelType = validationContext.ObjectInstance.GetType();
            var member = modelType.GetProperty(validationContext.MemberName);
            if (value.Equals(GetDefaultValue(member.PropertyType)))
            {

                if (member.PropertyType == typeof(Guid))
                {
                    value = Guid.NewGuid();
                }
                else
                    throw new NotImplementedException("Unkown type. Please describe the property type");
                member.SetValue(validationContext.ObjectInstance, value);
            }
            return ValidationResult.Success;

        }

        public static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
