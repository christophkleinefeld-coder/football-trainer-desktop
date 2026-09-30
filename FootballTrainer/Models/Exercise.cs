namespace FootballTrainer.Models;

public class TrainingSession
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Focus { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public string Location { get; set; } = string.Empty;
}
