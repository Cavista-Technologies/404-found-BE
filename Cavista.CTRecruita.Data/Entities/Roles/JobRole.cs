using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Cavista.CTRecruita.Data.Entities.Roles
{
    public class JobRole : BaseEntity
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Location { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Draft;
        public int NumberOfOpenings { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterEmail { get; set; }
        public string HiringManagerName { get; set; }
        public string HiringManagerEmail { get; set; }
        public JobPriority Priority { get; set; } = JobPriority.Normal;
        public DateTime? TargetHireDate { get; set; }   
        public int SlaTargetDays { get; set; }
        public string SalaryRange { get; set; }
        public string Reason { get; set; }
        [ForeignKey(nameof(Department))]
        public long DepartmentId { get; set; }
        public Department Department { get; set; }
        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
