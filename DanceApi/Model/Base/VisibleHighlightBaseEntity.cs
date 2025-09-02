namespace DanceApi.Model;

public class VisibleHighlightBaseEntity : BaseEntity
{
    public bool IsHighlighted { get; set; } = false; 
    public bool IsVisible { get; set; } = true; 
    

}