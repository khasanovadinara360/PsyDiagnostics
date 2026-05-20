using Newtonsoft.Json;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PsyDiagnostics.ViewModels
{
    public class AddTestViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly DatabaseService _db = new DatabaseService();
        private readonly List<Question> _questions = new();

        private string _testName;
        public string TestName
        {
            get => _testName;
            set { _testName = value; OnPropertyChanged(); }
        }

        public string LowMaxText { get; set; } = "32";
        public string MediumMaxText { get; set; } = "66";

        private string _questionsCountText = "";
        public string QuestionsCountText
        {
            get => _questionsCountText;
            set { _questionsCountText = value; OnPropertyChanged(); }
        }

        private string _questionText;
        public string QuestionText
        {
            get => _questionText;
            set { _questionText = value; OnPropertyChanged(); }
        }

        private string _answer1Text = "Нет";
        public string Answer1Text
        {
            get => _answer1Text;
            set { _answer1Text = value; OnPropertyChanged(); }
        }

        private string _answer1ValueText = "0";
        public string Answer1ValueText
        {
            get => _answer1ValueText;
            set { _answer1ValueText = value; OnPropertyChanged(); }
        }

        private string _answer2Text = "Иногда";
        public string Answer2Text
        {
            get => _answer2Text;
            set { _answer2Text = value; OnPropertyChanged(); }
        }

        private string _answer2ValueText = "1";
        public string Answer2ValueText
        {
            get => _answer2ValueText;
            set { _answer2ValueText = value; OnPropertyChanged(); }
        }

        private string _answer3Text = "Да";
        public string Answer3Text
        {
            get => _answer3Text;
            set { _answer3Text = value; OnPropertyChanged(); }
        }

        private string _answer3ValueText = "2";
        public string Answer3ValueText
        {
            get => _answer3ValueText;
            set { _answer3ValueText = value; OnPropertyChanged(); }
        }

        private string _selectedExistingTest;
        public string SelectedExistingTest
        {
            get => _selectedExistingTest;
            set
            {
                _selectedExistingTest = value;
                OnPropertyChanged();

                if (!string.IsNullOrWhiteSpace(value))
                    LoadTestIntoEditor(value);
            }
        }

        private string _selectedAddedQuestion;
        public string SelectedAddedQuestion
        {
            get => _selectedAddedQuestion;
            set
            {
                _selectedAddedQuestion = value;
                OnPropertyChanged();

                if (string.IsNullOrWhiteSpace(value))
                    return;

                int index = QuestionsPreview.IndexOf(value);

                if (index < 0 || index >= _questions.Count)
                    return;

                FillQuestionFields(_questions[index]);
                IsEditingQuestion = true;
            }
        }

        private bool _isEditingQuestion;
        public bool IsEditingQuestion
        {
            get => _isEditingQuestion;
            set
            {
                _isEditingQuestion = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ExistingTests { get; set; } = new();
        public ObservableCollection<string> QuestionsPreview { get; set; } = new();

        public ICommand AddQuestionCommand { get; }
        public ICommand SaveTestCommand { get; }
        public ICommand DeleteTestCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SaveQuestionChangesCommand { get; }
        public ICommand DeleteQuestionCommand { get; }
        public ICommand EditQuestionCommand { get; }

        public AddTestViewModel(MainViewModel main)
        {
            _main = main;

            AddQuestionCommand = new RelayCommand(_ => AddQuestion());
            SaveTestCommand = new RelayCommand(_ => SaveTest());
            DeleteTestCommand = new RelayCommand(_ => DeleteTest());
            BackCommand = new RelayCommand(_ => _main.ShowParticipantPage());
            SaveQuestionChangesCommand = new RelayCommand(_ => SaveQuestionChanges());
            DeleteQuestionCommand = new RelayCommand(_ => DeleteQuestion());
            EditQuestionCommand = new RelayCommand(_ => EditQuestion());

            LoadExistingTests();
        }

        private string GetDataFolder()
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        private string GetTestsFilePath()
        {
            return Path.Combine(GetDataFolder(), "tests.json");
        }

        private List<Test> LoadAllTests()
        {
            try
            {
                var path = GetTestsFilePath();

                if (!File.Exists(path))
                    return new List<Test>();

                var json = File.ReadAllText(path);

                var tests = JsonConvert.DeserializeObject<List<Test>>(json)
                            ?? new List<Test>();

                return tests
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .GroupBy(x => x.Name.Trim())
                    .Select(g => g.First())
                    .OrderBy(x => x.Name)
                    .ToList();
            }
            catch
            {
                return new List<Test>();
            }
        }

        private void SaveAllTests(List<Test> tests)
        {
            var path = GetTestsFilePath();

            var json = JsonConvert.SerializeObject(
                tests,
                Formatting.Indented);

            File.WriteAllText(path, json);
        }

        private void LoadExistingTests()
        {
            ExistingTests.Clear();

            foreach (var test in LoadAllTests())
            {
                ExistingTests.Add(test.Name);
            }
        }

        private void LoadTestIntoEditor(string testName)
        {
            var test = LoadAllTests()
                .FirstOrDefault(x => x.Name == testName);

            if (test == null)
                return;

            TestName = test.Name;
            QuestionsCountText = test.Questions?.Count.ToString() ?? "0";

            _questions.Clear();
            QuestionsPreview.Clear();

            if (test.Questions != null)
            {
                foreach (var question in test.Questions)
                {
                    _questions.Add(question);
                    QuestionsPreview.Add(BuildQuestionPreview(QuestionsPreview.Count + 1, question));
                }
            }

            FillQuestionFields(test.Questions?.FirstOrDefault());
        }

        private void AddQuestion()
        {
            if (!int.TryParse(QuestionsCountText, out int maxQuestions) || maxQuestions <= 0)
            {
                MessageBox.Show("Введите корректное количество вопросов");
                return;
            }

            if (_questions.Count >= maxQuestions)
            {
                MessageBox.Show($"Нельзя добавить больше {maxQuestions} вопросов.");
                return;
            }

            if (string.IsNullOrWhiteSpace(QuestionText))
            {
                MessageBox.Show("Введите текст вопроса");
                return;
            }

            var answers = new List<Answer>();

            AddAnswerIfValid(answers, Answer1Text, Answer1ValueText);
            AddAnswerIfValid(answers, Answer2Text, Answer2ValueText);
            AddAnswerIfValid(answers, Answer3Text, Answer3ValueText);

            if (answers.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один ответ");
                return;
            }

            var question = new Question
            {
                Text = QuestionText.Trim(),
                Answers = answers
            };

            _questions.Add(question);
            QuestionsPreview.Add(BuildQuestionPreview(_questions.Count, question));

            ClearQuestionFields();
        }

        private void EditQuestion()
        {
            if (string.IsNullOrWhiteSpace(SelectedAddedQuestion))
            {
                MessageBox.Show("Выберите вопрос для изменения");
                return;
            }

            int index = QuestionsPreview.IndexOf(SelectedAddedQuestion);

            if (index < 0 || index >= _questions.Count)
                return;

            FillQuestionFields(_questions[index]);
            IsEditingQuestion = true;
        }

        private void SaveQuestionChanges()
        {
            if (string.IsNullOrWhiteSpace(SelectedAddedQuestion))
            {
                MessageBox.Show("Выберите вопрос для изменения");
                return;
            }

            int index = QuestionsPreview.IndexOf(SelectedAddedQuestion);

            if (index < 0 || index >= _questions.Count)
                return;

            if (string.IsNullOrWhiteSpace(QuestionText))
            {
                MessageBox.Show("Введите текст вопроса");
                return;
            }

            var answers = new List<Answer>();

            AddAnswerIfValid(answers, Answer1Text, Answer1ValueText);
            AddAnswerIfValid(answers, Answer2Text, Answer2ValueText);
            AddAnswerIfValid(answers, Answer3Text, Answer3ValueText);

            if (answers.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один ответ");
                return;
            }

            _questions[index].Text = QuestionText.Trim();
            _questions[index].Answers = answers;

            QuestionsPreview[index] = BuildQuestionPreview(index + 1, _questions[index]);

            ClearQuestionFields();

            MessageBox.Show("Вопрос изменён");
        }

        private void DeleteQuestion()
        {
            if (string.IsNullOrWhiteSpace(SelectedAddedQuestion))
            {
                MessageBox.Show("Выберите вопрос для удаления");
                return;
            }

            int index = QuestionsPreview.IndexOf(SelectedAddedQuestion);

            if (index < 0 || index >= _questions.Count)
                return;

            _questions.RemoveAt(index);
            QuestionsPreview.RemoveAt(index);

            RefreshQuestionsPreview();
            ClearQuestionFields();

            MessageBox.Show("Вопрос удалён");
        }

        private void SaveTest()
        {
            if (string.IsNullOrWhiteSpace(TestName))
            {
                MessageBox.Show("Введите название теста");
                return;
            }

            if (!int.TryParse(QuestionsCountText, out int requiredCount) || requiredCount <= 0)
            {
                MessageBox.Show("Введите корректное количество вопросов");
                return;
            }

            if (_questions.Count != requiredCount)
            {
                string word = GetQuestionWord(requiredCount);

                MessageBox.Show(
                    $"Нужно добавить {requiredCount} {word}. Сейчас добавлено: {_questions.Count}.");
                return;
            }

            var tests = LoadAllTests();

            var newTest = new Test
            {
                Name = TestName.Trim(),
                LowMax = 32,
                MediumMax = 66,
                Questions = _questions.ToList()
            };

            var existing = tests.FirstOrDefault(t => t.Name == newTest.Name);

            if (existing != null)
            {
                existing.LowMax = newTest.LowMax;
                existing.MediumMax = newTest.MediumMax;
                existing.Questions = newTest.Questions;
            }
            else
            {
                tests.Add(newTest);
            }

            SaveAllTests(tests);
            LoadExistingTests();

            MessageBox.Show("Тест успешно сохранён");

            _main.ShowParticipantPage();
        }

        private void DeleteTest()
        {
            if (string.IsNullOrWhiteSpace(SelectedExistingTest))
            {
                MessageBox.Show("Выберите тест для удаления");
                return;
            }

            var result = MessageBox.Show(
                $"Удалить тест \"{SelectedExistingTest}\"?\n\nБудут удалены сам тест и связанные результаты из базы данных.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var tests = LoadAllTests();

            var removed = tests.RemoveAll(t =>
                string.Equals(t.Name, SelectedExistingTest, StringComparison.OrdinalIgnoreCase));

            if (removed == 0)
            {
                MessageBox.Show("Тест не найден в Data/tests.json");
                return;
            }

            SaveAllTests(tests);

            try
            {
                _db.DeleteTestResultsByName(SelectedExistingTest);
            }
            catch
            {
                MessageBox.Show(
                    "Тест удалён из JSON, но результаты из базы данных удалить не удалось. Проверьте метод DeleteTestResultsByName в DatabaseService.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            ClearTestEditor();
            LoadExistingTests();

            MessageBox.Show("Тест удалён");
        }

        private void ClearTestEditor()
        {
            _selectedExistingTest = null;
            OnPropertyChanged(nameof(SelectedExistingTest));

            TestName = "";
            QuestionsCountText = "";
            ClearQuestionFields();

            _questions.Clear();
            QuestionsPreview.Clear();
        }

        private void ClearQuestionFields()
        {
            SelectedAddedQuestion = null;
            IsEditingQuestion = false;

            QuestionText = "";
            Answer1Text = "Нет";
            Answer1ValueText = "0";
            Answer2Text = "Иногда";
            Answer2ValueText = "1";
            Answer3Text = "Да";
            Answer3ValueText = "2";
        }

        private void FillQuestionFields(Question question)
        {
            if (question == null)
            {
                QuestionText = "";
                Answer1Text = "Нет";
                Answer1ValueText = "0";
                Answer2Text = "Иногда";
                Answer2ValueText = "1";
                Answer3Text = "Да";
                Answer3ValueText = "2";
                return;
            }

            QuestionText = question.Text;

            Answer1Text = question.Answers != null && question.Answers.Count > 0
                ? question.Answers[0].Text
                : "";

            Answer1ValueText = question.Answers != null && question.Answers.Count > 0
                ? question.Answers[0].Value.ToString()
                : "";

            Answer2Text = question.Answers != null && question.Answers.Count > 1
                ? question.Answers[1].Text
                : "";

            Answer2ValueText = question.Answers != null && question.Answers.Count > 1
                ? question.Answers[1].Value.ToString()
                : "";

            Answer3Text = question.Answers != null && question.Answers.Count > 2
                ? question.Answers[2].Text
                : "";

            Answer3ValueText = question.Answers != null && question.Answers.Count > 2
                ? question.Answers[2].Value.ToString()
                : "";
        }

        private void AddAnswerIfValid(List<Answer> answers, string text, string valueText)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (!int.TryParse(valueText, out int value))
                value = 0;

            answers.Add(new Answer
            {
                Text = text.Trim(),
                Value = value
            });
        }

        private void RefreshQuestionsPreview()
        {
            QuestionsPreview.Clear();

            for (int i = 0; i < _questions.Count; i++)
            {
                QuestionsPreview.Add(BuildQuestionPreview(i + 1, _questions[i]));
            }
        }

        private string BuildQuestionPreview(int number, Question question)
        {
            var answers = question.Answers == null
                ? ""
                : string.Join("; ", question.Answers.Select(a => $"{a.Text} ({a.Value})"));

            return $"{number}. {question.Text}\nОтветы: {answers}";
        }

        private string GetQuestionWord(int count)
        {
            if (count % 10 == 1 && count % 100 != 11)
                return "вопрос";

            if (count % 10 >= 2 && count % 10 <= 4 &&
                !(count % 100 >= 12 && count % 100 <= 14))
                return "вопроса";

            return "вопросов";
        }
    }
}