namespace DanceApi.Model;

public abstract class BaseEntity
{
    
    public int Id { get; set; }  
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    // USER ACTIONS
    public string? CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    // NAVIGATION PROPS
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
    public User? DeletedBy { get; set; }
}