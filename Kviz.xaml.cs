using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Npgsql;


namespace UWP_Kviz
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public string[] WrongAnswers { get; set; }
    }

    public sealed partial class Kviz : Page
    {
        private List<int> displayedQuestionIds = new List<int>();
        private int totalNumberOfQuestions = 4;
        private Question currentQuestion; // Field to store the current question
        private bool isFirstPlayerTurn = true;
        private int firstPlayerScore = 0; // Track first player's score
        private int secondPlayerScore = 0; // Track second player's score
        private DispatcherTimer player1Timer;
        private DispatcherTimer player2Timer;
        private int player1TimerDuration = 60; // Duration of player 1's timer in seconds
        private int player2TimerDuration = 60; // Duration of player 2's timer in seconds
        private bool gameStarted = false;


        public Kviz()
        {
            this.InitializeComponent();
            FetchLastTwoRows();
            InitializeTimers();

        }
        private void InitializeTimers()
        {
            player1Timer = new DispatcherTimer();
            player1Timer.Interval = TimeSpan.FromSeconds(1);
            player1Timer.Tick += Player1Timer_Tick;

            player2Timer = new DispatcherTimer();
            player2Timer.Interval = TimeSpan.FromSeconds(1);
            player2Timer.Tick += Player2Timer_Tick;
        }

        private void StartPlayer1Timer()
        {
            player1Timer.Start();
        }

        private void StopPlayer1Timer()
        {
            player1Timer.Stop();
        }

        private void ResetPlayer1Timer()
        {
            player1Timer.Stop();
            player1Timer = new DispatcherTimer();
            player1Timer.Interval = TimeSpan.FromSeconds(1);
            player1Timer.Tick += Player1Timer_Tick;
        }

        private void StartPlayer2Timer()
        {
            player2Timer.Start();
        }

        private void StopPlayer2Timer()
        {
            player2Timer.Stop();
        }

        private void ResetPlayer2Timer()
        {
            player2Timer.Stop();
            player2Timer = new DispatcherTimer();
            player2Timer.Interval = TimeSpan.FromSeconds(1);
            player2Timer.Tick += Player2Timer_Tick;
        }
        private void Player1Timer_Tick(object sender, object e)
        {
            if (player1TimerDuration > 0)
            {
                // Update the UI with the remaining time for Player 1
                PrviIgracVrijeme.Text = player1TimerDuration.ToString() + "s";
                player1TimerDuration--;
            }
            else
            {
                // Player 1's timer has reached 0
                StopPlayer1Timer();
                PogresniOdgovori.Text = "IGRAC DVA JE POBJEDIO!";
                // Perform any actions you want when the timer reaches 0 for Player 1
                // For example, end the game or switch to the other player's turn
            }
        }
        private void Player2Timer_Tick(object sender, object e)
        {

            if (player2TimerDuration > 0)
            {
                // Update the UI with the remaining time for Player 2
                player2TimerDuration--;
                DrugiIgracVrijeme.Text = player2TimerDuration.ToString() + "s";

            }
            else
            {
                // Player 2's timer has reached 0
                StopPlayer2Timer();
                PogresniOdgovori.Text = "IGRAC JEDAN JE POBJEDIO!";
                // Perform any actions you want when the timer reaches 0 for Player 2
                // For example, end the game or switch to the other player's turn
            }
        }
        private void Započni_Click(object sender, RoutedEventArgs e)
        {
            if (gameStarted)
            {
                // If the game has started, do nothing and return
                return;
            }

            // Set the game as started
            gameStarted = true;
            StopPlayer1Timer();
            StopPlayer2Timer();
            ResetPlayer1Timer();
            ResetPlayer2Timer();
            if (isFirstPlayerTurn)
            {
                StartPlayer1Timer();
            }
            else
            {
                StartPlayer2Timer();
            }
            // Fetch a random question
            currentQuestion = GetRandomQuestion();

            // Ensure that a question was fetched
            if (currentQuestion != null)
            {
                // Combine correct and wrong answers into a list
                List<string> options = new List<string>();
                options.Add(currentQuestion.CorrectAnswer);
                options.AddRange(currentQuestion.WrongAnswers);

                // Shuffle the options
                Random rnd = new Random();
                options = options.OrderBy(x => rnd.Next()).ToList();

                // Update UI with the shuffled options
                questionTextBlock.Text = currentQuestion.QuestionText;

                // Check if we have at least four options
                if (options.Count >= 4)
                {
                    optionRadioButton1.Content = options[0];
                    optionRadioButton2.Content = options[1];
                    optionRadioButton3.Content = options[2];
                    optionRadioButton4.Content = options[3];

                    // Make the TextBlock and RadioButtons visible
                    Provjeri.Visibility = Visibility.Visible;
                    questionTextBlock.Visibility = Visibility.Visible;
                    optionRadioButton1.Visibility = Visibility.Visible;
                    optionRadioButton2.Visibility = Visibility.Visible;
                    optionRadioButton3.Visibility = Visibility.Visible;
                    optionRadioButton4.Visibility = Visibility.Visible;

                    // Clear error message
                    ErrorText.Text = "";
                }
                else
                {
                    // Handle the case when there are not enough options
                    ErrorText.Text = "Not enough options for the question.";
                }
            }
        }

        // Method to fetch a random question from SQLite database
        private Question GetRandomQuestion()
        {
            string connectionString = "Host=gejtejz-13872.8nj.gcp-europe-west1.cockroachlabs.cloud;Port=26257;Database=blitzbtl;Username=Mcacic;Password=NJhhoQj-IcRgyf1ffY60nQ;SSL Mode=Require;Trust Server Certificate=true";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch all questions
                    string query = "SELECT * FROM pitanja";
                    List<Question> allQuestions = new List<Question>();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Question question = new Question
                                {
                                    Id = reader.GetInt32(0),
                                    QuestionText = reader.GetString(1),
                                    CorrectAnswer = reader.GetString(2),
                                    WrongAnswers = new string[]
                                    {
                                reader.GetString(3),
                                reader.GetString(4),
                                reader.GetString(5)
                                    }
                                };
                                allQuestions.Add(question);
                            }
                        }
                    }

                    // Shuffle questions
                    Random rnd = new Random();
                    allQuestions = allQuestions.OrderBy(x => rnd.Next()).ToList();

                    // Ensure there are questions available
                    if (allQuestions.Count > 0)
                    {
                        // Select the first question (after shuffling)
                        Question selectedQuestion = allQuestions.First();

                        // Remove displayed question ID
                        displayedQuestionIds.Add(selectedQuestion.Id);
                        if (displayedQuestionIds.Count == totalNumberOfQuestions)
                        {
                            displayedQuestionIds.Clear();
                        }

                        return selectedQuestion;
                    }
                    else
                    {
                        ErrorText.Text = "No questions available.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = "An error occurred: " + ex.Message;
            }

            return null;
        }


        // Method to get the current question
        private Question GetCurrentQuestion()
        {
            return currentQuestion;
        }

        private void Provjeri_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = player1TimerDuration.ToString();
            if (player1TimerDuration == 0 || player2TimerDuration == 0)
            {
                return;
            }
            // Get the selected answer
            string selectedAnswer = GetSelectedAnswer();
            // Check if an answer is selected
            if (selectedAnswer != null)
            {
                ErrorText.Text = selectedAnswer + Environment.NewLine;
                ErrorText.Text += currentQuestion.CorrectAnswer;
                if (selectedAnswer.Equals(currentQuestion.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    if (isFirstPlayerTurn)
                        firstPlayerScore++;
                    else
                        secondPlayerScore++;

                    // Update score text blocks
                    PrviIgracBodovi.Text = firstPlayerScore.ToString();
                    DrugiIgracBodovi.Text = secondPlayerScore.ToString();
                    if (isFirstPlayerTurn)
                        StopPlayer1Timer();
                    else
                        StopPlayer2Timer();

                    isFirstPlayerTurn = !isFirstPlayerTurn;
                    if (isFirstPlayerTurn)
                        StartPlayer1Timer();
                    else
                        StartPlayer2Timer();
                    PogresniOdgovori.Text = "";
                    // Display new question
                    DisplayNewQuestion();
                }
                else
                {
                    // If the answer is wrong, display a message
                    PogresniOdgovori.Text = "Pogrešan odgovor! Pokušaj ponovno!";
                    DisplayNewQuestion();
                }
            }
            else
            {
                // If no answer is selected, display a message
                PogresniOdgovori.Text = "Molimo izaberite odgovor.";
            }
        }

        // Method to get the selected answer
        private string GetSelectedAnswer()
        {
            if (optionRadioButton1.IsChecked == true)
            {
                return optionRadioButton1.Content as string;
            }
            else if (optionRadioButton2.IsChecked == true)
            {
                return optionRadioButton2.Content as string;
            }
            else if (optionRadioButton3.IsChecked == true)
            {
                return optionRadioButton3.Content as string;
            }
            else if (optionRadioButton4.IsChecked == true)
            {
                return optionRadioButton4.Content as string;
            }
            else
            {
                return null;
            }
        }

        // Method to display a new question
        private void DisplayNewQuestion()
        {
            ErrorText.Text = "";
            PogresniOdgovori.Text = "";
            // Clear the selection of radio buttons
            optionRadioButton1.IsChecked = false;
            optionRadioButton2.IsChecked = false;
            optionRadioButton3.IsChecked = false;
            optionRadioButton4.IsChecked = false;

            // Fetch and display a new question
            currentQuestion = GetRandomQuestion(); // Update currentQuestion to the new question
            if (currentQuestion != null)
            {
                // Display the new question
                questionTextBlock.Text = currentQuestion.QuestionText;

                // Shuffle the options
                List<string> options = new List<string>();
                options.Add(currentQuestion.CorrectAnswer);
                options.AddRange(currentQuestion.WrongAnswers);
                Random rnd = new Random();
                options = options.OrderBy(x => rnd.Next()).ToList();

                // Update UI with the shuffled options
                optionRadioButton1.Content = options[0];
                optionRadioButton2.Content = options[1];
                optionRadioButton3.Content = options[2];
                optionRadioButton4.Content = options[3];
            }
        }



        // Definicija funkcije FetchLastTwoRows
        private void FetchLastTwoRows()
        {
            // Postavite svoje vlastite podatke za povezivanje s CockroachDB bazom podataka
            string connectionString = "Host=gejtejz-13872.8nj.gcp-europe-west1.cockroachlabs.cloud;Port=26257;Database=blitzbtl;Username=Mcacic;Password=NJhhoQj-IcRgyf1ffY60nQ;SSL Mode=Require;Trust Server Certificate=true";

            try
            {
                // Stvaranje veze s bazom podataka
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    // Otvaranje veze
                    connection.Open();

                    // SQL upit za dohvaćanje posljednja dva reda iz tablice OsobniPodaci
                    string query = "SELECT * FROM OsobniPodaci ORDER BY ID_Unos DESC LIMIT 2";

                    // Stvaranje naredbe koja sadrži SQL upit i povezivanje s već otvorenom vezom
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        // Izvršavanje upita i dohvaćanje rezultata
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            // Provjera da li postoje redovi u rezultatu
                            if (reader.HasRows)
                            {
                                // Brojač redova
                                int rowCounter = 0;

                                // Iteriranje kroz rezultat
                                while (reader.Read())
                                {
                                    // Dohvaćanje vrijednosti iz svakog stupca u trenutnom retku
                                    
                                    long OIB = reader.GetInt64(1);
                                    string ime = reader.GetString(2);
                                    string prezime = reader.GetString(3);

                                    // Ažuriranje tekstualnih blokova s podacima iz baze
                                    if (rowCounter == 0)
                                    {
                                        DrugiIgracOIB.Text = $"{OIB}";
                                        DrugiIgracIme.Text = $"{ime}";
                                        DrugiIgracPrezime.Text = $"{prezime}";
                                    }
                                    else if (rowCounter == 1)
                                    {
                                        PrviIgracOIB.Text = $"{OIB}";
                                        PrviIgracIme.Text = $"{ime}";
                                        PrviIgracPrezime.Text = $"{prezime}";
                                    }

                                    // Inkrementiranje brojača redova
                                    rowCounter++;
                                }
                            }
                            else
                            {
                                // Nema redova u rezultatu
                                ErrorText.Text = "No rows returned by the query.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Uhvatiti i prikazati bilo kakvu grešku koja se dogodi pri radu s bazom podataka
                ErrorText.Text = "An error occurred: " + ex.Message;
            }
        }

    }
}