using System;
using System.Data;
using Npgsql;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UWP_Kviz
{
    public sealed partial class OsobniPodaci : Page
    {
        private int playerCount = 0;
        private string connectionString = "Host=gejtejz-13872.8nj.gcp-europe-west1.cockroachlabs.cloud;Port=26257;Username=Mcacic;Password=NJhhoQj-IcRgyf1ffY60nQ;Database=blitzbtl;SSL Mode=Require";
        private string insertQuery = "INSERT INTO OsobniPodaci (OIB, Ime, Prezime) VALUES (@OIB, @Ime, @Prezime)";
        private string selectQuery = "SELECT COUNT(*) FROM OsobniPodaci WHERE OIB = @OIB";

        public OsobniPodaci()
        {
            this.InitializeComponent();
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void Zaigraj_Click(object sender, RoutedEventArgs e)
        {
            long value1;
            string value2 = Imeunos.Text;
            string value3 = Prezimeunos.Text;

            if (string.IsNullOrEmpty(OIBunos.Text) || string.IsNullOrEmpty(value2) || string.IsNullOrEmpty(value3))
            {
                ErrorTextbox.Text = "Ups! Nedostaju podaci!";
                return;
            }
            else if (!long.TryParse(OIBunos.Text, out value1))
            {
                ErrorTextbox.Text = "OIB mora biti u brojčanom obliku!";
                return;
            }
            else if (!IsText(value2) || !IsText(value3))
            {
                ErrorTextbox.Text = "Ime i prezime moraju biti u tekstualnom obliku!";
                return;
            }

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OIB", value1);
                        int rowCount = Convert.ToInt32(command.ExecuteScalar());

                        if (rowCount > 0)
                        {
                            ErrorTextbox.Text = "Osoba s istim OIB-om već postoji!";
                            return;
                        }
                    }
                }

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OIB", value1);
                        command.Parameters.AddWithValue("@Ime", value2);
                        command.Parameters.AddWithValue("@Prezime", value3);
                        command.ExecuteNonQuery();
                    }
                }

                playerCount++;
                if (playerCount == 1)
                {
                    BrojIgraca.Text = "Drugi igrač:";
                    
                }

                OIBunos.Text = "";
                Imeunos.Text = "";
                Prezimeunos.Text = "";
                ErrorTextbox.Text = "";

                if (playerCount == 2)
                {
                    Frame.Navigate(typeof(Kviz));
                }
            }
            catch (Exception ex)
            {
                ErrorTextbox.Text = "An error occurred: " + ex.Message;
            }
        }

      

private bool IsText(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
