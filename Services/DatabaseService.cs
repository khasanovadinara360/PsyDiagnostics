using Microsoft.Data.Sqlite;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;

namespace PsyDiagnostics.Services
{
    public class DatabaseService
    {
        private string _conn = "Data Source=psy.db";

        public Participant GetParticipant(string id)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM Participants WHERE TRIM(PrisonerId) = TRIM($id)";
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
    }
}