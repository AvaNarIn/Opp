namespace Bugtrack.Api.Classes;

public class SupportMessage
{
    public Guid Id { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public void ChangeText(string text)
    {
        Text = text;
    }
}