namespace DanceApi.Dto;

public abstract class BaseReadDto
{
    public string? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedDate { get; set; }

    public string? UpdatedById { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? UpdatedDate { get; set; }
}