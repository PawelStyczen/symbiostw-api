namespace DanceApi.Dto
{
    public class TypeOfMeetingReadDto : BaseReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        
        public bool IsHighlighted { get; set; }
        public bool IsVisible { get; set; }
        public bool IsIndividual { get; set; }
        
        public bool IsSolo { get; set; }
        
        public bool IsEvent { get; set; }
        
    }

    public class TypeOfMeetingCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public decimal Price { get; set; }
        
        public bool IsIndividual { get; set; }
        public bool IsSolo { get; set; }
        
        
        public bool IsHighlighted { get; set; }
        public bool IsVisible { get; set; }
    }

    public class TypeOfMeetingUpdateDto
    {
     
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public decimal Price { get; set; }
        
        public bool IsIndividual { get; set; }
        public bool IsSolo { get; set; }
        
        public bool IsHighlighted { get; set; }
        public bool IsVisible { get; set; }
        
        public bool IsEvent { get; set; }
    }
}