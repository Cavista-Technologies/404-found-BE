using Cavista.CTRecruita.Data.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Cavista.CTRecruita.Web.RequestModels.ApplicationModel
{
    public class UpdateApplicationStage
    {
        public ApplicationStage Stage { get; set; }
        public string Reason { get; set; }
    }
}