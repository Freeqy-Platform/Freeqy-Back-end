namespace Freeqy_APIs.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email);

        config.NewConfig<Entities.Projects, Contracts.Projects.ProjectListItemResponse>()
            .Map(dest => dest.ProjectId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Visibility, src => src.Visibility)
            .Map(dest => dest.EstimatedTime, src => src.EstimatedTime)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
            .Map(dest => dest.Category, src => new Contracts.Projects.CategoryDto(src.Category.Id, src.Category.Name))
            .Map(dest => dest.Owner, src => new Contracts.Projects.SimpleUserDto(src.Owner.Id, src.Owner.FirstName + " " + src.Owner.LastName))
            .Map(dest => dest.Technologies, src => src.ProjectTechnologies.Select(pt => new Contracts.Projects.TechnologyDto(pt.technology.Id, pt.technology.Name)).ToList())
            .Map(dest => dest.MembersCount, src => src.ProjectMembers.Count)
            .Map(dest => dest.Tags, src => src.ProjectMembers.Where(pm => pm.IsActive).Select(pm => new Contracts.Projects.TagDto(pm.UserId, "")).ToList()); // Placeholder for tags mapping

        config.NewConfig<Entities.Tags, Contracts.Projects.TagDto>()
            .Map(dest => dest.TagId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<Entities.Technologies, Contracts.Projects.TechnologyDto>()
            .Map(dest => dest.TechnologyId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

		TypeAdapterConfig<ApplicationUser, UserProfileResponse>.NewConfig();
	}
}