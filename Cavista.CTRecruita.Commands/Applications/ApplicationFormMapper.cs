using Cavista.CTRecruita.Data.Entities.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Commands.Applications
{
    public static class ApplicationFormMapper
    {
        public static FormField MapField(FormFieldDto f) => new FormField
        {
            Label = f.Label,
            Placeholder = f.Placeholder,
            FieldType = f.FieldType,
            IsRequired = f.IsRequired,
            SortOrder = f.SortOrder,
            IsStandard = f.IsStandard,
            OptionsJson = f.Options != null ? JsonSerializer.Serialize(f.Options) : null
        };
        public static string BuildSlug(string title)
        {
            var baseSlug = title.ToLower().Replace(" ", "-");
            return $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";
        }
    }
}
