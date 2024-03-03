using System;
using System.Collections.Generic;
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
        private List<Question> questions = new List<Question>();
        private List<int> questionIndexes = new List<int>();
        private int currentQuestionIndex = -1;
        private bool isFirstPlayerTurn = true;
        private int firstPlayerScore = 0;
        private int secondPlayerScore = 0;
        private DispatcherTimer playerTimer;
        private int playerTimerDuration = 60;
        private bool gameStarted = false;
        private Random rnd = new Random();

        public Kviz()
        {
            this.InitializeComponent();
            InitializeTimers();
            InitializeButtons();
            LoadQuestionsFromDatabase();
            ShuffleQuestions();
        }

        private void InitializeButtons()
        {
            optionBtn1.Click += OptionButton_Click;
            optionBtn2.Click += OptionButton_Click;
            optionBtn3.Click += OptionButton_Click;
            optionBtn4.Click += OptionButton_Click;
            Započni.Click += Započni_Click;
        }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (currentQuestionIndex >= 0 && currentQuestionIndex < questions.Count)
            {
                Question currentQuestion = questions[questionIndexes[currentQuestionIndex]];
                string selectedAnswer = clickedButton.Content.ToString();
                if (selectedAnswer == currentQuestion.CorrectAnswer)
                {
                    if (isFirstPlayerTurn)
                    {
                        firstPlayerScore++;
                        PrviIgracBodovi.Text = firstPlayerScore.ToString();
                    }
                    else
                    {
                        secondPlayerScore++;
                        DrugiIgracBodovi.Text = secondPlayerScore.ToString();
                    }
                    StopCurrentTimer(); // Zaustavi trenutni timer
                    ChangeQuestion();
                    ChangePlayerTurn();
                }
                else
                {
                    // Ne zaustavljamo timer, nego samo mijenjamo pitanje
                    ChangeQuestion();
                }
            }
        }

        private void InitializeTimers()
        {
            playerTimer = new DispatcherTimer();
            playerTimer.Interval = TimeSpan.FromSeconds(1);
            playerTimer.Tick += PlayerTimer_Tick;
        }

        private void StartCurrentTimer()
        {
            playerTimer.Start();
        }

        private void StopCurrentTimer()
        {
            playerTimer.Stop();
        }

        private void ChangePlayerTurn()
        {
            isFirstPlayerTurn = !isFirstPlayerTurn;
        }

        private void PlayerTimer_Tick(object sender, object e)
        {
            if (playerTimerDuration > 0)
            {
                if (isFirstPlayerTurn)
                {
                    PrviIgracVrijeme.Text = playerTimerDuration.ToString() + "s";
                }
                else
                {
                    DrugiIgracVrijeme.Text = playerTimerDuration.ToString() + "s";
                }
                playerTimerDuration--;
            }
            else
            {
                StopCurrentTimer();
                if (isFirstPlayerTurn)
                {
                    PogresniOdgovori.Text = "IGRAČ DVA JE POBJEDIO!";
                }
                else
                {
                    PogresniOdgovori.Text = "IGRAČ JEDAN JE POBJEDIO!";
                }
                // Ako je isteklo vrijeme, promijeni igrača
                ChangePlayerTurn();
            }
        }

        private void Započni_Click(object sender, RoutedEventArgs e)
        {
            Započni.Visibility = Visibility.Collapsed;
            if (!gameStarted)
            {
                gameStarted = true;
                StartCurrentTimer();
                ChangeQuestion(); // Prikazi prvo pitanje odmah
            }
        }

        private void ChangeQuestion()
        {
            currentQuestionIndex++;
            if (currentQuestionIndex < questions.Count)
            {
                Question currentQuestion = questions[questionIndexes[currentQuestionIndex]];

                questionTextBlock.Text = currentQuestion.QuestionText;

                List<string> options = new List<string>();
                options.Add(currentQuestion.CorrectAnswer);
                options.AddRange(currentQuestion.WrongAnswers);

                options = options.OrderBy(x => rnd.Next()).ToList();

                optionBtn1.Content = options[0];
                optionBtn2.Content = options[1];
                optionBtn3.Content = options[2];
                optionBtn4.Content = options[3];

                questionTextBlock.Visibility = Visibility.Visible;
                optionBtn1.Visibility = Visibility.Visible;
                optionBtn2.Visibility = Visibility.Visible;
                optionBtn3.Visibility = Visibility.Visible;
                optionBtn4.Visibility = Visibility.Visible;

                ErrorText.Text = "";

                // Nastavi timer od trenutne vrijednosti
                StartCurrentTimer();
            }
            else
            {
                // End of game
                StopCurrentTimer();
            }
        }

        private void LoadQuestionsFromDatabase()
        {
            string connectionString = "Host=gejtejz-13872.8nj.gcp-europe-west1.cockroachlabs.cloud;Port=26257;Database=blitzbtl;Username=Mcacic;Password=NJhhoQj-IcRgyf1ffY60nQ;SSL Mode=Require;Trust Server Certificate=true";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM pitanja";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Question question = new Question();
                                question.Id = Convert.ToInt32(reader["id_pitanja"]);
                                question.QuestionText = reader["textpitanja"].ToString();
                                question.CorrectAnswer = reader["correctanswer"].ToString();
                                question.WrongAnswers = new string[]
                                {
                                    reader["wronganswer1"].ToString(),
                                    reader["wronganswer2"].ToString(),
                                    reader["wronganswer3"].ToString()
                                };
                                questions.Add(question);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = ex.Message;
            }
        }

        private void ShuffleQuestions()
        {
            questionIndexes = Enumerable.Range(0, questions.Count).ToList();
            int n = questionIndexes.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                int value = questionIndexes[k];
                questionIndexes[k] = questionIndexes[n];
                questionIndexes[n] = value;
            }
        }
    }
}
