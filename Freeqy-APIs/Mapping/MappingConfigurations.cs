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

         config.NewConfig<Project, ProjectListItemResponse>()  
              .Map(dest => dest.Owner, src => src.Owner.Adapt<SimpleUserDto>())
              .Map(dest => dest.Category, src => src.Category.Adapt<CategoryDto>())
              .Map(dest => dest.Technologies, src => src.Technologies.Select(t => t.Adapt<TechnologyDto>()).ToList())
              .Map(dest => dest.MembersCount, src => src.ProjectMembers.Count);
         

        config.NewConfig<ApplicationUser, UserProfileResponse>()
	        .Map(dest => dest.Track, src => src.Track!.Name)
	        .Map(dest => dest.Skills, src => src.Skills.Select(us => us.Skill))
	        .Map(dest => dest.SocialLinks, src => src.SocialMediaLinks.Select(sm => new SocialMediaLinkDto(sm.Platform, sm.Link)));

        config.NewConfig<(ProjectInvitation invitation, Project project, ApplicationUser senderUser), ProjectInvitationResponse>()
            .Map(dest => dest.InviteId, src => src.invitation.Id)
            .Map(dest => dest.ProjectId, src => src.project.Id)
            .Map(dest => dest.Status, src => src.invitation.Status.ToString())
            .Map(dest => dest.InvitedUser, src => src.invitation.InvitedEmail)
            .Map(dest => dest.ExpiresAt, src => src.invitation.ExpiresAt)
            .Map(dest => dest.SentBy,
                 src => src.senderUser.FirstName + " " + src.senderUser.LastName);

      
    }
}