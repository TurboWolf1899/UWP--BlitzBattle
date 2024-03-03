using System;
using System.Data.SQLite;
using System.IO;

using Npgsql;

public static class DatabaseHelper
{
    private static string connectionString = "Host=gejtejz-13872.8nj.gcp-europe-west1.cockroachlabs.cloud;Port=26257;Database=blitzbtl;Username=Mcacic;Password=NJhhoQj-IcRgyf1ffY60nQ;SSL Mode=Require;Trust Server Certificate=true";

    public static void InitializeDatabase()
    {
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Provjerite postoji li tablica Pitanja, ako ne, stvorite ju
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Pitanja(
                            ID SERIAL PRIMARY KEY,
                            QuestionText TEXT NOT NULL,
                            CorrectAnswer TEXT NOT NULL,
                            WrongAnswer1 TEXT NOT NULL,
                            WrongAnswer2 TEXT NOT NULL,
                            WrongAnswer3 TEXT NOT NULL
                        );";
                    command.ExecuteNonQuery();
                }

                // Provjerite postoji li tablica OsobniPodaci, ako ne, stvorite ju
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS OsobniPodaci(
                            ID_Unos SERIAL PRIMARY KEY,
                            OIB BIGINT NOT NULL,
                            Ime TEXT NOT NULL,
                            Prezime TEXT NOT NULL
                        );";
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            // Obrada grešaka
        }
    }
}

