namespace FootballTrainer.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int JerseyNumber { get; set; }
    public int TeamId { get; set; }
}
