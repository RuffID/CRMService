using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.Dto.Mappers.OkdeskEntity
{
    public static class ModelMapping
    {
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
    }
}
