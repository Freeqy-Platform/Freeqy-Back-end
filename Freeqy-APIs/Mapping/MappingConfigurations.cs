using Freeqy_APIs.Contracts.Projects;

namespace Freeqy_APIs.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email);

        config.NewConfig<ApplicationUser, SimpleUserDto>()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Name, src => src.FirstName + " " + src.LastName);

        config.NewConfig<Categories, CategoryDto>()
            .Map(dest => dest.CategoryId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<Technologies, TechnologyDto>()
            .Map(dest => dest.TechnologyId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<Tags, TagDto>()
            .Map(dest => dest.TagId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<Projects, ProjectListItemResponse>()
            .Map(dest => dest.ProjectId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Visibility, src => src.Visibility)
            .Map(dest => dest.EstimatedTime, src => src.EstimatedTime)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)

            .Map(dest => dest.Owner, src => src.Owner.Adapt<SimpleUserDto>())

			.Map(dest => dest.Technologies,
				src => src.Technologies.Select(t => t.Adapt<TechnologyDto>()).ToList())

			.Map(dest => dest.Tags, src => new List<TagDto>())

			.Map(dest => dest.MembersCount, src => src.ProjectMembers.Count)

            .Map(dest => dest.Category, src => src.Category.Adapt<CategoryDto>());

		config.NewConfig<ApplicationUser, UserProfileResponse>()
	        .Map(dest => dest.Track, src => src.Track!.Name)
	        .Map(dest => dest.Skills, src => src.Skills.Select(us => us.Skill));
	}
}