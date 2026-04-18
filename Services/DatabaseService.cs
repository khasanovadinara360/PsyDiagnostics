using Microsoft.Data.Sqlite;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PsyDiagnostics.Services
{
    public class DatabaseService
    {
        private string _conn = "Data Source=psy.db";
        private void InitializeDatabase(SqliteConnection db)
        {
            var cmd = db.CreateCommand();

            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Participants (
                PrisonerId                  INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName            TEXT,
                Gender              INTEGER,
                BirthDate           TEXT,
                BirthPlace          TEXT,
                Nationality         TEXT,
                Residence           TEXT,
                FamilyUpbringing    INTEGER,
                MaritalStatus       INTEGER,
                HasCloseRelatives   INTEGER,
                HasChildren         INTEGER,
                ChildrenCount       INTEGER,
                WillKeepContact     INTEGER,
                EducationLevel      INTEGER,
                HasProfession       INTEGER,
                Profession          TEXT,
                Religion            INTEGER,

                ArmyService         INTEGER,
                ArmyBranch          TEXT,
                CombatParticipation INTEGER,
                SomaticDiseases     INTEGER,
                Disability          INTEGER,
                MentalDiseases      INTEGER,
                PsychiatristRegistry INTEGER,
                Gambling            INTEGER,
                Obligations         INTEGER,
                NarcologistRegistry INTEGER,
                DrugUse             INTEGER,

                ArticleNumber       TEXT,
                ArticlePart         TEXT,
                ArticlePoint        TEXT,
                SentenceTerm        INTEGER,
                CrimeType           INTEGER,
                Recidivism          INTEGER,
                Unit                TEXT,
                Category            INTEGER,

                CurrentFeelings     INTEGER,
                AttitudeToUIS       INTEGER,
                SuicideAttempts     INTEGER,
                SelfHarmScars       INTEGER,
                RelativesSuicide    INTEGER
            );

            CREATE TABLE IF NOT EXISTS AiResults (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                PrisonerId   TEXT    NOT NULL,
                TestName     TEXT    NOT NULL,
                Score        INTEGER NOT NULL,
                Prediction   INTEGER NOT NULL,
                Probability  REAL,
                Date         TEXT    NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Results (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PrisonerId TEXT,
                TestName TEXT,
                Score INTEGER,
                Date TEXT
            );
            ";
            cmd.ExecuteNonQuery();
        }

        public Participant GetParticipant(string id)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM Participants WHERE TRIM(PrisonerId)=TRIM($id)";
            cmd.Parameters.AddWithValue("$id", id?.Trim() ?? "");

            using var r = cmd.ExecuteReader();

            if (!r.Read())
                return null;
            var count = Convert.ToInt32(r["ChildrenCount"]);

            var articleNumber = r["ArticleNumber"]?.ToString();
            var articlePart = r["ArticlePart"]?.ToString();
            var articlePoint = r["ArticlePoint"]?.ToString();
            if (articlePart.Length > 1)
            {
                articlePart = articlePart.Remove(0, 2);
            }
            if (articlePoint.Length > 1)
            {
                articlePoint = articlePoint.Remove(0, 2);

            }

            var p = new Participant
            {
                PrisonerId = r["PrisonerId"]?.ToString(),

                FullName = r["FullName"]?.ToString(),

                Gender = EnumTry(r["Gender"], Gender.НеВыбрано),

                BirthDate = DateTime.TryParse(r["BirthDate"]?.ToString(), out var d)
                    ? d
                    : DateTime.Today,

                BirthPlace = r["BirthPlace"]?.ToString(),
                Nationality = r["Nationality"]?.ToString(),
                Residence = r["Residence"]?.ToString(),

                Citizenship = EnumTry(r["Citizenship"], Citizenship.НеВыбрано),

                EducationLevel = EnumTryUnchecked(r["EducationLevel"], EducationSurvey.НеВыбрано),

                FamilyUpbringing = EnumTryUnchecked(r["FamilyUpbringing"], FamilyUpbringing.НеВыбрано),
                MaritalStatus = EnumTryUnchecked(r["MaritalStatus"], MaritalStatus.НеЖенат),

                HasCloseRelatives = EnumTryUnchecked(r["HasCloseRelatives"], YesNo.Нет),
                ChildrenCount = count,

                HasChildren = count > 0
                ? ChildrenPresence.Да
                : ChildrenPresence.Нет,

                WillKeepContact = EnumTryUnchecked(r["WillKeepContact"], YesNo.Нет),

                //ProfessionBeforeConviction = r["ProfessionBeforeConviction"]?.ToString(),

                HasProfession = EnumTry(r["HasProfession"], ProfessionPresence.Нет),
                Profession = r["Profession"]?.ToString(),

                Religion = EnumTryUnchecked(r["Religion"], Religion.НеВыбрано),

                ArmyService = EnumTryUnchecked(r["ArmyService"], default(ArmyService)),
                ArmyBranch = r["ArmyBranch"]?.ToString(),
                CombatParticipation = EnumTryUnchecked(r["CombatParticipation"], CombatParticipation.Нет),

                SomaticDiseases = EnumTryUnchecked(r["SomaticDiseases"], SomaticDiseases.Нет),
                Disability = EnumTryUnchecked(r["Disability"], Disability.Нет),
                MentalDiseases = EnumTryUnchecked(r["MentalDiseases"], MentalDiseases.Нет),
                PsychiatristRegistry = EnumTryUnchecked(r["PsychiatristRegistry"], PsychiatristRegistry.Нет),
                Gambling = EnumTryUnchecked(r["Gambling"], Gambling.Нет),

                Obligations = EnumTryUnchecked(r["Obligations"], Obligations.Нет),
                NarcologistRegistry = EnumTryUnchecked(r["NarcologistRegistry"], NarcologistRegistry.Нет),
                DrugUse = EnumTryUnchecked(r["DrugUse"], DrugUse.Нет),

                ArticleNumber = articleNumber,
                ArticlePart = articlePart,
                ArticlePoint = articlePoint,

                SentenceTerm = TryInt(r["SentenceTerm"]),

                CrimeType = EnumTryUnchecked(r["CrimeType"], CrimeType.НеВыбрано),
                Recidivism = EnumTryUnchecked(r["Recidivism"], Recidivism.Нет),
                //PreviousConvictions = TryInt(r["PreviousConvictions"]),

                Unit = r["Unit"]?.ToString(),
                Category = EnumTryUnchecked(r["Category"], Category.НеВыбрано),

                CurrentFeelings = EnumTry(r["CurrentFeelings"], CurrentFeelings.НеВыбрано),
                AttitudeToUIS = EnumTryUnchecked(r["AttitudeToUIS"], AttitudeToUIS.НеВыбрано),
                SuicideAttempts = EnumTryUnchecked(r["SuicideAttempts"], SuicideAttempts.Нет),
                SelfHarmScars = EnumTryUnchecked(r["SelfHarmScars"], SelfHarmScars.Нет),
                RelativesSuicide = EnumTryUnchecked(r["RelativesSuicide"], RelativesSuicide.Нет)
            };
            //MessageBox.Show(articleNumber + articlePart + articlePoint);
            return p;
        }

        public void SaveParticipant(Participant p)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText =
            @"INSERT OR REPLACE INTO Participants
            (PrisonerId, FullName, Gender, BirthDate, BirthPlace, Nationality, Residence,
             FamilyUpbringing, MaritalStatus, HasCloseRelatives, HasChildren, ChildrenCount,
             WillKeepContact, EducationLevel, HasProfession, Profession, Religion,
             ArmyService, ArmyBranch, CombatParticipation, SomaticDiseases, Disability,
             MentalDiseases, PsychiatristRegistry, Gambling, Obligations, NarcologistRegistry, DrugUse,
             ArticleNumber, ArticlePart, ArticlePoint, SentenceTerm, CrimeType, Recidivism, Unit, Category,
             CurrentFeelings, AttitudeToUIS, SuicideAttempts, SelfHarmScars, RelativesSuicide)
            VALUES
            ($id,$name,$gender,$birth,$place,$nat,$res,
             $fam,$mar,$rel,$hasChild,$childCount,
             $keep,$edu,$hasProf,$prof,$relg,
             $army,$armyBranch,$combat,$som,$dis,
             $ment,$psyc,$gamb,$obl,$narc,$drug,
             $artNum,$artPart,$artPoint,$term,$crime,$rec,$unit,$cat,
             $cf,$att,$suic,$scar,$relSuic)";

            cmd.Parameters.AddWithValue("$id", p.PrisonerId?.Trim() ?? "");
            cmd.Parameters.AddWithValue("$name", p.FullName ?? "");
            cmd.Parameters.AddWithValue("$gender", (int)p.Gender);
            cmd.Parameters.AddWithValue("$birth", p.BirthDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$place", p.BirthPlace ?? "");
            cmd.Parameters.AddWithValue("$nat", p.Nationality ?? "");
            cmd.Parameters.AddWithValue("$res", p.Residence ?? "");

            cmd.Parameters.AddWithValue("$fam", (int)p.FamilyUpbringing);
            cmd.Parameters.AddWithValue("$mar", (int)p.MaritalStatus);
            cmd.Parameters.AddWithValue("$rel", p.HasCloseRelatives == YesNo.Да ? 1 : 0);
            cmd.Parameters.AddWithValue("$hasChild", p.HasChildren == ChildrenPresence.Да ? 1 : 0);
            cmd.Parameters.AddWithValue("$childCount", p.ChildrenCount);

            cmd.Parameters.AddWithValue("$keep", p.WillKeepContact == YesNo.Да ? 1 : 0);
            cmd.Parameters.AddWithValue("$edu", (int)p.EducationLevel);
            cmd.Parameters.AddWithValue("$hasProf", (int)p.HasProfession);
            cmd.Parameters.AddWithValue("$prof", p.Profession ?? "");
            cmd.Parameters.AddWithValue("$relg", (int)p.Religion);

            cmd.Parameters.AddWithValue("$army", (int)p.ArmyService);
            cmd.Parameters.AddWithValue("$armyBranch", p.ArmyBranch ?? "");
            cmd.Parameters.AddWithValue("$combat", (int)p.CombatParticipation);
            cmd.Parameters.AddWithValue("$som", (int)p.SomaticDiseases);
            cmd.Parameters.AddWithValue("$dis", (int)p.Disability);
            cmd.Parameters.AddWithValue("$ment", (int)p.MentalDiseases);
            cmd.Parameters.AddWithValue("$psyc", (int)p.PsychiatristRegistry);
            cmd.Parameters.AddWithValue("$gamb", (int)p.Gambling);
            cmd.Parameters.AddWithValue("$obl", (int)p.Obligations);
            cmd.Parameters.AddWithValue("$narc", (int)p.NarcologistRegistry);
            cmd.Parameters.AddWithValue("$drug", (int)p.DrugUse);

            cmd.Parameters.AddWithValue("$artNum", p.ArticleNumber ?? "");
            cmd.Parameters.AddWithValue("$artPart", p.ArticlePart ?? "");
            cmd.Parameters.AddWithValue("$artPoint", p.ArticlePoint ?? "");
            cmd.Parameters.AddWithValue("$term", p.SentenceTerm);
            cmd.Parameters.AddWithValue("$crime", (int)p.CrimeType);
            cmd.Parameters.AddWithValue("$rec", (int)p.Recidivism);
            cmd.Parameters.AddWithValue("$unit", p.Unit ?? "");
            cmd.Parameters.AddWithValue("$cat", (int)p.Category);

            cmd.Parameters.AddWithValue("$cf", (int)p.CurrentFeelings);
            cmd.Parameters.AddWithValue("$att", (int)p.AttitudeToUIS);
            cmd.Parameters.AddWithValue("$suic", (int)p.SuicideAttempts);
            cmd.Parameters.AddWithValue("$scar", (int)p.SelfHarmScars);
            cmd.Parameters.AddWithValue("$relSuic", (int)p.RelativesSuicide);

            cmd.ExecuteNonQuery();
        }

        public void SaveTestResult(string prisonerId, string testName, int score, int prediction, double probability)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText =
            @"INSERT INTO AiResults 
            (PrisonerId, TestName, Score, Prediction, Probability, Date)
            VALUES ($id,$test,$score,$pred,$prob,$date)";

            cmd.Parameters.AddWithValue("$id", prisonerId);
            cmd.Parameters.AddWithValue("$test", testName);
            cmd.Parameters.AddWithValue("$score", score);
            cmd.Parameters.AddWithValue("$pred", prediction);
            cmd.Parameters.AddWithValue("$prob", probability);
            cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.ExecuteNonQuery();
        }

        public (Participant participant, List<ResultRecord> results, List<TestResultRecord> aiResults)
            GetFullReport(string id)
        {
            var participant = GetParticipant(id);
            var results = new List<ResultRecord>();
            var aiResults = new List<TestResultRecord>();

            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            try
            {
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
            }
            catch
            {
            }

            var cmd2 = db.CreateCommand();
            cmd2.CommandText = "SELECT * FROM AiResults WHERE PrisonerId=$id";
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
                        Probability = r["Probability"] != DBNull.Value ? Convert.ToDouble(r["Probability"]) : 0,
                        Date = r["Date"].ToString()
                    });
                }
            }

            return (participant, results, aiResults);
        }

        public List<AiData> GetAiTrainingData()
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM AiResults";

            using var r = cmd.ExecuteReader();

            var temp = new Dictionary<string, AiData>();

            while (r.Read())
            {
                string id = r["PrisonerId"].ToString();
                string test = r["TestName"].ToString();
                int score = Convert.ToInt32(r["Score"]);
                int pred = Convert.ToInt32(r["Prediction"]);

                if (!temp.ContainsKey(id))
                    temp[id] = new AiData();

                var d = temp[id];

                switch (test)
                {
                    case "Aggression": d.Aggression = score; break;
                    case "Impulsivity": d.Impulsivity = score; break;
                    case "Depression": d.Depression = score; break;
                    case "Stress": d.Stress = score; break;
                    case "Adaptation": d.Adaptation = score; break;
                    case "Anxiety": d.Anxiety = score; break;
                    case "Resilience": d.Resilience = score; break;
                    case "Hostility": d.Hostility = score; break;
                }

                d.Label = pred;
            }

            return temp.Values.ToList();
        }

        public void SeedTestParticipants(int count = 50)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var rnd = new Random();
            using var tx = db.BeginTransaction();

            for (int i = 1; i <= count; i++)
            {
                string prisonerId = $"TEST-{i:000}";
                string fullName = $"Иванов Иван Иванович {i}";
                var birthDate = DateTime.Today.AddYears(-rnd.Next(20, 60)).AddDays(rnd.Next(0, 365));
                string birthPlace = "Оренбург";
                string nationality = "Русский";
                string residence = "Оренбургская область";

                int gender = rnd.Next(0, 2);
                int familyUpbringing = rnd.Next(0, 3);
                int maritalStatus = rnd.Next(0, 4);
                int hasCloseRelatives = rnd.Next(0, 2);
                int hasChildren = rnd.Next(0, 2);
                int childrenCount = hasChildren == 1 ? rnd.Next(1, 4) : 0;
                int willKeepContact = rnd.Next(0, 2);
                int educationLevel = rnd.Next(0, 4);
                int hasProfession = rnd.Next(0, 2);
                string profession = hasProfession == 1 ? "Слесарь" : "";
                int religion = rnd.Next(0, 4);

                int armyService = rnd.Next(0, 3);
                string armyBranch = armyService == 0 ? "" : "ВС РФ";
                int combatParticipation = rnd.Next(0, 2);
                int somaticDiseases = rnd.Next(0, 2);
                int disability = rnd.Next(0, 2);
                int mentalDiseases = rnd.Next(0, 2);
                int psychiatristRegistry = rnd.Next(0, 2);
                int gambling = rnd.Next(0, 2);
                int obligations = rnd.Next(0, 2);
                int narcologistRegistry = rnd.Next(0, 2);
                int drugUse = rnd.Next(0, 2);

                string articleNumber = rnd.Next(105, 162).ToString();
                string articlePart = rnd.Next(0, 2) == 1 ? "1" : "";
                string articlePoint = rnd.Next(0, 2) == 1 ? "а" : "";
                int sentenceTerm = rnd.Next(1, 15);

                int crimeType = rnd.Next(0, 4);
                int recidivism = rnd.Next(0, 2);
                string unit = rnd.Next(1, 10).ToString();
                int category = rnd.Next(0, 4);

                int currentFeelings = rnd.Next(0, 4);
                int attitudeToUIS = rnd.Next(0, 4);
                int suicideAttempts = rnd.Next(0, 2);
                int selfHarmScars = rnd.Next(0, 2);
                int relativesSuicide = rnd.Next(0, 2);

                var cmd = db.CreateCommand();
                cmd.CommandText = @"
                INSERT OR REPLACE INTO Participants
                (PrisonerId, FullName, Gender, BirthDate, BirthPlace, Nationality, Residence,
                 FamilyUpbringing, MaritalStatus, HasCloseRelatives, HasChildren, ChildrenCount,
                 WillKeepContact, EducationLevel, HasProfession, Profession, Religion,
                 ArmyService, ArmyBranch, CombatParticipation, SomaticDiseases, Disability,
                 MentalDiseases, PsychiatristRegistry, Gambling, Obligations, NarcologistRegistry, DrugUse,
                 ArticleNumber, ArticlePart, ArticlePoint, SentenceTerm, CrimeType, Recidivism, Unit, Category,
                 CurrentFeelings, AttitudeToUIS, SuicideAttempts, SelfHarmScars, RelativesSuicide)
                VALUES
                ($id,$name,$gender,$birth,$place,$nat,$res,
                 $fam,$mar,$rel,$hasChild,$childCount,
                 $keep,$edu,$hasProf,$prof,$relg,
                 $army,$armyBranch,$combat,$som,$dis,
                 $ment,$psyc,$gamb,$obl,$narc,$drug,
                 $artNum,$artPart,$artPoint,$term,$crime,$rec,$unit,$cat,
                 $cf,$att,$suic,$scar,$relSuic)";

                cmd.Parameters.AddWithValue("$id", prisonerId);
                cmd.Parameters.AddWithValue("$name", fullName);
                cmd.Parameters.AddWithValue("$gender", gender);
                cmd.Parameters.AddWithValue("$birth", birthDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("$place", birthPlace);
                cmd.Parameters.AddWithValue("$nat", nationality);
                cmd.Parameters.AddWithValue("$res", residence);

                cmd.Parameters.AddWithValue("$fam", familyUpbringing);
                cmd.Parameters.AddWithValue("$mar", maritalStatus);
                cmd.Parameters.AddWithValue("$rel", hasCloseRelatives);
                cmd.Parameters.AddWithValue("$hasChild", hasChildren);
                cmd.Parameters.AddWithValue("$childCount", childrenCount);

                cmd.Parameters.AddWithValue("$keep", willKeepContact);
                cmd.Parameters.AddWithValue("$edu", educationLevel);
                cmd.Parameters.AddWithValue("$hasProf", hasProfession);
                cmd.Parameters.AddWithValue("$prof", profession);
                cmd.Parameters.AddWithValue("$relg", religion);

                cmd.Parameters.AddWithValue("$army", armyService);
                cmd.Parameters.AddWithValue("$armyBranch", armyBranch);
                cmd.Parameters.AddWithValue("$combat", combatParticipation);
                cmd.Parameters.AddWithValue("$som", somaticDiseases);
                cmd.Parameters.AddWithValue("$dis", disability);
                cmd.Parameters.AddWithValue("$ment", mentalDiseases);
                cmd.Parameters.AddWithValue("$psyc", psychiatristRegistry);
                cmd.Parameters.AddWithValue("$gamb", gambling);
                cmd.Parameters.AddWithValue("$obl", obligations);
                cmd.Parameters.AddWithValue("$narc", narcologistRegistry);
                cmd.Parameters.AddWithValue("$drug", drugUse);

                cmd.Parameters.AddWithValue("$artNum", articleNumber);
                cmd.Parameters.AddWithValue("$artPart", articlePart);
                cmd.Parameters.AddWithValue("$artPoint", articlePoint);
                cmd.Parameters.AddWithValue("$term", sentenceTerm);
                cmd.Parameters.AddWithValue("$crime", crimeType);
                cmd.Parameters.AddWithValue("$rec", recidivism);
                cmd.Parameters.AddWithValue("$unit", unit);
                cmd.Parameters.AddWithValue("$cat", category);

                cmd.Parameters.AddWithValue("$cf", currentFeelings);
                cmd.Parameters.AddWithValue("$att", attitudeToUIS);
                cmd.Parameters.AddWithValue("$suic", suicideAttempts);
                cmd.Parameters.AddWithValue("$scar", selfHarmScars);
                cmd.Parameters.AddWithValue("$relSuic", relativesSuicide);

                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        private static int TryInt(object val)
        {
            if (val == null || val == DBNull.Value) return 0;
            return int.TryParse(val.ToString(), out var i) ? i : 0;
        }

        private static bool ToBool(object val)
        {
            if (val == null || val == DBNull.Value) return false;
            try { return Convert.ToInt32(val) == 1; }
            catch { return false; }
        }
        private static TEnum EnumTryUnchecked<TEnum>(object dbVal, TEnum @default) where TEnum : struct
        {
            if (dbVal == null || dbVal == DBNull.Value)
                return @default;

            // если число
            if (int.TryParse(dbVal.ToString(), out var intVal))
            {
                intVal += 1; // 🔥 СДВИГ

                if (Enum.IsDefined(typeof(TEnum), intVal))
                    return (TEnum)Enum.ToObject(typeof(TEnum), intVal);
            }

            // если строка (на всякий случай)
            if (Enum.TryParse<TEnum>(dbVal.ToString(), out var res))
                return res;

            return @default;
        }


        private static TEnum EnumTry<TEnum>(object dbVal, TEnum @default) where TEnum : struct
        {
            if (dbVal == null || dbVal == DBNull.Value) return @default;
            var s = dbVal.ToString();
            return Enum.TryParse<TEnum>(s, out var res) ? res : @default;
        }

        public double GetAverageRiskByUnit(string unit)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
        SELECT AVG(Probability)
        FROM AiResults ar
        JOIN Participants p ON p.PrisonerId = ar.PrisonerId
        WHERE p.Unit = $unit
    ";

            cmd.Parameters.AddWithValue("$unit", unit ?? "");

            var result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                return 0;

            return Convert.ToDouble(result);
        }
        public (int count, double low, double mid, double high) GetUnitStats(string unit)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
        SELECT 
            COUNT(DISTINCT p.PrisonerId),

            AVG(CASE WHEN ar.Probability <= 0.32 THEN 1.0 ELSE 0 END),
            AVG(CASE WHEN ar.Probability > 0.32 AND ar.Probability <= 0.66 THEN 1.0 ELSE 0 END),
            AVG(CASE WHEN ar.Probability > 0.66 THEN 1.0 ELSE 0 END)

        FROM Participants p
        LEFT JOIN AiResults ar 
            ON CAST(p.PrisonerId AS TEXT) = ar.PrisonerId
        WHERE p.Unit = $unit
    ";

            cmd.Parameters.AddWithValue("$unit", unit ?? "");

            using var r = cmd.ExecuteReader();

            if (r.Read())
            {
                int count = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                double low = r.IsDBNull(1) ? 0 : r.GetDouble(1);
                double mid = r.IsDBNull(2) ? 0 : r.GetDouble(2);
                double high = r.IsDBNull(3) ? 0 : r.GetDouble(3);

                return (count, low, mid, high);
            }

            return (0, 0, 0, 0);
        }

    }
}