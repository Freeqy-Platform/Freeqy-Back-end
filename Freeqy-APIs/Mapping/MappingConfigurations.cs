using Freeqy_APIs.Contracts.Projects;

namespace Freeqy_APIs.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // User Registration Mapping
        config.NewConfig<RegisterRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email);

        // ApplicationUser → SimpleUserDto
        config.NewConfig<ApplicationUser, SimpleUserDto>()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Name, src => src.FirstName + " " + src.LastName);

        // Category → CategoryDto
        config.NewConfig<Categories, CategoryDto>()
            .Map(dest => dest.CategoryId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        // Technology → TechnologyDto
        config.NewConfig<Technologies, TechnologyDto>()
            .Map(dest => dest.TechnologyId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        // Tag → TagDto (Future use)
        config.NewConfig<Tags, TagDto>()
            .Map(dest => dest.TagId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        // Projects → ProjectListItemResponse
        config.NewConfig<Projects, ProjectListItemResponse>()
            .Map(dest => dest.ProjectId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Visibility, src => src.Visibility)
            .Map(dest => dest.EstimatedTime, src => src.EstimatedTime)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)

            // Owner Mapping
            .Map(dest => dest.Owner, src => src.Owner.Adapt<SimpleUserDto>())

            // Category Mapping
            .Map(dest => dest.Category, src => src.Category.Adapt<CategoryDto>())

<<<<<<< HEAD
		config.NewConfig<ApplicationUser, UserProfileResponse>()
	        .Map(dest => dest.Track, src => src.Track!.Name)
	        .Map(dest => dest.Skills, src => src.Skills.Select(us => us.Skill));
	}
}
=======
            // Technologies Mapping
            .Map(dest => dest.Technologies,
                src => src.Technologies.Select(t => t.Adapt<TechnologyDto>()).ToList())

            // You have no Tags entity in Project → return empty list
            .Map(dest => dest.Tags, src => new List<TagDto>())

            // Members Count
            .Map(dest => dest.MembersCount, src => src.ProjectMembers.Count);

        // Additional mappings
        TypeAdapterConfig<ApplicationUser, UserProfileResponse>.NewConfig();
    }
}
>>>>>>> b1ef44e4e9242e5b7a3c5c49f0db60e092ed725d
