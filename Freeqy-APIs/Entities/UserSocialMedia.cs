using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class UserSocialMedia
{
    public long Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = default!;
}