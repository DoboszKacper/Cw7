using System.ComponentModel.DataAnnotations;

namespace APBD5.DTOs.RequestModels
{
    public class PromotionRequest
    {
        [Required]
        public string Studies { get; set; }
        [Required]
        public int Semester { get; set; }
    }
}
