using Freeqy_APIs.Contracts.Projects;

namespace Freeqy_APIs.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email);
        config.NewConfig<ApplicationUser, SimpleUserDto>()
            .Map(dest => dest.Name, src => src.FirstName + " " + src.LastName);

         config.NewConfig<Projects, ProjectListItemResponse>()  
              .Map(dest => dest.Owner, src => src.Owner.Adapt<SimpleUserDto>())
              .Map(dest => dest.Category, src => src.Category.Adapt<CategoryDto>())
              .Map(dest => dest.Technologies, src => src.Technologies.Select(t => t.Adapt<TechnologyDto>()).ToList())
              .Map(dest => dest.MembersCount, src => src.ProjectMembers.Count);



        config.NewConfig<ApplicationUser, UserProfileResponse>()
	        .Map(dest => dest.Track, src => src.Track!.Name)
	        .Map(dest => dest.Skills, src => src.Skills.Select(us => us.Skill));
	}
}