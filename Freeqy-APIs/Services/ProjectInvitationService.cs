using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Helper;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Freeqy_APIs.Services;

public class ProjectInvitationService(ApplicationDbContext dbContext,UserManager<ApplicationUser> userManager,
    IEmailSender emailService,
    IHttpContextAccessor httpContextAccessor) : IProjectInvitationService 
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender _emailService = emailService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<ProjectInvitationResponse>> SendInvitationAsync(string projectId,string senderId,
        SendProjectInvitationRequest request,
        CancellationToken cancellationToken = default)  
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure<ProjectInvitationResponse>(ProjectErrors.NotFound);

        if (project.OwnerId != senderId)
            return Result.Failure<ProjectInvitationResponse>(InvitationErrors.Unauthorized);

        var senderUser = await _userManager.FindByIdAsync(senderId);

        if(senderUser!.Email == request.Email)
            return Result.Failure<ProjectInvitationResponse>(InvitationErrors.SelfInvitation);

        var existingInvitation = await _dbContext.ProjectInvitations
            .FirstOrDefaultAsync(pi => pi.ProjectId == projectId && 
                                       pi.InvitedEmail == request.Email &&
                                       (pi.Status == ProjectInvitationStatus.Pending || 
                                        pi.Status == ProjectInvitationStatus.Accepted),
                cancellationToken);

        if (existingInvitation is not null)
            return Result.Failure<ProjectInvitationResponse>(InvitationErrors.AlreadyExists);

        var invitedUser = await _userManager.FindByEmailAsync(request.Email);
        if (invitedUser is not null)
        {
            var isAlreadyMember = await _dbContext.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == invitedUser.Id,
                    cancellationToken);

            if (isAlreadyMember)
                return Result.Failure<ProjectInvitationResponse>(InvitationErrors.AlreadyMember);
        }



        var invitation = new ProjectInvitation
        {
            ProjectId = projectId,
            InvitedEmail = request.Email,
            InvitedUserId = invitedUser?.Id,
            SentByUserId = senderId,
            Status = ProjectInvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _dbContext.ProjectInvitations.AddAsync(invitation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var emailSendResult = await SendInvitationEmailAsync(invitation, project, senderUser!);

        var response = (invitation, project, senderUser).Adapt<ProjectInvitationResponse>();
    
        return Result.Success(response);
    }

    public async Task<Result<UserInvitationsResponse>> GetUserInvitationsAsync(string userId,CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure<UserInvitationsResponse>(UserErrors.UserNotFound);

        var invitations = await _dbContext.ProjectInvitations
            .Include(pi => pi.Project)
            .Include(pi => pi.SentByUser)
            .Where(pi => pi.InvitedEmail == user.Email!.ToLower() || pi.InvitedUserId == userId)
            .OrderByDescending(pi => pi.CreatedAt)
            .ToListAsync(cancellationToken);

        var expiredInvitations = invitations
            .Where(pi => pi.Status == ProjectInvitationStatus.Pending && 
                        DateTime.UtcNow > pi.ExpiresAt)
            .ToList();

        foreach (var invitation in expiredInvitations)
        {
            invitation.Status = ProjectInvitationStatus.Expired;
        }

        if (expiredInvitations.Any())
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var pendingInvitations = invitations
            .Where(pi => pi.Status == ProjectInvitationStatus.Pending)
            .Select(pi => MapToInvitationDetailResponse(pi))
            .ToList();

        var respondedInvitations = invitations
            .Where(pi => pi.Status != ProjectInvitationStatus.Pending)
            .Select(pi => MapToInvitationDetailResponse(pi))
            .ToList();

        var response = new UserInvitationsResponse(
            PendingInvitations: pendingInvitations,
            RespondedInvitations: respondedInvitations
        );

        return Result.Success(response);
    }

    public async Task<Result<InvitationDetailResponse>> GetInvitationDetailsAsync(
        string invitationId,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _dbContext.ProjectInvitations
            .Include(pi => pi.Project)
            .Include(pi => pi.SentByUser)
            .FirstOrDefaultAsync(pi => pi.Id == invitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure<InvitationDetailResponse>(InvitationErrors.NotFound);

        return Result.Success(MapToInvitationDetailResponse(invitation));
    }

    public async Task<Result<RespondToInvitationResponse>> RespondToInvitationAsync(
        string userId,
        RespondToInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure<RespondToInvitationResponse>(UserErrors.UserNotFound);

        var invitation = await _dbContext.ProjectInvitations
            .Include(pi => pi.Project)
            .FirstOrDefaultAsync(pi => pi.Id == request.InvitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure<RespondToInvitationResponse>(InvitationErrors.NotFound);

        if (invitation.InvitedEmail != user.Email!.ToLower() && invitation.InvitedUserId != userId)
            return Result.Failure<RespondToInvitationResponse>(InvitationErrors.Unauthorized);

        if (invitation.Status != ProjectInvitationStatus.Pending)
            return Result.Failure<RespondToInvitationResponse>(InvitationErrors.AlreadyResponded);

        if (DateTime.UtcNow > invitation.ExpiresAt)
        {
            invitation.Status = ProjectInvitationStatus.Expired;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Failure<RespondToInvitationResponse>(InvitationErrors.Expired);
        }

        if (request.Accept)
        {
            var projectMember = new ProjectMembers
            {
                ProjectId = invitation.ProjectId,
                UserId = userId,
                Role = "Member",
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };

            await _dbContext.ProjectMembers.AddAsync(projectMember, cancellationToken);
            invitation.Status = ProjectInvitationStatus.Accepted;
            invitation.RespondedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var message = $"{user.FirstName} {user.LastName} accepted the project invitation";
            return Result.Success(new RespondToInvitationResponse(
                InviteId: invitation.Id,
                Status: "Accepted",
                Message: message
            ));
        }
        else
        {
            invitation.Status = ProjectInvitationStatus.Rejected;
            invitation.RespondedAt = DateTime.UtcNow;
            invitation.RespondedReason = request.RejectionReason;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var message = $"{user.FirstName} {user.LastName} rejected the project invitation";
            return Result.Success(new RespondToInvitationResponse(
                InviteId: invitation.Id,
                Status: "Rejected",
                Message: message
            ));
        }
    }

    public async Task<Result> ResendInvitationAsync(
        string invitationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _dbContext.ProjectInvitations
            .Include(pi => pi.Project)
            .Include(pi => pi.SentByUser)
            .FirstOrDefaultAsync(pi => pi.Id == invitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure(InvitationErrors.NotFound);

        if (invitation.Project?.OwnerId != userId && invitation.SentByUserId != userId)
            return Result.Failure(InvitationErrors.Unauthorized);

        if (invitation.Status != ProjectInvitationStatus.Pending)
            return Result.Failure(InvitationErrors.AlreadyResponded);

        invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);

        var emailResult = await SendInvitationEmailAsync(invitation, invitation.Project!, invitation.SentByUser);
        if (emailResult.IsFailure)
            return emailResult;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> CancelInvitationAsync(
        string invitationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _dbContext.ProjectInvitations
            .Include(pi => pi.Project)
            .FirstOrDefaultAsync(pi => pi.Id == invitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure(InvitationErrors.NotFound);

        if (invitation.Project?.OwnerId != userId)
            return Result.Failure(InvitationErrors.Unauthorized);

        if (invitation.Status != ProjectInvitationStatus.Pending)
            return Result.Failure(InvitationErrors.AlreadyResponded);

        _dbContext.ProjectInvitations.Remove(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ProjectInvitationsResponse>> GetProjectInvitationsAsync(string userId, string projectId, CancellationToken cancellationToken = default)
    {
        var ProjectExists = await _dbContext.Projects
            .AnyAsync(p => p.Id == projectId && p.DeletedAt == null && p.OwnerId == userId, cancellationToken);

        if (!ProjectExists)
            return Result.Failure<ProjectInvitationsResponse>(ProjectErrors.NotFound);

        var invitations = await _dbContext.ProjectInvitations
            .Include(pi => pi.SentByUser)
            .Where(pi => pi.ProjectId == projectId)
            .OrderByDescending(pi => pi.CreatedAt)
            .Select(pi => MapToInvitationDetailResponse(pi))
            .ToListAsync(cancellationToken);

        var response = new ProjectInvitationsResponse(
            Invitations: invitations
        );
        return Result.Success(response);
    }

    private async Task<Result> SendInvitationEmailAsync(
        ProjectInvitation invitation,
        Project project,
        ApplicationUser sender)
    {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;
            var invitationLink = $"{origin}/projects/invitations/{invitation.Id}";

            var emailBody = EmailBuilder.GenerateEmailBody("project-invitation",
                new Dictionary<string, string>
                {
                    { "{{senderName}}", $"{sender.FirstName} {sender.LastName}" },
                    { "{{projectName}}", project.Name },
                    { "{{projectDescription}}", project.Description },
                    { "{{acceptLink}}", invitationLink },
                    { "{{expiryDate}}", invitation.ExpiresAt.ToString("MMMM dd, yyyy") }
                });

            await _emailService.SendEmailAsync(
                invitation.InvitedEmail,
                $"Invitation to join project: {project.Name}",
                emailBody);

            return Result.Success();
    }

    private static InvitationDetailResponse MapToInvitationDetailResponse(ProjectInvitation invitation)
    {
        return new InvitationDetailResponse(
            InviteId: invitation.Id,
            ProjectId: invitation.ProjectId,
            ProjectName: invitation.Project?.Name ?? "Unknown Project",
            InvitedEmail: invitation.InvitedEmail,
            InvitedUserId: invitation.InvitedUserId,
            Status: invitation.Status.ToString(),
            CreatedAt: invitation.CreatedAt,
            ExpiresAt: invitation.ExpiresAt,
            RespondedAt: invitation.RespondedAt,
            SentByName: invitation.SentByUser != null 
                ? $"{invitation.SentByUser.FirstName} {invitation.SentByUser.LastName}" 
                : "Unknown"
        );
    }

  
}
