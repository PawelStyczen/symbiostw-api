using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model
{
    public class FAQ : BaseEntity 
    {
        
        [Required]
        [MaxLength(200)] 
        public string Question { get; set; }
        
        [Required]
        public string Answer { get; set; } 

        public bool IsVisible { get; set; } = true; 
    }
}