using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class UserSocialMedia
{
    public string Id { get; set; }

    public string UserId { get; set; }

    // e.g. "GitHub", "LinkedIn"
    public string Platform { get; set; }

    // Profile or account URL
    public string Url { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }
}