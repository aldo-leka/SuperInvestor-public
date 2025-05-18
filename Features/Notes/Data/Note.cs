using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SuperInvestor.Features.Identity.Data;
using SuperInvestor.Features.Research.Data;

namespace SuperInvestor.Features.Notes.Data;

public class Note
{
    [Key]
    public Guid Id { get; set; }
    public string ShortId { get; set; } = GenerateShortId();

    [ForeignKey("ApplicationUser")]
    public string UserId { get; set; }

    public string Ticker { get; set; }
    public string AccessionNumber { get; set; }
    public string Text { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid? ResearchId { get; set; }
    public virtual SuperInvestor.Features.Research.Data.Research Research { get; set; }

    public virtual ApplicationUser User { get; set; }

    private static string GenerateShortId()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 11);
    }
}