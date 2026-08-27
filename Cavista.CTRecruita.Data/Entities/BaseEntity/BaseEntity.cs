using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Entities.BaseEntites
{
    public abstract class BaseEntity  
    {
        public long Id { get; set; } 
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    public enum UserType 
    {
        SuperAdmin = 1,
        Recruiter,
        HiringManager
    }
    public enum UserStatus 
    { 
        Pending = 1, 
        Active,
        Inactive,
        Suspended 
    }
    //public enum EmploymentType 
    //{ 
    //    FullTime = 1, 
    //    PartTime,
    //    Contract,
    //    Internship,
    //    Temporary
    //}
    //public enum JobStatus 
    //{ 
    //    Draft = 1, 
    //    Open, 
    //    OnHold, 
    //    Closed, 
    //    Filled 
    //}
    //public enum ApplicationStage { Applied = 1, Screening = 2, Interview = 3, Assessment = 4, Offer = 5, Hired = 6, Rejected = 7, Withdrawn = 8 }
    //public enum ApplicationStatus { Active = 1, Rejected = 2, Hired = 3, Withdrawn = 4 }
    public enum InterviewMode { Onsite = 1, Phone = 2, Video = 3 }
    public enum InterviewStatus { Scheduled = 1, Completed = 2, Cancelled = 3, NoShow = 4 }
    public enum DocumentType { Resume = 1, CoverLetter = 2, Certificate = 3, Other = 99 }
}
