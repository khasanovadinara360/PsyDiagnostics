using Microsoft.Data.Sqlite;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;

namespace PsyDiagnostics.Services
{
    public class DatabaseService
    {
        private string _conn = "Data Source=psy.db";

        // =========================
        // ПОЛУЧИТЬ УЧАСТНИКА
        // =========================
        public Participant GetParticipant(string id)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM Participants WHERE PrisonerId=$id";
            cmd.Parameters.AddWithValue("$id", id);

            using var r = cmd.ExecuteReader();

            if (r.Read())
            {
                return new Participant
                {
                    Id = r["PrisonerId"].ToString(),
                    Name = r["FullName"].ToString(),

                    BirthDate = DateTime.TryParse(r["BirthDate"]?.ToString(), out var d) ? d : null,
                    BirthPlace = r["BirthPlace"].ToString(),
                    Citizenship = r["Citizenship"].ToString(),

                    EducationLevel = r["EducationLevel"].ToString(),
                    MaritalStatus = r["MaritalStatus"].ToString(),
                    HasChildren = Convert.ToInt32(r["HasChildren"]) == 1,
                    ProfessionBeforeConviction = r["ProfessionBeforeConviction"].ToString(),

                    CriminalArticle = r["CriminalArticle"].ToString(),

                    SentenceTerm = int.TryParse(r["SentenceTerm"]?.ToString(), out var s) ? s : 0,

                    CrimeType = r["CrimeType"].ToString(),
                    Recidivism = r["Recidivism"].ToString(),
                    Unit = r["Unit"].ToString(),
                    Category = r["Category"].ToString()
                };
            }

            return null;
        }

        // =========================
        // СОХРАНИТЬ УЧАСТНИКА
        // =========================
        public void SaveParticipant(Participant p)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();

            cmd.CommandText =
            @"INSERT OR REPLACE INTO Participants
            (PrisonerId, FullName, BirthDate, BirthPlace, Citizenship,
             EducationLevel, MaritalStatus, HasChildren, ProfessionBeforeConviction,
             CriminalArticle, SentenceTerm, CrimeType, Recidivism, Unit, Category)
            VALUES
            ($id,$name,$birth,$place,$cit,$edu,$mar,$child,$prof,$art,$term,$crime,$rec,$unit,$cat)";

            cmd.Parameters.AddWithValue("$id", p.Id);
            cmd.Parameters.AddWithValue("$name", p.FullName);
            cmd.Parameters.AddWithValue("$birth", p.BirthDate?.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$place", p.BirthPlace);
            cmd.Parameters.AddWithValue("$cit", p.Citizenship);
            cmd.Parameters.AddWithValue("$edu", p.EducationLevel);
            cmd.Parameters.AddWithValue("$mar", p.MaritalStatus);
            cmd.Parameters.AddWithValue("$child", p.HasChildren ? 1 : 0);
            cmd.Parameters.AddWithValue("$prof", p.ProfessionBeforeConviction);
            cmd.Parameters.AddWithValue("$art", p.CriminalArticle);
            cmd.Parameters.AddWithValue("$term", p.SentenceTerm);
            cmd.Parameters.AddWithValue("$crime", p.CrimeType);
            cmd.Parameters.AddWithValue("$rec", p.Recidivism);
            cmd.Parameters.AddWithValue("$unit", p.Unit);
            cmd.Parameters.AddWithValue("$cat", p.Category);

            cmd.ExecuteNonQuery();
        }

        // =========================
        // СОХРАНИТЬ РЕЗУЛЬТАТ ТЕСТА
        // =========================
        public void SaveResult(string prisonerId, string testName, int score)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();
            cmd.CommandText =
            @"INSERT INTO Results (PrisonerId, TestName, Score, Date)
              VALUES ($id,$test,$score,$date)";

            cmd.Parameters.AddWithValue("$id", prisonerId);
            cmd.Parameters.AddWithValue("$test", testName);
            cmd.Parameters.AddWithValue("$score", score);
            cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd"));

            cmd.ExecuteNonQuery();
        }

        // =========================
        // ПОЛНЫЙ ОТЧЁТ (участник + результаты)
        // =========================
        public (Participant participant, List<ResultRecord> results) GetFullReport(string id)
        {
            var participant = GetParticipant(id);
            var results = new List<ResultRecord>();

            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM Results WHERE PrisonerId=$id";
            cmd.Parameters.AddWithValue("$id", id);

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                results.Add(new ResultRecord
                {
                    TestName = r["TestName"].ToString(),
                    Score = Convert.ToInt32(r["Score"]),
                    Date = r["Date"].ToString()
                });
            }

            return (participant, results);
        }
    }
}