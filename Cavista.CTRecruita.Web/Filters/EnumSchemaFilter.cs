using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;

namespace Cavista.CTRecruita.Web.Filters
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum) return;

            // Determine the underlying enum type
            var underlyingType = Enum.GetUnderlyingType(context.Type);
            var rows = new List<string>();

            schema.Enum.Clear();
            schema.Type = underlyingType == typeof(long) ? "integer" : "integer";
            schema.Format = underlyingType == typeof(long) ? "int64" : "int32";

            var enumValues = Enum.GetValues(context.Type);
            int enumLength = enumValues.Length;
            for (int count = 0; count < enumLength; count++)
            {
                var value = enumValues.GetValue(count);
                var numericValue = Convert.ChangeType(value, underlyingType);
                var name = Enum.GetName(context.Type, value);
                var field = context.Type.GetField(value.ToString());
                var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
                description = description ?? name;

                if (underlyingType == typeof(long))
                {
                    schema.Enum.Add(new OpenApiLong((long)numericValue));
                }
                else
                {
                    schema.Enum.Add(new OpenApiInteger((int)numericValue));
                }

                rows.Add($"<tr><td>{numericValue}</td><td>{name}</td><td>{description}</td></tr>");

                if (count == enumLength - 1)
                {
                    schema.Description += $"<br/> <b>Possible Values:</b><br/>" +
                        $"<table><thead><tr><th>Value</th><th>Name</th><th>Description (UI Friendly)</th></tr></thead>" +
                        $"<tbody>{string.Join("", rows)}</tbody></table>";
                }
            }
        }
    }
}
