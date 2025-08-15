namespace PetLoveCommunity.Application.DTOs;

public class PetDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PetType { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string AdoptionStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Views { get; set; }
    public DateTime CreatedAt { get; set; }
    public OwnerDto Owner { get; set; } = null!;
    public List<PetPhotoDto> Photos { get; set; } = new();
}

public class OwnerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class PetPhotoDto
{
    public Guid Id { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}