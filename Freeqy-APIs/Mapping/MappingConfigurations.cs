using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Contracts.AiAnalysis;

namespace Freeqy_APIs.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterRequest, ApplicationUser>();
        
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
	        .Map(dest => dest.SocialLinks, src => src.SocialMediaLinks.Select(sm => new SocialMediaLinkDto(sm.Platform, sm.Link)))
	        .Map(dest => dest.Availability, src => src.Availability.ToString())
	        .Map(dest => dest.Educations, src => src.Educations.Select(e => new EducationDto(
		        e.Id,
		        e.InstitutionName,
		        e.Degree,
		        e.FieldOfStudy,
		        e.StartDate,
		        e.EndDate,
		        e.Grade,
		        e.Description
	        )))
	        .Map(dest => dest.Certificates, src => src.Certificates.Select(c => new CertificateDto(
		        c.Id,
		        c.CertificateName,
		        c.Issuer,
		        c.IssueDate,
		        c.ExpirationDate,
		        c.CredentialId,
		        c.CredentialUrl,
		        c.Description
	        )));

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