namespace Cavista.CTRecruita.Data.Entities.Enums
{
    public enum EmploymentType 
    { 
        FullTime = 1, 
        PartTime, 
        Contract, 
        Internship, 
        Temporary 
    }
    public enum JobStatus 
    { 
        Draft = 1, 
        Open, 
        OnHold, 
        Closed, 
        Filled 
    }
    public enum ApplicationStage 
    { 
        Applied = 1, 
        Screen, 
        Interview, 
        Offer, 
        Hired, 
        Rejected, 
        Withdrawn
    }
    public enum ApplicationStatus 
    { 
        Active = 1, 
        Hired, 
        Rejected, 
        Withdrawn 
    }
    public enum InterviewMode 
    { 
        Onsite = 1, 
        Phone,
        Video
    }
    public enum InterviewStatus 
    { 
        Scheduled = 1, 
        Completed, 
        Cancelled, 
        NoShow 
    }
    public enum JobPriority 
    { 
        Low = 1,
        Normal,
        High
    }

}
