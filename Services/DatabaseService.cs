using Microsoft.Data.Sqlite;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;

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
        PrisonerId INTEGER PRIMARY KEY AUTOINCREMENT,
        FullName TEXT,
        Gender INTEGER,
        BirthDate TEXT,
        BirthPlace TEXT,
        Nationality TEXT,
        Residence TEXT,
        FamilyUpbringing INTEGER,
        MaritalStatus INTEGER,
        HasCloseRelatives INTEGER,
        HasChildren INTEGER,
        ChildrenCount INTEGER,
        WillKeepContact INTEGER,
        EducationLevel INTEGER,
        HasProfession INTEGER,
        Profession TEXT,
        Religion INTEGER,
        ArmyService INTEGER,
        ArmyBranch TEXT,
        CombatParticipation INTEGER,
        SomaticDiseases INTEGER,
        Disability INTEGER,
        MentalDiseases INTEGER,
        PsychiatristRegistry INTEGER,
        Gambling INTEGER,
        Obligations INTEGER,
        NarcologistRegistry INTEGER,
        DrugUse INTEGER,
        ArticleNumber TEXT,
        ArticlePart TEXT,
        ArticlePoint TEXT,
        SentenceTerm INTEGER,
        CrimeType INTEGER,
        Recidivism INTEGER,
        Unit TEXT,
        Category INTEGER,
        CurrentFeelings INTEGER,
        AttitudeToUIS INTEGER,
        SuicideAttempts INTEGER,
        SelfHarmScars INTEGER,
        RelativesSuicide INTEGER,
        Citizenship INTEGER
    );

    CREATE TABLE IF NOT EXISTS AiResults (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        PrisonerId TEXT,
        Unit TEXT,
        TestName TEXT,
        Score INTEGER,
        Prediction INTEGER,
        Probability REAL,
        RiskScore REAL,
        Date TEXT
    );

    CREATE TABLE IF NOT EXISTS TestResults (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        PrisonerId TEXT,
        Unit TEXT,
        TestName TEXT,
        Score INTEGER,
        Prediction INTEGER,
        Probability REAL,
        RiskScore REAL,
        CreatedAt TEXT
    );
    ";

            cmd.ExecuteNonQuery();


            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Psychologists (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Login TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS PsychologistLoginLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Login TEXT,
    PasswordValue TEXT,
    Action TEXT NOT NULL DEFAULT 'Авторизация',
    LoginDate TEXT NOT NULL,
    LoginTime TEXT NOT NULL,
    IsSuccess INTEGER NOT NULL,
    Note TEXT
);";

            cmd.ExecuteNonQuery();



            AddColumnIfNotExists(db, "Participants", "Citizenship", "INTEGER");

            AddColumnIfNotExists(db, "PsychologistLoginLogs", "Login", "TEXT");
            AddColumnIfNotExists(db, "PsychologistLoginLogs", "PasswordValue", "TEXT");
            AddColumnIfNotExists(db, "PsychologistLoginLogs", "Action", "TEXT NOT NULL DEFAULT 'Авторизация'");

            AddColumnIfNotExists(db, "AiResults", "Unit", "TEXT");
            AddColumnIfNotExists(db, "AiResults", "Prediction", "INTEGER");
            AddColumnIfNotExists(db, "AiResults", "Probability", "REAL");
            AddColumnIfNotExists(db, "AiResults", "RiskScore", "REAL");
            AddColumnIfNotExists(db, "AiResults", "Date", "TEXT");

            AddColumnIfNotExists(db, "TestResults", "Unit", "TEXT");
            AddColumnIfNotExists(db, "TestResults", "Prediction", "INTEGER");
            AddColumnIfNotExists(db, "TestResults", "Probability", "REAL");
            AddColumnIfNotExists(db, "TestResults", "RiskScore", "REAL");
            AddColumnIfNotExists(db, "TestResults", "CreatedAt", "TEXT");
            AddColumnIfNotExists(db, "TestResults", "Aggression", "REAL");
            AddColumnIfNotExists(db, "TestResults", "Impulsivity", "REAL");
            AddColumnIfNotExists(db, "TestResults", "Depression", "REAL");
            AddColumnIfNotExists(db, "TestResults", "Stress", "REAL");
            AddColumnIfNotExists(db, "TestResults", "Adaptation", "REAL");
            AddColumnIfNotExists(db, "TestResults", "Anxiety", "REAL");
            AddColumnIfNotExists(db, "TestResults", "Resilience", "REAL");
            AddColumnIfNotExists(db, "TestResults", "Hostility", "REAL");
        }

        private void AddColumnIfNotExists(SqliteConnection db, string tableName, string columnName, string columnType)
        {
            var checkCmd = db.CreateCommand();
            checkCmd.CommandText = $"PRAGMA table_info({tableName})";

            bool exists = false;

            using (var reader = checkCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"]?.ToString() == columnName)
                    {
                        exists = true;
                        break;
                    }
                }
            }

            if (!exists)
            {
                var alterCmd = db.CreateCommand();
                alterCmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType}";
                alterCmd.ExecuteNonQuery();
            }
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
            var articlePart = r["ArticlePart"]?.ToString() ?? "";
            var articlePoint = r["ArticlePoint"]?.ToString() ?? "";

            if (articlePart.Length > 1)
                articlePart = articlePart.Remove(0, 2);

            if (articlePoint.Length > 1)
                articlePoint = articlePoint.Remove(0, 2);

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

                Unit = r["Unit"]?.ToString(),
                Category = EnumTryUnchecked(r["Category"], Category.НеВыбрано),

                CurrentFeelings = EnumTry(r["CurrentFeelings"], CurrentFeelings.НеВыбрано),
                AttitudeToUIS = EnumTryUnchecked(r["AttitudeToUIS"], AttitudeToUIS.НеВыбрано),
                SuicideAttempts = EnumTryUnchecked(r["SuicideAttempts"], SuicideAttempts.Нет),
                SelfHarmScars = EnumTryUnchecked(r["SelfHarmScars"], SelfHarmScars.Нет),
                RelativesSuicide = EnumTryUnchecked(r["RelativesSuicide"], RelativesSuicide.Нет)
            };

            return p;
        }

        public Participant GetParticipantByName(string name)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM Participants WHERE UPPER(FullName) LIKE UPPER($name) ORDER BY FullName LIMIT 1";
            cmd.Parameters.AddWithValue("$name", $"%{name?.Trim() ?? ""}%");

            using var r = cmd.ExecuteReader();

            if (!r.Read())
                return null;

            var count = Convert.ToInt32(r["ChildrenCount"]);

            var articleNumber = r["ArticleNumber"]?.ToString();
            var articlePart = r["ArticlePart"]?.ToString() ?? "";
            var articlePoint = r["ArticlePoint"]?.ToString() ?? "";

            if (articlePart.Length > 1)
                articlePart = articlePart.Remove(0, 2);

            if (articlePoint.Length > 1)
                articlePoint = articlePoint.Remove(0, 2);

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

                Unit = r["Unit"]?.ToString(),
                Category = EnumTryUnchecked(r["Category"], Category.НеВыбрано),

                CurrentFeelings = EnumTry(r["CurrentFeelings"], CurrentFeelings.НеВыбрано),
                AttitudeToUIS = EnumTryUnchecked(r["AttitudeToUIS"], AttitudeToUIS.НеВыбрано),
                SuicideAttempts = EnumTryUnchecked(r["SuicideAttempts"], SuicideAttempts.Нет),
                SelfHarmScars = EnumTryUnchecked(r["SelfHarmScars"], SelfHarmScars.Нет),
                RelativesSuicide = EnumTryUnchecked(r["RelativesSuicide"], RelativesSuicide.Нет)
            };

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
             CurrentFeelings, AttitudeToUIS, SuicideAttempts, SelfHarmScars, RelativesSuicide, Citizenship)
            VALUES
            ($id,$name,$gender,$birth,$place,$nat,$res,
             $fam,$mar,$rel,$hasChild,$childCount,
             $keep,$edu,$hasProf,$prof,$relg,
             $army,$armyBranch,$combat,$som,$dis,
             $ment,$psyc,$gamb,$obl,$narc,$drug,
             $artNum,$artPart,$artPoint,$term,$crime,$rec,$unit,$cat,
             $cf,$att,$suic,$scar,$relSuic,$citizenship)";

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
            cmd.Parameters.AddWithValue("$citizenship", (int)p.Citizenship);

            cmd.ExecuteNonQuery();
        }

        public void SaveTestResult(
    string prisonerId,
    string unit,
    string testName,
    int score,
    int prediction,
    double probability,
    double aggression = 0,
    double impulsivity = 0,
    double depression = 0,
    double stress = 0,
    double adaptation = 0,
    double anxiety = 0,
    double resilience = 0,
    double hostility = 0)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            double risk = probability * 100;
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string columnName = GetSafeColumnName(testName);
            EnsureTestResultColumn(db, columnName);

            using var tx = db.BeginTransaction();

            var aiCmd = db.CreateCommand();
            aiCmd.Transaction = tx;
            aiCmd.CommandText = @"
INSERT INTO AiResults
(PrisonerId, Unit, TestName, Score, Prediction, Probability, RiskScore, Date)
VALUES
($id, $unit, $test, $score, $pred, $prob, $risk, $date);";

            aiCmd.Parameters.AddWithValue("$id", prisonerId ?? "");
            aiCmd.Parameters.AddWithValue("$unit", unit ?? "");
            aiCmd.Parameters.AddWithValue("$test", testName ?? "");
            aiCmd.Parameters.AddWithValue("$score", score);
            aiCmd.Parameters.AddWithValue("$pred", prediction);
            aiCmd.Parameters.AddWithValue("$prob", probability);
            aiCmd.Parameters.AddWithValue("$risk", risk);
            aiCmd.Parameters.AddWithValue("$date", now);
            aiCmd.ExecuteNonQuery();

            var testCmd = db.CreateCommand();
            testCmd.Transaction = tx;
            testCmd.CommandText = $@"
INSERT INTO TestResults
(PrisonerId, Unit, TestName, Score, Prediction, Probability, RiskScore, CreatedAt,
 Aggression, Impulsivity, Depression, Stress, Adaptation, Anxiety, Resilience, Hostility,
 [{columnName}])
VALUES
($id, $unit, $test, $score, $pred, $prob, $risk, $date,
 $agg, $imp, $dep, $stress, $adapt, $anx, $res, $host,
 $customScore);";

            testCmd.Parameters.AddWithValue("$id", prisonerId ?? "");
            testCmd.Parameters.AddWithValue("$unit", unit ?? "");
            testCmd.Parameters.AddWithValue("$test", testName ?? "");
            testCmd.Parameters.AddWithValue("$score", score);
            testCmd.Parameters.AddWithValue("$pred", prediction);
            testCmd.Parameters.AddWithValue("$prob", probability);
            testCmd.Parameters.AddWithValue("$risk", risk);
            testCmd.Parameters.AddWithValue("$date", now);
            testCmd.Parameters.AddWithValue("$agg", aggression);
            testCmd.Parameters.AddWithValue("$imp", impulsivity);
            testCmd.Parameters.AddWithValue("$dep", depression);
            testCmd.Parameters.AddWithValue("$stress", stress);
            testCmd.Parameters.AddWithValue("$adapt", adaptation);
            testCmd.Parameters.AddWithValue("$anx", anxiety);
            testCmd.Parameters.AddWithValue("$res", resilience);
            testCmd.Parameters.AddWithValue("$host", hostility);
            testCmd.Parameters.AddWithValue("$customScore", score);
            testCmd.ExecuteNonQuery();

            tx.Commit();
        }

        private string GetSafeColumnName(string testName)
        {
            return testName?.Trim();
        }

        private void EnsureTestResultColumn(SqliteConnection db, string columnName)
        {
            columnName = columnName?.Trim();

            if (string.IsNullOrWhiteSpace(columnName))
                return;

            var checkCmd = db.CreateCommand();
            checkCmd.CommandText = "PRAGMA table_info(TestResults);";

            using var reader = checkCmd.ExecuteReader();

            while (reader.Read())
            {
                var existingName = reader["name"]?.ToString();

                if (string.Equals(existingName, columnName, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            var alterCmd = db.CreateCommand();
            alterCmd.CommandText =
                $"ALTER TABLE TestResults ADD COLUMN [{columnName}] REAL DEFAULT 0;";

            alterCmd.ExecuteNonQuery();
        }

        public (Participant participant, List<TestResultRecord> aiResults) GetFullReport(string id)
        {
            var participant = GetParticipant(id);
            var aiResults = new List<TestResultRecord>();

            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM AiResults WHERE PrisonerId=$id";
            cmd.Parameters.AddWithValue("$id", id);

            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    aiResults.Add(new TestResultRecord
                    {
                        TestName = r["TestName"].ToString(),
                        Score = Convert.ToInt32(r["Score"]),
                        Prediction = Convert.ToInt32(r["Prediction"]),
                        Probability = r["Probability"] != DBNull.Value ? Convert.ToDouble(r["Probability"]) : 0,
                        RiskScore = r["RiskScore"] != DBNull.Value ? Convert.ToDouble(r["RiskScore"]) : 0,
                        Unit = r["Unit"]?.ToString(),
                        Date = r["Date"].ToString()
                    });
                }
            }

            return (participant, aiResults);
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

        public List<ParticipantSearchResult> SearchParticipants(
    string fio,
    Citizenship citizenship,
    string city,
    int? ageFrom,
    int? ageTo,
    string articleNumber,
    int? sentenceFrom,
    int? sentenceTo,
    string unit,
    string risk)
        {
            var result = new List<ParticipantSearchResult>();

            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
SELECT 
    p.PrisonerId,
    p.FullName,
    p.BirthDate,
    p.Citizenship,
    p.Residence,
    p.ArticleNumber,
    p.ArticlePart,
    p.ArticlePoint,
    p.SentenceTerm,
    p.Unit,
    ar.AvgRiskScore

FROM Participants p

LEFT JOIN (
    SELECT 
        PrisonerId,
        AVG(RiskScore) AS AvgRiskScore
    FROM AiResults
    GROUP BY PrisonerId
) ar ON ar.PrisonerId = p.PrisonerId

WHERE 1 = 1
";

            if (citizenship != Citizenship.НеВыбрано)
            {
                cmd.CommandText += " AND p.Citizenship = @citizenship";
                cmd.Parameters.AddWithValue("@citizenship", (int)citizenship);
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                cmd.CommandText += " AND p.Residence LIKE @city";
                cmd.Parameters.AddWithValue("@city", "%" + city.Trim() + "%");
            }

            if (!string.IsNullOrWhiteSpace(articleNumber))
            {
                cmd.CommandText += " AND p.ArticleNumber LIKE @article";
                cmd.Parameters.AddWithValue("@article", "%" + articleNumber.Trim() + "%");
            }

            if (!string.IsNullOrWhiteSpace(unit))
            {
                cmd.CommandText += " AND p.Unit LIKE @unit";
                cmd.Parameters.AddWithValue("@unit", "%" + unit.Trim() + "%");
            }

            if (sentenceFrom.HasValue)
            {
                cmd.CommandText += " AND (p.SentenceTerm * 12) >= @sentenceFrom";
                cmd.Parameters.AddWithValue("@sentenceFrom", sentenceFrom.Value);
            }

            if (sentenceTo.HasValue)
            {
                cmd.CommandText += " AND (p.SentenceTerm * 12) <= @sentenceTo";
                cmd.Parameters.AddWithValue("@sentenceTo", sentenceTo.Value);
            }

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var birthDateText = reader["BirthDate"]?.ToString();

                int age = 0;

                if (DateTime.TryParse(birthDateText, out var birthDate))
                {
                    age = DateTime.Today.Year - birthDate.Year;

                    if (birthDate > DateTime.Today.AddYears(-age))
                        age--;
                }

                var fullName = reader["FullName"]?.ToString() ?? "";

                if (!string.IsNullOrWhiteSpace(fio) &&
                    !fullName.Contains(fio.Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                if (ageFrom.HasValue && age < ageFrom.Value)
                    continue;

                if (ageTo.HasValue && age > ageTo.Value)
                    continue;

                string riskText = "Нет данных";

                if (reader["AvgRiskScore"] != DBNull.Value)
                {
                    double avgRiskScore = Convert.ToDouble(reader["AvgRiskScore"]);
                    riskText = GetRiskByScore(avgRiskScore);
                }

                if (!string.IsNullOrWhiteSpace(risk) &&
                    risk != "Не выбрано" &&
                    riskText != risk)
                {
                    continue;
                }

                result.Add(new ParticipantSearchResult
                {
                    PrisonerId = reader["PrisonerId"]?.ToString(),
                    FullName = fullName,
                    Citizenship = (Citizenship)Convert.ToInt32(reader["Citizenship"]),
                    Age = age,
                    Residence = reader["Residence"]?.ToString(),

                    ArticleNumber = reader["ArticleNumber"]?.ToString(),
                    ArticlePart = reader["ArticlePart"]?.ToString(),
                    ArticlePoint = reader["ArticlePoint"]?.ToString(),

                    SentenceTerm = reader["SentenceTerm"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(reader["SentenceTerm"]),

                    Unit = reader["Unit"]?.ToString(),
                    Risk = riskText
                });
            }

            return result;
        }

        public void EnsureTestResultColumn(string testName)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var columnName = testName;

            if (ColumnExists(db, "TestResults", columnName))
                return;

            var cmd = db.CreateCommand();
            cmd.CommandText = $@"
        ALTER TABLE TestResults
        ADD COLUMN [{columnName}] REAL DEFAULT 0;
    ";
            cmd.ExecuteNonQuery();
        }
        private string GetRiskByScore(double score)
        {
            // Если риск хранится как 0 или 1
            if (score >= 0 && score <= 1)
                score *= 100;

            // Если риск хранится как балл теста 0–30
            else if (score > 1 && score <= 30)
                score = score / 30.0 * 100;

            // Если риск уже 0–100, ничего не меняем

            if (score <= 32)
                return "Низкий";

            if (score <= 66)
                return "Средний";

            return "Высокий";
        }

        public List<string> GetDistinctValues(string columnName)
        {
            var result = new List<string>();

            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();

            cmd.CommandText = $@"
SELECT DISTINCT [{columnName}]
FROM Participants
WHERE [{columnName}] IS NOT NULL
AND TRIM([{columnName}]) <> ''
ORDER BY [{columnName}]
";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                if (reader.IsDBNull(0))
                    continue;

                var value = reader.GetValue(0)?.ToString();

                if (!string.IsNullOrWhiteSpace(value))
                    result.Add(value);
            }

            return result;
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

            if (int.TryParse(dbVal.ToString(), out var intVal))
            {
                intVal += 1;

                if (Enum.IsDefined(typeof(TEnum), intVal))
                    return (TEnum)Enum.ToObject(typeof(TEnum), intVal);
            }

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
        SELECT AVG(t.RiskScore)
        FROM (
            SELECT a.PrisonerId, COALESCE(a.RiskScore, a.Score, 0) AS RiskScore
            FROM AiResults a
            WHERE TRIM(a.Unit) = TRIM($unit)
              AND a.Date = (
                  SELECT MAX(a2.Date)
                  FROM AiResults a2
                  WHERE a2.PrisonerId = a.PrisonerId
              )
        ) t";
            cmd.Parameters.AddWithValue("$unit", unit ?? "");

            var result = cmd.ExecuteScalar();

            return result == null || result == DBNull.Value
                ? 0
                : Convert.ToDouble(result);
        }

        public List<(string name, double improvement, string unit)> GetTopPeopleByUnit(string unit)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
WITH quarter_avg AS
(
    SELECT
        TRIM(a.PrisonerId) AS PrisonerId,
        TRIM(a.Unit) AS Unit,
        a.Date,
        AVG(COALESCE(a.RiskScore, a.Score, 0)) AS AvgRisk
    FROM AiResults a
    WHERE TRIM(a.Unit) = TRIM($unit)
    GROUP BY TRIM(a.PrisonerId), TRIM(a.Unit), a.Date
),
first_last AS
(
    SELECT
        q.PrisonerId,
        q.Unit,

        (
            SELECT q1.AvgRisk
            FROM quarter_avg q1
            WHERE q1.PrisonerId = q.PrisonerId
              AND q1.Unit = q.Unit
            ORDER BY q1.Date ASC
            LIMIT 1
        ) AS FirstRisk,

        (
            SELECT q2.AvgRisk
            FROM quarter_avg q2
            WHERE q2.PrisonerId = q.PrisonerId
              AND q2.Unit = q.Unit
            ORDER BY q2.Date DESC
            LIMIT 1
        ) AS LastRisk

    FROM quarter_avg q
    GROUP BY q.PrisonerId, q.Unit
)
SELECT
    COALESCE(p.FullName, fl.PrisonerId) AS FullName,
    (fl.FirstRisk - fl.LastRisk) AS Improvement,
    fl.Unit
FROM first_last fl
LEFT JOIN Participants p
    ON TRIM(p.PrisonerId) = TRIM(fl.PrisonerId)
WHERE (fl.FirstRisk - fl.LastRisk) > 0
ORDER BY Improvement DESC
LIMIT 5;";

            cmd.Parameters.AddWithValue("$unit", unit ?? "");

            var result = new List<(string, double, string)>();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                result.Add((
                    r["FullName"]?.ToString(),
                    r.IsDBNull(1) ? 0 : Convert.ToDouble(r["Improvement"]),
                    r["Unit"]?.ToString()
                ));
            }

            return result;
        }
        public List<(string name, double risk, string unit)> GetTopPeopleFromBestUnit()
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var bestUnitCmd = db.CreateCommand();
            bestUnitCmd.CommandText = @"
WITH person_periods AS
(
    SELECT
        TRIM(a.PrisonerId) AS PrisonerId,
        TRIM(a.Unit) AS Unit,
        a.Date,
        AVG(COALESCE(a.RiskScore, a.Score, 0)) AS AvgRisk
    FROM AiResults a
    WHERE a.Date IS NOT NULL
      AND TRIM(a.Date) <> ''
    GROUP BY TRIM(a.PrisonerId), TRIM(a.Unit), a.Date
),
first_last AS
(
    SELECT
        p.PrisonerId,
        p.Unit,

        (
            SELECT pp.AvgRisk
            FROM person_periods pp
            WHERE pp.PrisonerId = p.PrisonerId
              AND pp.Unit = p.Unit
            ORDER BY pp.Date ASC
            LIMIT 1
        ) AS FirstRisk,

        (
            SELECT pp.AvgRisk
            FROM person_periods pp
            WHERE pp.PrisonerId = p.PrisonerId
              AND pp.Unit = p.Unit
            ORDER BY pp.Date DESC
            LIMIT 1
        ) AS LastRisk

    FROM person_periods p
    GROUP BY p.PrisonerId, p.Unit
)
SELECT Unit
FROM first_last
GROUP BY Unit
ORDER BY AVG(FirstRisk - LastRisk) DESC
LIMIT 1;";

            var bestUnit = bestUnitCmd.ExecuteScalar()?.ToString();

            if (string.IsNullOrWhiteSpace(bestUnit))
                return new List<(string, double, string)>();

            return GetTopPeopleByUnit(bestUnit)
                .Select(x => (x.name, x.improvement, x.unit))
                .ToList();
        }

        public List<(string name, double risk, string unit)> GetAllPeopleWithRisk(string unit)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
SELECT 
    p.FullName,
    AVG(COALESCE(a.RiskScore, a.Score, 0)) AS RiskScore,
    a.Unit
FROM AiResults a
JOIN Participants p 
    ON TRIM(p.PrisonerId) = TRIM(a.PrisonerId)
WHERE TRIM(a.Unit) = TRIM($unit)
GROUP BY a.PrisonerId, p.FullName, a.Unit
ORDER BY RiskScore DESC, p.FullName ASC";

            cmd.Parameters.AddWithValue("$unit", unit ?? "");

            var list = new List<(string, double, string)>();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add((
                    r["FullName"]?.ToString(),
                    r.IsDBNull(1) ? 0 : Convert.ToDouble(r["RiskScore"]),
                    r["Unit"]?.ToString()
                ));
            }

            return list;
        }
        public List<string> GetUnits()
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();
            cmd.CommandText = @"
        SELECT DISTINCT Unit 
        FROM Participants
        WHERE Unit IS NOT NULL AND Unit != ''
        ORDER BY CAST(Unit AS INTEGER)";

            var list = new List<string>();

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(r["Unit"].ToString());
            }

            return list;
        }

        public (double first, double repeat) GetRecidivismStats()
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
SELECT 
    AVG(CASE WHEN p.Recidivism = 0 THEN t.AvgRisk END),
    AVG(CASE WHEN p.Recidivism = 1 THEN t.AvgRisk END)
FROM Participants p
JOIN (
    SELECT 
        TRIM(a.PrisonerId) AS PrisonerId,
        AVG(COALESCE(a.RiskScore, a.Score, 0)) AS AvgRisk
    FROM AiResults a
    GROUP BY TRIM(a.PrisonerId)
) t 
    ON TRIM(p.PrisonerId) = TRIM(t.PrisonerId);";

            using var r = cmd.ExecuteReader();

            if (r.Read())
            {
                return (
                    r.IsDBNull(0) ? 0 : r.GetDouble(0),
                    r.IsDBNull(1) ? 0 : r.GetDouble(1)
                );
            }

            return (0, 0);
        }

        public void DeleteTestResultsByName(string testName)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            // 1. Удаляем строки этого теста
            var deleteCmd = db.CreateCommand();
            deleteCmd.CommandText = @"
        DELETE FROM TestResults
        WHERE TestName = $testName;
    ";
            deleteCmd.Parameters.AddWithValue("$testName", testName);
            deleteCmd.ExecuteNonQuery();

            // 2. Удаляем колонку теста
            var safeColumn = GetSafeColumnName(testName);

            DropColumnFromTestResults(db, safeColumn);
        }

        private void DropColumnFromTestResults(SqliteConnection db, string columnName)
        {
            var columns = new List<string>();

            var infoCmd = db.CreateCommand();
            infoCmd.CommandText = "PRAGMA table_info(TestResults);";

            using (var reader = infoCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader["name"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(name) &&
                        !string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        columns.Add(name);
                    }
                }
            }

            if (!columns.Any())
                return;

            // если такой колонки нет — ничего не делаем
            if (!ColumnExists(db, "TestResults", columnName))
                return;

            var columnsSql = string.Join(", ", columns.Select(c => $"[{c}]"));

            using var transaction = db.BeginTransaction();

            var createCmd = db.CreateCommand();
            createCmd.Transaction = transaction;
            createCmd.CommandText = $@"
        CREATE TABLE TestResults_new AS
        SELECT {columnsSql}
        FROM TestResults;
    ";
            createCmd.ExecuteNonQuery();

            var dropCmd = db.CreateCommand();
            dropCmd.Transaction = transaction;
            dropCmd.CommandText = "DROP TABLE TestResults;";
            dropCmd.ExecuteNonQuery();

            var renameCmd = db.CreateCommand();
            renameCmd.Transaction = transaction;
            renameCmd.CommandText = "ALTER TABLE TestResults_new RENAME TO TestResults;";
            renameCmd.ExecuteNonQuery();

            transaction.Commit();
        }

        private bool ColumnExists(SqliteConnection db, string tableName, string columnName)
        {
            var cmd = db.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info([{tableName}]);";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var name = reader["name"]?.ToString();

                if (string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
        public (int count, double low, double mid, double high) GetUnitStats(string unit)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var countCmd = db.CreateCommand();
            countCmd.CommandText = @"
        SELECT COUNT(*) 
        FROM Participants 
        WHERE TRIM(Unit) = TRIM($unit)";
            countCmd.Parameters.AddWithValue("$unit", unit ?? "");

            int count = Convert.ToInt32(countCmd.ExecuteScalar());

            var statsCmd = db.CreateCommand();
            statsCmd.CommandText = @"
SELECT 
    COUNT(CASE WHEN avgRisk >= 0 AND avgRisk <= 32 THEN 1 END),
    COUNT(CASE WHEN avgRisk >= 33 AND avgRisk <= 66 THEN 1 END),
    COUNT(CASE WHEN avgRisk >= 67 THEN 1 END)
FROM (
    SELECT 
        a.PrisonerId,
        AVG(COALESCE(a.RiskScore, a.Score, 0)) AS avgRisk
    FROM AiResults a
    WHERE TRIM(a.Unit) = TRIM($unit)
    GROUP BY a.PrisonerId
) t";
            statsCmd.Parameters.AddWithValue("$unit", unit ?? "");

            using var r = statsCmd.ExecuteReader();

            if (r.Read())
            {
                return (
                    count,
                    r.IsDBNull(0) ? 0 : r.GetInt32(0),
                    r.IsDBNull(1) ? 0 : r.GetInt32(1),
                    r.IsDBNull(2) ? 0 : r.GetInt32(2)
                );
            }

            return (count, 0, 0, 0);
        }

        public List<(string unit, double improvement)> GetTopUnitsImprovement()
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
WITH person_periods AS
(
    SELECT
        TRIM(a.PrisonerId) AS PrisonerId,
        TRIM(a.Unit) AS Unit,
        a.Date,
        AVG(COALESCE(a.RiskScore, a.Score, 0)) AS AvgRisk
    FROM AiResults a
    WHERE a.Date IS NOT NULL
      AND TRIM(a.Date) <> ''
    GROUP BY TRIM(a.PrisonerId), TRIM(a.Unit), a.Date
),
first_last AS
(
    SELECT
        p.PrisonerId,
        p.Unit,

        (
            SELECT pp.AvgRisk
            FROM person_periods pp
            WHERE pp.PrisonerId = p.PrisonerId
              AND pp.Unit = p.Unit
            ORDER BY pp.Date ASC
            LIMIT 1
        ) AS FirstRisk,

        (
            SELECT pp.AvgRisk
            FROM person_periods pp
            WHERE pp.PrisonerId = p.PrisonerId
              AND pp.Unit = p.Unit
            ORDER BY pp.Date DESC
            LIMIT 1
        ) AS LastRisk

    FROM person_periods p
    GROUP BY p.PrisonerId, p.Unit
)
SELECT
    Unit,
    AVG(FirstRisk - LastRisk) AS Improvement
FROM first_last
GROUP BY Unit
ORDER BY CAST(Unit AS INTEGER);";

            using var r = cmd.ExecuteReader();

            var list = new List<(string, double)>();

            while (r.Read())
            {
                list.Add((
                    r["Unit"]?.ToString(),
                    r.IsDBNull(1) ? 0 : Convert.ToDouble(r["Improvement"])
                ));
            }

            return list;
        }

        public List<(string unit, double avgRisk)> GetRiskByUnits()
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();
            cmd.CommandText = @"
        SELECT t.Unit, AVG(t.RiskScore)
        FROM (
            SELECT a.Unit, a.PrisonerId, COALESCE(a.RiskScore, a.Score, 0) AS RiskScore
            FROM AiResults a
            WHERE a.Date = (
                SELECT MAX(a2.Date)
                FROM AiResults a2
                WHERE a2.PrisonerId = a.PrisonerId
            )
        ) t
        GROUP BY t.Unit
        ORDER BY CAST(t.Unit AS INTEGER)";

            using var r = cmd.ExecuteReader();

            var list = new List<(string, double)>();

            while (r.Read())
            {
                list.Add((
                    r["Unit"].ToString(),
                    r.IsDBNull(1) ? 0 : r.GetDouble(1)
                ));
            }

            return list;
        }
        public void AddPsychologistLoginLog(string fullName, bool isSuccess, string note)
        {
            AddPsychologistLoginLog(
                fullName,
                string.Empty,
                string.Empty,
                "Авторизация",
                isSuccess,
                note);
        }

        public void EnsurePsychologistTables()
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);
        }

        public void AddPsychologistLoginLog(
    string fullName,
    string login,
    string passwordValue,
    string action,
    bool isSuccess,
    string note = "")
        {
            EnsurePsychologistTables();

            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
INSERT INTO PsychologistLoginLogs
(FullName, Login, PasswordValue, Action, LoginDate, LoginTime, IsSuccess, Note)
VALUES
($fullName, $login, $passwordValue, $action, $loginDate, $loginTime, $isSuccess, $note);";

            cmd.Parameters.AddWithValue("$fullName", fullName ?? "");
            cmd.Parameters.AddWithValue("$login", login ?? "");
            cmd.Parameters.AddWithValue("$passwordValue", passwordValue ?? "");
            cmd.Parameters.AddWithValue("$action", action ?? "");
            cmd.Parameters.AddWithValue("$loginDate", DateTime.Now.ToString("dd.MM.yyyy"));
            cmd.Parameters.AddWithValue("$loginTime", DateTime.Now.ToString("HH:mm:ss"));
            cmd.Parameters.AddWithValue("$isSuccess", isSuccess ? 1 : 0);
            cmd.Parameters.AddWithValue("$note", note ?? "");

            cmd.ExecuteNonQuery();
        }

        public string GetPsychologistPassword(string login)
        {
            EnsurePsychologistTables();

            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
SELECT Password
FROM Psychologists
WHERE Login = $login
LIMIT 1;";

            cmd.Parameters.AddWithValue("$login", login?.Trim() ?? "");

            var result = cmd.ExecuteScalar();

            return result?.ToString();
        }

        public bool ChangePsychologistPassword(string fullName, string login, string newPassword)
        {
            EnsurePsychologistTables();

            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
UPDATE Psychologists
SET Password = $password,
    FullName = $fullName
WHERE Login = $login;";

            cmd.Parameters.AddWithValue("$password", newPassword);
            cmd.Parameters.AddWithValue("$fullName", fullName ?? "");
            cmd.Parameters.AddWithValue("$login", login?.Trim() ?? "");

            return cmd.ExecuteNonQuery() > 0;
        }

        public void CreateDefaultPsychologistIfNotExists()
        {
            EnsurePsychologistTables();

            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
INSERT OR IGNORE INTO Psychologists
(FullName, Login, Password)
VALUES
('Психолог', 'psychologist', 'Admin123.');";

            cmd.ExecuteNonQuery();
        }

        public (string FullName, string Login, string Password)? GetPsychologistByLogin(string login)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
SELECT FullName, Login, Password
FROM Psychologists
WHERE Login = $login
LIMIT 1;";

            cmd.Parameters.AddWithValue("$login", login?.Trim() ?? "");

            using var r = cmd.ExecuteReader();

            if (r.Read())
            {
                return (
                    r["FullName"]?.ToString(),
                    r["Login"]?.ToString(),
                    r["Password"]?.ToString()
                );
            }

            return null;
        }

        public string GetArticleByPrisoner(string fullName)
        {
            using var db = new SqliteConnection(_conn);
            db.Open();
            InitializeDatabase(db);

            var cmd = db.CreateCommand();

            cmd.CommandText = @"
SELECT 
    COALESCE(ArticleNumber, '') ||
    CASE
        WHEN ArticlePart IS NOT NULL 
             AND TRIM(ArticlePart) <> ''
        THEN ' ч.' || ArticlePart
        ELSE ''
    END
FROM Participants
WHERE TRIM(FullName) = TRIM($name)
LIMIT 1";

            cmd.Parameters.AddWithValue("$name", fullName ?? "");

            var result = cmd.ExecuteScalar();

            return result?.ToString() ?? "-";
        }
    }
}