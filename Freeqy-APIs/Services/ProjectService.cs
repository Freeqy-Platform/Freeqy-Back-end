using Freeqy_APIs.Abstractions;
using Freeqy_APIs.Contracts.Category;
using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Contracts.Technology;
using Freeqy_APIs.Entities;
using Freeqy_APIs.Hubs;
using Microsoft.AspNetCore.SignalR;
using CategoryResponse = Freeqy_APIs.Contracts.Category.CategoryResponse;

namespace Freeqy_APIs.Services;

public class ProjectService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IHubContext<ChatHub, IChatClient> hubContext,
    IProjectHistoryService historyService,
    INotificationService notificationService) : IProjectService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext = hubContext;
    private readonly IProjectHistoryService _historyService = historyService;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<Result<PaginatedList<ProjectListItemResponse>>> GetProjectsAsync(
        ProjectRequestFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = GetActiveProjects()
            .Include(p => p.Owner)
            .Include(p => p.Category)
            .Include(p => p.Technologies)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.OwnerId))
            query = query.Where(p => p.OwnerId == filter.OwnerId);

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(p => p.Category.Name == filter.Category);

        if (filter.Status.HasValue)
            query = query.Where(p => p.Status == filter.Status.Value);

        if (filter.Visibility.HasValue)
            query = query.Where(p => p.Visibility == filter.Visibility.Value);

        if (!string.IsNullOrWhiteSpace(filter.Tech))
            query = query.Where(p => p.Technologies.Any(t => t.Name == filter.Tech));

        var paginatedList = await PaginatedList<ProjectListItemResponse>.CreateAsync(
            query.ProjectToType<ProjectListItemResponse>(),
            filter.PageNumber,
            filter.PageSize,
            cancellationToken);

        return Result.Success(paginatedList);
    }

    // Should Review This Service
    public async Task<Result<ProjectItemRespone>> GetProjectByIdAsync(string id, 
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Owner)
            .Include(p => p.Category)
            .Include(p => p.Technologies)
            .Include(p => p.ProjectMembers)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    
        if (project == null)
            return Result.Failure<ProjectItemRespone>(ProjectErrors.NotFound);
    
        var response = project.Adapt<ProjectItemRespone>();
    
        return Result.Success(response);
    }

    public async Task<Result> ChangeProjectStatusAsync(string userId, string id, ChangeProjectStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects.FindAsync(id, cancellationToken);
        
        if (project == null) 
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
        {
            return Result.Failure(UserErrors.NoAuthenticate);
        }

        if (project.Status != request.ProjectStatus)
        {
            project.Status = request.ProjectStatus;
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Notify all project members about the status change
            var memberIds = await _dbContext.ProjectMembers
                .Where(pm => pm.ProjectId == id && pm.IsActive)
                .Select(pm => pm.UserId)
                .ToListAsync(cancellationToken);

            await _notificationService.SendToManyAsync(
                recipientIds: memberIds,
                actorId: userId,
                type: NotificationType.ProjectStatusChanged,
                title: "Project Status Updated",
                message: $"Project \"{project.Name}\" status changed to {request.ProjectStatus}",
                entityType: "Project",
                entityId: project.Id,
                ct: cancellationToken);

            if (request.ProjectStatus == ProjectStatus.Completed)
            {
                await _historyService.RecordEventAsync(
                    userId,
                    project.Id,
                    project.Name,
                    string.Empty,
                    HistoryEventType.ProjectCompleted,
                    role: "Owner",
                    projectStatusAtEvent: ProjectStatus.Completed,
                    ct: cancellationToken);

                var members = await _dbContext.ProjectMembers
                    .Where(pm => pm.ProjectId == id && pm.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var m in members)
                {
                    await _historyService.RecordEventAsync(
                        m.UserId,
                        project.Id,
                        project.Name,
                        string.Empty,
                        HistoryEventType.ProjectCompleted,
                        role: m.Role,
                        projectStatusAtEvent: ProjectStatus.Completed,
                        ct: cancellationToken);
                }
            }
        }
            
        return  Result.Success();
    }

    public async Task<Result<CategoryResponse>> AddCategoryAsync(CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var isExist = await _dbContext.Categories.AnyAsync(c => c.Name == request.Name, cancellationToken);
        
        if (isExist)
            return Result.Failure<CategoryResponse>(CategoryErrors.DuplicateName);
        
        Category category = request.Adapt<Category>();
        
        await _dbContext.Categories.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success(category.Adapt<CategoryResponse>());
    }

    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(string id,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories.FindAsync(id, cancellationToken);
        
        if (category == null)
            return Result.Failure<CategoryResponse>(CategoryErrors.NotFound);
        
        return  Result.Success(category.Adapt<CategoryResponse>());
    }
    
    public async Task<Result<TechnologyResponse>> AddTechnologyAsync(TechnologyRequest request, CancellationToken cancellationToken = default)
    {
        var isExist = await _dbContext.Technologies.AnyAsync(c => c.Name == request.Name, cancellationToken);
        
        if (isExist)
            return Result.Failure<TechnologyResponse>(TechnologyErrors.DuplicateName);
        
        var technology = request.Adapt<Technology>();
        
        await _dbContext.Technologies.AddAsync(technology, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success(technology.Adapt<TechnologyResponse>());
    }

    public async Task<Result<TechnologyResponse>> GetTechnologyByIdAsync(string id,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Technologies.FindAsync(id, cancellationToken);
        
        if (category == null)
            return Result.Failure<TechnologyResponse>(TechnologyErrors.NotFound);
        
        return  Result.Success(category.Adapt<TechnologyResponse>());
    }

    public async Task<Result<List<TechnologyResponse>>> GetTechnologiesAsync(CancellationToken cancellationToken = default)
    {
        var technologies = await _dbContext
            .Technologies
            .AsNoTracking()
            .ProjectToType<TechnologyResponse>()
            .ToListAsync(cancellationToken);
        
        return Result.Success(technologies);
    }
    
    public async Task<Result<List<CategoryResponse>>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext
            .Categories
            .AsNoTracking()
            .ProjectToType<CategoryResponse>()
            .ToListAsync(cancellationToken);
        
        return Result.Success(categories);
    }

    public async Task<Result<ProjectListItemResponse>> AddProjectAsync(string userId, ProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var isExistingProjectName = await _dbContext.Projects.AnyAsync(p => p.Name == request.Name, cancellationToken);

        if (isExistingProjectName) return Result.Failure<ProjectListItemResponse>(ProjectErrors.DuplicateName);
        
        var category = await _dbContext.Categories.FindAsync(request.CategoryId, cancellationToken);
        if (category is null) 
            return Result.Failure<ProjectListItemResponse>(CategoryErrors.NotFound);

        var technologies = await _dbContext.Technologies
            .Where(x => request.TechnologyIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (technologies.Count != request.TechnologyIds.Count)
            return Result.Failure<ProjectListItemResponse>(TechnologyErrors.NotFound);
        
        var user = _userManager.FindByIdAsync(userId).Result;
        
        Project project = request.Adapt<Project>();
        project.Category = category;
        project.Technologies = technologies;
        project.Owner =  user!;
        project.OwnerId  = userId; 
        
        await _dbContext.Projects.AddAsync(project, cancellationToken);

        // Auto-create team chat for the new project
        var teamConversation = new Conversation
        {
            Type = ConversationType.ProjectTeam,
            ProjectId = project.Id,
            Title = project.Name,
            ChannelName = "General",
            CreatedByUserId = userId,
            Participants =
            [
                new ConversationParticipant
                {
                    UserId = userId,
                    Role = ParticipantRole.Admin
                }
            ]
        };

        var systemMessage = new Message
        {
            ConversationId = teamConversation.Id,
            SenderId = userId,
            Content = "Team chat created.",
            Type = MessageType.System
        };

        teamConversation.LastMessageAt = systemMessage.CreatedAt;
        _dbContext.Conversations.Add(teamConversation);
        _dbContext.Messages.Add(systemMessage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _historyService.RecordEventAsync(
            userId,
            project.Id,
            project.Name,
            category.Name,
            HistoryEventType.Joined,
            role: "Owner",
            projectStatusAtEvent: ProjectStatus.Pending,
            ct: cancellationToken);
        
        return Result.Success(project.Adapt<ProjectListItemResponse>());
    }

    public async Task<Result> UpdateProjectAsync(
          string projectId,
          string userId,
          ProjectRequest request,
          CancellationToken cancellationToken = default)
    {
        var project = await GetActiveProjects()
            .Include(p => p.Technologies)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        var isDuplicateName = await GetActiveProjects()
            .AnyAsync(p => p.Name == request.Name && p.Id != projectId, cancellationToken);

        if (isDuplicateName)
            return Result.Failure(ProjectErrors.DuplicateName);

        var category = await _dbContext.Categories.FindAsync(new object[] { request.CategoryId },
            cancellationToken: cancellationToken);
        if (category is null)
            return Result.Failure(CategoryErrors.NotFound);

        var technologies = await _dbContext.Technologies
            .Where(x => request.TechnologyIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (technologies.Count != request.TechnologyIds.Count)
            return Result.Failure(TechnologyErrors.NotFound);

        project.Name = request.Name;
        project.Description = request.Description;
        project.CategoryId = request.CategoryId;
        project.Category = category;
        project.UpdatedAt = DateTime.UtcNow;
        project.Status = request.Status;
        project.Visibility = request.Visibility;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;

        project.Technologies.Clear();
        project.Technologies = technologies;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    
    public async Task<Result> DeleteProjectAsync(string projectId, string userId,
        CancellationToken cancellationToken = default)
    {
        var project = await GetActiveProjects()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        var activeMembers = await _dbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        await _historyService.RecordEventAsync(
            userId,
            project.Id,
            project.Name,
            project.Category?.Name ?? string.Empty,
            HistoryEventType.ProjectDeleted,
            role: "Owner",
            projectStatusAtEvent: project.Status,
            ct: cancellationToken);

        foreach (var member in activeMembers.Where(pm => pm.UserId != userId))
        {
            await _historyService.RecordEventAsync(
                member.UserId,
                project.Id,
                project.Name,
                project.Category?.Name ?? string.Empty,
                HistoryEventType.ProjectDeleted,
                role: member.Role,
                projectStatusAtEvent: project.Status,
                ct: cancellationToken);
        }

        project.DeletedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RestoreProjectAsync(string projectId, string userId,
    CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        if (!project.IsDeleted)
            return Result.Failure(ProjectErrors.NotDeleted);

        project.DeletedAt = null;
        project.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
    public async Task<Result> ChangeProjectVisibilityAsync(string projectId, string userId, CancellationToken cancellationToken = default)
    {
        var project = await GetActiveProjects()
          .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        if (project.Visibility == ProjectVisibility.Public)
            project.Visibility = ProjectVisibility.Private;
        else
            project.Visibility = ProjectVisibility.Public;

        project.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }


    private IQueryable<Project> GetActiveProjects()
    {
        return _dbContext.Projects.Where(p => p.DeletedAt == null);
    }

    public async Task<Result> RemoveMemberFromProject(string projectId, string userId, string memberId, CancellationToken cancellationToken = default)
    {
        var project = await GetActiveProjects()
         .Include(p => p.Category)
         .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound); 

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        var projectMember = await _dbContext.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == memberId, cancellationToken);

        if (projectMember is null)
            return Result.Failure(ProjectErrors.MemberNotFound);

        _dbContext.ProjectMembers.Remove(projectMember);

        // Auto-remove member from ALL project channels and post system message
        var teamChannels = await _dbContext.Conversations
            .Include(c => c.Participants)
            .Where(c => c.Type == ConversationType.ProjectTeam && c.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        var removedUser = await _userManager.FindByIdAsync(memberId);

        foreach (var channel in teamChannels)
        {
            var chatParticipant = channel.Participants
                .FirstOrDefault(p => p.UserId == memberId);

            if (chatParticipant is not null)
            {
                _dbContext.ConversationParticipants.Remove(chatParticipant);

                var systemMsg = new Message
                {
                    ConversationId = channel.Id,
                    SenderId = userId,
                    Content = $"{removedUser?.FirstName} {removedUser?.LastName} was removed from the team.",
                    Type = MessageType.System
                };
                _dbContext.Messages.Add(systemMsg);
                channel.LastMessageAt = systemMsg.CreatedAt;

                // Notify remaining participants via SignalR
                foreach (var p in channel.Participants.Where(p => p.UserId != memberId))
                {
                    await _hubContext.Clients.User(p.UserId)
                        .ReceiveMessage(channel.Id, new MessageResponse(
                            systemMsg.Id, systemMsg.SenderId, "System", null,
                            systemMsg.Content, systemMsg.Type.ToString(),
                            systemMsg.CreatedAt, null, false));
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Notify the removed member
        await _notificationService.SendAsync(
            recipientId: memberId,
            actorId: userId,
            type: NotificationType.MemberRemoved,
            title: "Removed from Project",
            message: $"You were removed from project \"{project.Name}\"",
            entityType: "Project",
            entityId: projectId,
            priority: NotificationPriority.High,
            ct: cancellationToken);

        await _historyService.RecordEventAsync(
            memberId,
            projectId,
            project.Name,
            project.Category?.Name ?? string.Empty,
            HistoryEventType.Left,
            role: projectMember.Role,
            projectStatusAtEvent: project.Status,
            ct: cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ProjectMembersResponse>> GetProjectMembersAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId && p.DeletedAt == null, cancellationToken);

        if (project is null)
            return Result.Failure<ProjectMembersResponse>(ProjectErrors.NotFound);

        var members = await _dbContext.ProjectMembers
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId)
            .AsNoTracking()
            .Select(pm => new ProjectMemberDto(
                pm.UserId,
                pm.User.FirstName,
                pm.User.LastName,
                pm.User.Email,
                pm.User.PhotoUrl,
                pm.Role,
                pm.IsActive,
                pm.JoinedAt
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(new ProjectMembersResponse(members));
    }

    public async Task<Result> UpdateMemberRoleAsync(string projectId, string userId, string memberId, UpdateMemberRoleRequest request, CancellationToken cancellationToken = default)
    {
        var project = await GetActiveProjects()
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        if (memberId == userId)
            return Result.Failure(new Error("Project.CannotChangeOwnRole", "You cannot change your own role", StatusCodes.Status400BadRequest));

        var projectMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == memberId, cancellationToken);

        if (projectMember is null)
            return Result.Failure(ProjectErrors.MemberNotFound);

        projectMember.Role = request.Role;
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Notify the member about their role change
        await _notificationService.SendAsync(
            recipientId: memberId,
            actorId: userId,
            type: NotificationType.MemberRoleChanged,
            title: "Role Updated",
            message: $"Your role in project \"{project.Name}\" has been changed to {request.Role}",
            entityType: "Project",
            entityId: projectId,
            ct: cancellationToken);

        await _historyService.RecordEventAsync(
            memberId,
            projectId,
            project.Name,
            string.Empty,
            HistoryEventType.RoleChanged,
            role: request.Role,
            projectStatusAtEvent: project.Status,
            ct: cancellationToken);

        return Result.Success();
    }
}