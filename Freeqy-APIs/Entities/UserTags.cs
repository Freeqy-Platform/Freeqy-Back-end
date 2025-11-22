using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class UserTags
{
    public string Id { get; set; }

    public string UserId { get; set; }
    public string TagId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    [ForeignKey(nameof(TagId))]
    public Tags Tag { get; set; }
}