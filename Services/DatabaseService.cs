using Microsoft.Data.Sqlite;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PsyDiagnostics.Services
{
    public class DatabaseService
    {
        private string _conn = "Data Source=psy.db";

        // ===================== INIT DB =====================

        private void InitializeDatabase(SqliteConnection db)
        {
            var cmd = db.CreateCommand();

            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Participants (
                PrisonerId TEXT PRIMARY KEY,
                FullName TEXT,
                BirthDate TEXT,
                BirthPlace TEXT,
                Citizenship TEXT,
                EducationLevel TEXT,
                MaritalStatus TEXT,
                HasChildren INTEGER,
                ProfessionBeforeConviction TEXT,
                CriminalArticle TEXT,
                SentenceTerm TEXT,
                CrimeType TEXT,
                Recidivism TEXT,
                Unit TEXT,
                Category TEXT
            );

            CREATE TABLE IF NOT EXISTS Results (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PrisonerId TEXT,
                TestName TEXT,
                Score INTEGER,
                Date TEXT
            );

            CREATE TABLE IF NOT EXISTS TestResults (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PrisonerId TEXT,
                TestName TEXT,
                Score INTEGER,
                Prediction INTEGER,
                CreatedAt TEXT
            );
            ";

            cmd.ExecuteNonQuery();
        }

        // ===================== PARTICIPANT =====================

        public Participant GetParticipant(string id)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM Participants WHERE TRIM(PrisonerId)=TRIM($id)";
            cmd.Parameters.AddWithValue("$id", id?.Trim());

            using var r = cmd.ExecuteReader();

            if (r.Read())
            {
                return new Participant
                {
                    PrisonerId = r["PrisonerId"].ToString(),
                    FullName = r["FullName"].ToString(),

                    BirthDate = DateTime.TryParse(r["BirthDate"]?.ToString(), out var d)
                        ? d : DateTime.Now,

                    BirthPlace = r["BirthPlace"].ToString(),

                    Citizenship = Enum.TryParse<Citizenship>(r["Citizenship"]?.ToString(), out var c)
                        ? c : Citizenship.РФ,

                    EducationLevel = Enum.TryParse<EducationLevel>(r["EducationLevel"]?.ToString(), out var e)
                        ? e : EducationLevel.Среднее,

                    MaritalStatus = Enum.TryParse<MaritalStatus>(r["MaritalStatus"]?.ToString(), out var m)
                        ? m : MaritalStatus.НеЖенат,

                    HasChildren = Convert.ToInt32(r["HasChildren"]) == 1,

                    ProfessionBeforeConviction = r["ProfessionBeforeConviction"].ToString(),

                    ArticleNumber = r["CriminalArticle"]?.ToString(),
                    SentenceTerm = r["SentenceTerm"]?.ToString(),

                    CrimeType = Enum.TryParse<CrimeType>(r["CrimeType"]?.ToString(), out var ct)
                        ? ct : CrimeType.НеВыбрано,

                    Recidivism = Enum.TryParse<Recidivism>(r["Recidivism"]?.ToString(), out var rec)
                        ? rec : Recidivism.Нет,

                    Unit = r["Unit"].ToString(),

                    Category = Enum.TryParse<Category>(r["Category"]?.ToString(), out var cat)
                        ? cat : Category.НеВыбрано
                };
            }

            return null;
        }

        public void SaveParticipant(Participant p)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText =
            @"INSERT OR REPLACE INTO Participants
            (PrisonerId, FullName, BirthDate, BirthPlace, Citizenship,
             EducationLevel, MaritalStatus, HasChildren, ProfessionBeforeConviction,
             CriminalArticle, SentenceTerm, CrimeType, Recidivism, Unit, Category)
            VALUES
            ($id,$name,$birth,$place,$cit,$edu,$mar,$child,$prof,$art,$term,$crime,$rec,$unit,$cat)";

            cmd.Parameters.AddWithValue("$id", p.PrisonerId?.Trim());
            cmd.Parameters.AddWithValue("$name", p.FullName);
            cmd.Parameters.AddWithValue("$birth", p.BirthDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$place", p.BirthPlace);

            cmd.Parameters.AddWithValue("$cit", p.Citizenship.ToString());
            cmd.Parameters.AddWithValue("$edu", p.EducationLevel.ToString());
            cmd.Parameters.AddWithValue("$mar", p.MaritalStatus.ToString());

            cmd.Parameters.AddWithValue("$child", p.HasChildren ? 1 : 0);
            cmd.Parameters.AddWithValue("$prof", p.ProfessionBeforeConviction);

            cmd.Parameters.AddWithValue("$art", p.CriminalArticle);
            cmd.Parameters.AddWithValue("$term", p.SentenceTerm);

            cmd.Parameters.AddWithValue("$crime", p.CrimeType.ToString());
            cmd.Parameters.AddWithValue("$rec", p.Recidivism.ToString());

            cmd.Parameters.AddWithValue("$unit", p.Unit);
            cmd.Parameters.AddWithValue("$cat", p.Category.ToString());

            cmd.ExecuteNonQuery();
        }

        // ===================== NEW AI RESULTS =====================

        public void SaveTestResult(string prisonerId, string testName, int score, int prediction)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText =
            @"INSERT INTO TestResults 
              (PrisonerId, TestName, Score, Prediction, CreatedAt)
              VALUES ($id,$test,$score,$pred,$date)";

            cmd.Parameters.AddWithValue("$id", prisonerId);
            cmd.Parameters.AddWithValue("$test", testName);
            cmd.Parameters.AddWithValue("$score", score);
            cmd.Parameters.AddWithValue("$pred", prediction);
            cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.ExecuteNonQuery();
        }

        // ===================== FULL REPORT =====================

        public (Participant participant, List<ResultRecord> results, List<TestResultRecord> aiResults)
            GetFullReport(string id)
        {
            var participant = GetParticipant(id);
            var results = new List<ResultRecord>();
            var aiResults = new List<TestResultRecord>();

            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd1 = db.CreateCommand();
            cmd1.CommandText = "SELECT * FROM Results WHERE PrisonerId=$id";
            cmd1.Parameters.AddWithValue("$id", id);

            using (var r = cmd1.ExecuteReader())
            {
                while (r.Read())
                {
                    results.Add(new ResultRecord
                    {
                        TestName = r["TestName"].ToString(),
                        Score = Convert.ToInt32(r["Score"]),
                        Date = r["Date"].ToString()
                    });
                }
            }

            var cmd2 = db.CreateCommand();
            cmd2.CommandText = "SELECT * FROM TestResults WHERE PrisonerId=$id";
            cmd2.Parameters.AddWithValue("$id", id);

            using (var r = cmd2.ExecuteReader())
            {
                while (r.Read())
                {
                    aiResults.Add(new TestResultRecord
                    {
                        TestName = r["TestName"].ToString(),
                        Score = Convert.ToInt32(r["Score"]),
                        Prediction = Convert.ToInt32(r["Prediction"]),
                        Date = r["CreatedAt"].ToString()
                    });
                }
            }

            return (participant, results, aiResults);
        }
    }
}