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

        // AI Analysis Mapping Configurations
        config.NewConfig<TeamStructureResponse, TeamStructureResponse>()
            .Map(dest => dest.Roles, src => src.Roles)
            .Map(dest => dest.RequiredSkills, src => src.RequiredSkills);

        config.NewConfig<RoleRecommendation, RoleRecommendation>()
            .Map(dest => dest.Role, src => src.Role)
            .Map(dest => dest.Track, src => src.Track)
            .Map(dest => dest.RecommendedMembers, src => src.RecommendedMembers)
            .Map(dest => dest.Skills, src => src.Skills)
            .Map(dest => dest.Priority, src => src.Priority);

        config.NewConfig<TechStackResponse, TechStackResponse>()
            .Map(dest => dest.Backend, src => src.Backend)
            .Map(dest => dest.Frontend, src => src.Frontend)
            .Map(dest => dest.Database, src => src.Database)
            .Map(dest => dest.AiStack, src => src.AiStack)
            .Map(dest => dest.DevOps, src => src.DevOps);

        config.NewConfig<FullAnalysisResponse, FullAnalysisResponse>()
            .Map(dest => dest.Success, src => src.Success)
            .Map(dest => dest.TeamStructure, src => src.TeamStructure)
            .Map(dest => dest.TechStack, src => src.TechStack)
            .Map(dest => dest.TotalRoles, src => src.TotalRoles)
            .Map(dest => dest.TotalMembers, src => src.TotalMembers)
            .Map(dest => dest.ProcessingTime, src => src.ProcessingTime);
	}
}