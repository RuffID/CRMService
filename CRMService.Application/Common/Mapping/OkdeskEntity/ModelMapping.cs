using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Common.Mapping.OkdeskEntity
{
    public static class ModelMapping
    {
        private const int MAX_DESCRIPTION_LENGTH = 500;

        public static IEnumerable<ModelDto> ToDto(this IEnumerable<Model> models)
        {
            foreach (Model model in models)
                yield return model.ToDto();
        }

        public static ModelDto ToDto(this Model model)
        {
            return new ModelDto()
            {
                Id = model.Id,
                Name = model.Name,
                Code = model.Code
            };
        }

        public static IEnumerable<Model> ToEntity(this IEnumerable<ModelDto> models)
        {
            foreach (ModelDto model in models)
                yield return model.ToEntity();
        }

        public static Model ToEntity(this ModelDto dto)
        {
            Model entity = new()
            {
                Id = dto.Id,
                Code = dto.Code ?? string.Empty,
                Name = dto.Name ?? string.Empty,
                Description = TrimDescription(dto.Description),
                Visible = dto.Visible
            };

            if (dto.Kind?.Id is int kId && kId > 0)
                entity.KindId = kId;

            if (dto.Manufacturer?.Id is int mId && mId > 0)
                entity.ManufacturerId = mId;

            return entity;
        }

        private static string? TrimDescription(string? description)
        {
            if (string.IsNullOrEmpty(description))
                return description;

            return description.Length <= MAX_DESCRIPTION_LENGTH
                ? description
                : description[..MAX_DESCRIPTION_LENGTH];
        }
    }
}



