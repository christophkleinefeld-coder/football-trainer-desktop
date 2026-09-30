using System.Windows;
using FootballTrainer.Models;
using FootballTrainer.Services;

namespace FootballTrainer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        DatabaseService.Initialize();

        var teams = DatabaseService.GetTeams();
        var players = DatabaseService.GetPlayers();
        var trainings = DatabaseService.GetTrainings();
        var exercises = DatabaseService.GetExercises();

        TeamsGrid.ItemsSource = teams;
        PlayersGrid.ItemsSource = players;
        TrainingsGrid.ItemsSource = trainings;
        ExercisesGrid.ItemsSource = exercises;

        TeamsCountText.Text = teams.Count.ToString();
        PlayersCountText.Text = players.Count.ToString();
        TrainingsCountText.Text = trainings.Count.ToString();
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e) => HeaderTitle.Text = "Dashboard";
    private void Team_Click(object sender, RoutedEventArgs e) => HeaderTitle.Text = "Mannschaft";
    private void Players_Click(object sender, RoutedEventArgs e) => HeaderTitle.Text = "Spieler";
    private void Training_Click(object sender, RoutedEventArgs e) => HeaderTitle.Text = "Trainings";
    private void Exercises_Click(object sender, RoutedEventArgs e) => HeaderTitle.Text = "Übungen";
    private void Attendance_Click(object sender, RoutedEventArgs e) => HeaderTitle.Text = "Anwesenheiten";
    private void Calendar_Click(object sender, RoutedEventArgs e) => HeaderTitle.Text = "Kalender";

    private void AddTeam_Click(object sender, RoutedEventArgs e)
    {
        var name = TeamNameTextBox.Text.Trim();
        var ageGroup = AgeGroupTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(ageGroup))
        {
            MessageBox.Show("Bitte Teamnamen und Altersgruppe eingeben.", "Eingabe erforderlich");
            return;
        }

        DatabaseService.InsertTeam(new Team
        {
            Name = name,
            AgeGroup = ageGroup
        });

        LoadData();
        TeamNameTextBox.Text = string.Empty;
        AgeGroupTextBox.Text = string.Empty;
    }

    private void AddPlayer_Click(object sender, RoutedEventArgs e)
    {
        var name = PlayerNameTextBox.Text.Trim();
        var position = PlayerPositionTextBox.Text.Trim();
        var numberText = PlayerNumberTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Bitte Spielernamen eingeben.", "Eingabe erforderlich");
            return;
        }

        int.TryParse(numberText, out var number);

        DatabaseService.InsertPlayer(new Player
        {
            Name = name,
            Position = position,
            JerseyNumber = number,
            TeamId = 1
        });

        LoadData();
        PlayerNameTextBox.Text = string.Empty;
        PlayerPositionTextBox.Text = string.Empty;
        PlayerNumberTextBox.Text = string.Empty;
    }

    private void AddTraining_Click(object sender, RoutedEventArgs e)
    {
        var title = TrainingTitleTextBox.Text.Trim();
        var date = TrainingDateTextBox.Text.Trim();
        var focus = TrainingFocusTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Bitte Trainingstitel eingeben.", "Eingabe erforderlich");
            return;
        }

        DatabaseService.InsertTraining(new TrainingSession
        {
            Title = title,
            Date = DateTime.TryParse(date, out var parsedDate) ? parsedDate : DateTime.Today,
            Focus = focus,
            TeamId = 1,
            Location = "Hauptplatz"
        });

        LoadData();
        TrainingTitleTextBox.Text = string.Empty;
        TrainingDateTextBox.Text = string.Empty;
        TrainingFocusTextBox.Text = string.Empty;
    }

    private void AddExercise_Click(object sender, RoutedEventArgs e)
    {
        var name = ExerciseNameTextBox.Text.Trim();
        var category = ExerciseCategoryTextBox.Text.Trim();
        var description = ExerciseDescriptionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Bitte Übungsnamen eingeben.", "Eingabe erforderlich");
            return;
        }

        DatabaseService.InsertExercise(new Exercise
        {
            Name = name,
            Category = category,
            Description = description,
            DurationMinutes = 20
        });

        LoadData();
        ExerciseNameTextBox.Text = string.Empty;
        ExerciseCategoryTextBox.Text = string.Empty;
        ExerciseDescriptionTextBox.Text = string.Empty;
    }
}
