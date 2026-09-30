using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using FootballTrainer.Models;

namespace FootballTrainer.Services;

public static class DatabaseService
{
    private const string ConnectionString = "Data Source=football_trainer.db";

    public static void Initialize()
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();

            // Teams table
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Teams (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    AgeGroup TEXT
                )";
            command.ExecuteNonQuery();

            // Players table
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Players (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Position TEXT,
                    JerseyNumber INTEGER,
                    TeamId INTEGER
                )";
            command.ExecuteNonQuery();

            // TrainingSessions table
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS TrainingSessions (
                    Id INTEGER PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Date TEXT NOT NULL,
                    Focus TEXT,
                    TeamId INTEGER,
                    Location TEXT
                )";
            command.ExecuteNonQuery();

            // Exercises table
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Exercises (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Category TEXT,
                    Description TEXT,
                    DurationMinutes INTEGER
                )";
            command.ExecuteNonQuery();

            // Videos table
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Videos (
                    Id INTEGER PRIMARY KEY,
                    FileName TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    TeamId INTEGER,
                    UploadDate TEXT NOT NULL,
                    Duration REAL,
                    EventType TEXT
                )";
            command.ExecuteNonQuery();

            // Tags table
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Tags (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Color TEXT,
                    KeyboardShortcut TEXT,
                    IsCustom INTEGER
                )";
            command.ExecuteNonQuery();

            // VideoTags table (junction for tagging)
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS VideoTags (
                    Id INTEGER PRIMARY KEY,
                    VideoId INTEGER NOT NULL,
                    TagId INTEGER NOT NULL,
                    Timestamp REAL NOT NULL,
                    PlayerId INTEGER,
                    FieldZone TEXT,
                    Notes TEXT,
                    FOREIGN KEY(VideoId) REFERENCES Videos(Id),
                    FOREIGN KEY(TagId) REFERENCES Tags(Id),
                    FOREIGN KEY(PlayerId) REFERENCES Players(Id)
                )";
            command.ExecuteNonQuery();

            // Insert default tags if not exists
            InsertDefaultTags();
        }
    }

    private static void InsertDefaultTags()
    {
        var defaultTags = new[]
        {
            new { Name = "Ballgewinn", Color = "#22c55e", Shortcut = "G" },
            new { Name = "Ballverlust", Color = "#ef4444", Shortcut = "V" },
            new { Name = "Pressing", Color = "#f59e0b", Shortcut = "P" },
            new { Name = "Umschaltmoment", Color = "#8b5cf6", Shortcut = "U" },
            new { Name = "Torabschluss", Color = "#3b82f6", Shortcut = "T" },
            new { Name = "Flanke", Color = "#06b6d4", Shortcut = "F" },
            new { Name = "Standardsituation", Color = "#ec4899", Shortcut = "S" },
            new { Name = "Spielerereignis", Color = "#14b8a6", Shortcut = "E" },
            new { Name = "Taktische Phase", Color = "#d946ef", Shortcut = "X" }
        };

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();

            foreach (var tag in defaultTags)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Tags WHERE Name = @Name";
                command.Parameters.AddWithValue("@Name", tag.Name);

                var count = (long)command.ExecuteScalar();
                if (count == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Tags (Name, Color, KeyboardShortcut, IsCustom)
                        VALUES (@Name, @Color, @Shortcut, 0)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@Name", tag.Name);
                    command.Parameters.AddWithValue("@Color", tag.Color);
                    command.Parameters.AddWithValue("@Shortcut", tag.Shortcut);
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    // Team methods
    public static List<Team> GetTeams()
    {
        var teams = new List<Team>();
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, AgeGroup FROM Teams";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    teams.Add(new Team
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        AgeGroup = reader.GetString(2)
                    });
                }
            }
        }
        return teams;
    }

    public static void InsertTeam(Team team)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Teams (Name, AgeGroup) VALUES (@Name, @AgeGroup)";
            command.Parameters.AddWithValue("@Name", team.Name);
            command.Parameters.AddWithValue("@AgeGroup", team.AgeGroup);
            command.ExecuteNonQuery();
        }
    }

    // Player methods
    public static List<Player> GetPlayers()
    {
        var players = new List<Player>();
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Position, JerseyNumber, TeamId FROM Players";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    players.Add(new Player
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Position = reader.GetString(2),
                        JerseyNumber = reader.GetInt32(3),
                        TeamId = reader.GetInt32(4)
                    });
                }
            }
        }
        return players;
    }

    public static void InsertPlayer(Player player)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Players (Name, Position, JerseyNumber, TeamId)
                VALUES (@Name, @Position, @JerseyNumber, @TeamId)";
            command.Parameters.AddWithValue("@Name", player.Name);
            command.Parameters.AddWithValue("@Position", player.Position);
            command.Parameters.AddWithValue("@JerseyNumber", player.JerseyNumber);
            command.Parameters.AddWithValue("@TeamId", player.TeamId);
            command.ExecuteNonQuery();
        }
    }

    // Training methods
    public static List<TrainingSession> GetTrainings()
    {
        var trainings = new List<TrainingSession>();
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Date, Focus, TeamId, Location FROM TrainingSessions";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    trainings.Add(new TrainingSession
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Date = DateTime.Parse(reader.GetString(2)),
                        Focus = reader.GetString(3),
                        TeamId = reader.GetInt32(4),
                        Location = reader.GetString(5)
                    });
                }
            }
        }
        return trainings;
    }

    public static void InsertTraining(TrainingSession training)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO TrainingSessions (Title, Date, Focus, TeamId, Location)
                VALUES (@Title, @Date, @Focus, @TeamId, @Location)";
            command.Parameters.AddWithValue("@Title", training.Title);
            command.Parameters.AddWithValue("@Date", training.Date.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Focus", training.Focus);
            command.Parameters.AddWithValue("@TeamId", training.TeamId);
            command.Parameters.AddWithValue("@Location", training.Location);
            command.ExecuteNonQuery();
        }
    }

    // Exercise methods
    public static List<Exercise> GetExercises()
    {
        var exercises = new List<Exercise>();
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Category, Description, DurationMinutes FROM Exercises";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    exercises.Add(new Exercise
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Category = reader.GetString(2),
                        Description = reader.GetString(3),
                        DurationMinutes = reader.GetInt32(4)
                    });
                }
            }
        }
        return exercises;
    }

    public static void InsertExercise(Exercise exercise)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Exercises (Name, Category, Description, DurationMinutes)
                VALUES (@Name, @Category, @Description, @DurationMinutes)";
            command.Parameters.AddWithValue("@Name", exercise.Name);
            command.Parameters.AddWithValue("@Category", exercise.Category);
            command.Parameters.AddWithValue("@Description", exercise.Description);
            command.Parameters.AddWithValue("@DurationMinutes", exercise.DurationMinutes);
            command.ExecuteNonQuery();
        }
    }

    // Video methods
    public static List<Video> GetVideos()
    {
        var videos = new List<Video>();
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, FileName, FilePath, TeamId, UploadDate, Duration, EventType FROM Videos";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    videos.Add(new Video
                    {
                        Id = reader.GetInt32(0),
                        FileName = reader.GetString(1),
                        FilePath = reader.GetString(2),
                        TeamId = reader.GetInt32(3),
                        UploadDate = reader.GetString(4),
                        Duration = reader.GetDouble(5),
                        EventType = reader.GetString(6)
                    });
                }
            }
        }
        return videos;
    }

    public static void InsertVideo(Video video)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Videos (FileName, FilePath, TeamId, UploadDate, Duration, EventType)
                VALUES (@FileName, @FilePath, @TeamId, @UploadDate, @Duration, @EventType)";
            command.Parameters.AddWithValue("@FileName", video.FileName);
            command.Parameters.AddWithValue("@FilePath", video.FilePath);
            command.Parameters.AddWithValue("@TeamId", video.TeamId);
            command.Parameters.AddWithValue("@UploadDate", video.UploadDate);
            command.Parameters.AddWithValue("@Duration", video.Duration);
            command.Parameters.AddWithValue("@EventType", video.EventType);
            command.ExecuteNonQuery();
        }
    }

    // Tag methods
    public static List<Tag> GetTags()
    {
        var tags = new List<Tag>();
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Color, KeyboardShortcut, IsCustom FROM Tags";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tags.Add(new Tag
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Color = reader.GetString(2),
                        KeyboardShortcut = reader.GetString(3),
                        IsCustom = reader.GetInt32(4) == 1
                    });
                }
            }
        }
        return tags;
    }

    public static void InsertTag(Tag tag)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Tags (Name, Color, KeyboardShortcut, IsCustom)
                VALUES (@Name, @Color, @KeyboardShortcut, @IsCustom)";
            command.Parameters.AddWithValue("@Name", tag.Name);
            command.Parameters.AddWithValue("@Color", tag.Color);
            command.Parameters.AddWithValue("@KeyboardShortcut", tag.KeyboardShortcut);
            command.Parameters.AddWithValue("@IsCustom", tag.IsCustom ? 1 : 0);
            command.ExecuteNonQuery();
        }
    }

    // VideoTag methods (tagging)
    public static List<VideoTag> GetVideoTags(int videoId)
    {
        var videoTags = new List<VideoTag>();
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT vt.Id, vt.VideoId, vt.TagId, vt.Timestamp, vt.PlayerId, vt.FieldZone, vt.Notes, t.Name, t.Color
                FROM VideoTags vt
                JOIN Tags t ON vt.TagId = t.Id
                WHERE vt.VideoId = @VideoId
                ORDER BY vt.Timestamp";
            command.Parameters.AddWithValue("@VideoId", videoId);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    videoTags.Add(new VideoTag
                    {
                        Id = reader.GetInt32(0),
                        VideoId = reader.GetInt32(1),
                        TagId = reader.GetInt32(2),
                        Timestamp = reader.GetDouble(3),
                        PlayerId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        FieldZone = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        Notes = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        TagName = reader.GetString(7),
                        TagColor = reader.GetString(8)
                    });
                }
            }
        }
        return videoTags;
    }

    public static void InsertVideoTag(VideoTag videoTag)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO VideoTags (VideoId, TagId, Timestamp, PlayerId, FieldZone, Notes)
                VALUES (@VideoId, @TagId, @Timestamp, @PlayerId, @FieldZone, @Notes)";
            command.Parameters.AddWithValue("@VideoId", videoTag.VideoId);
            command.Parameters.AddWithValue("@TagId", videoTag.TagId);
            command.Parameters.AddWithValue("@Timestamp", videoTag.Timestamp);
            command.Parameters.AddWithValue("@PlayerId", videoTag.PlayerId == 0 ? DBNull.Value : videoTag.PlayerId);
            command.Parameters.AddWithValue("@FieldZone", string.IsNullOrWhiteSpace(videoTag.FieldZone) ? DBNull.Value : videoTag.FieldZone);
            command.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(videoTag.Notes) ? DBNull.Value : videoTag.Notes);
            command.ExecuteNonQuery();
        }
    }

    public static void DeleteVideoTag(int videoTagId)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM VideoTags WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", videoTagId);
            command.ExecuteNonQuery();
        }
    }
}
