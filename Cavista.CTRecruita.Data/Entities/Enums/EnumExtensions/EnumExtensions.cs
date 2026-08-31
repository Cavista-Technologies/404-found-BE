using System.ComponentModel;
using System.Reflection;

namespace Cavista.CTRecruita.Data.Entities.Enums.EnumExtensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum? theEnum)
        {
            if (theEnum == null)
            {
                return null;
            }
            FieldInfo field = theEnum.GetType().GetField(theEnum.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? theEnum.ToString();
        }
    }
}
