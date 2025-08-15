using PetLoveCommunity.Application.DTOs;
using PetLoveCommunity.Domain.Entities;

namespace PetLoveCommunity.Application.Extensions;

public static class Mappers
{
    public static PetListDto MapToPetListDto(this Pet pet, string baseApiUrl)
    {
        return new PetListDto
        {
            Id = pet.Id,
            Name = pet.Name,
            PetType = pet.PetType.ToString(),
            Breed = pet.Breed,
            Image = $"{baseApiUrl}/images/pets/{pet.Image}",
            Price = pet.Price,
            Gender = pet.Gender.ToString(),
            AdoptionStatus = pet.AdoptionStatus.ToString(),
            IsActive = pet.IsActive,
            Views = pet.Views,
            CreatedAt = pet.CreatedAt
        };
    }

    public static PetDetailDto MapToPetDetailDto(this Pet pet, string baseApiUrl)
    {
        return new PetDetailDto
        {
            Id = pet.Id,
            Name = pet.Name,
            PetType = pet.PetType.ToString(),
            Breed = pet.Breed,
            Image = $"{baseApiUrl}/images/pets/{pet.Image}",
            DateOfBirth = pet.DateOfBirth,
            Price = pet.Price,
            Description = pet.Description,
            Gender = pet.Gender.ToString(),
            AdoptionStatus = pet.AdoptionStatus.ToString(),
            IsActive = pet.IsActive,
            Views = pet.Views,
            CreatedAt = pet.CreatedAt,
            Owner = new OwnerDto
            {
                Id = pet.Owner.Id,
                Name = $"{pet.Owner.FirstName} {pet.Owner.LastName}",
                Email = pet.Owner.Email
            },
            Photos = pet.Photos?.Select(p => new PetPhotoDto
            {
                Id = p.Id,
                PhotoUrl = $"{baseApiUrl}/images/pets/{p.PhotoUrl}",
                Caption = p.Caption,
                DisplayOrder = p.DisplayOrder,
                IsPrimary = p.IsPrimary
            }).ToList() ?? new List<PetPhotoDto>()
        };
    }

    public static IEnumerable<PetListDto> MapToPetListDtos(this IEnumerable<Pet> pets, string baseApiUrl)
    {
        return pets.Select(pet => pet.MapToPetListDto(baseApiUrl));
    }

    public static IEnumerable<PetDetailDto> MapToPetDetailDtos(this IEnumerable<Pet> pets, string baseApiUrl)
    {
        return pets.Select(pet => pet.MapToPetDetailDto(baseApiUrl));
    }
}