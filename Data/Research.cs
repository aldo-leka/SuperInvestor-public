using System.ComponentModel.DataAnnotations;

namespace SuperInvestor.Data;

public class Research
{
    [Key]
    public Guid Id { get; set; }
    public string ShortId { get; set; } = GenerateShortId();
    public string UserId { get; set; }
    public string Ticker { get; set; }
    public string AccessionNumber { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual ICollection<Note> Notes { get; set; }

    private static string GenerateShortId()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 8);
    }
}