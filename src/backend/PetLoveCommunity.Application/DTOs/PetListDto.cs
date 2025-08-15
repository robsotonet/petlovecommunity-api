namespace PetLoveCommunity.Application.DTOs;

public class PetListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PetType { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string AdoptionStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Views { get; set; }
    public DateTime CreatedAt { get; set; }
}