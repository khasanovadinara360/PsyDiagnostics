using Newtonsoft.Json;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace PsyDiagnostics.ViewModels
{
    public class AddTestViewModel : BaseViewModel
    {
        private readonly string _testsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tests.json");
        private readonly MainViewModel _main;
        private readonly DatabaseService _db = new();
        private readonly List<Question> _questions = new();

        private string _testName;
        public string TestName
        {
            get => _testName;
            set { _testName = value; OnPropertyChanged(); }
        }

        private string _lowMaxText = "32";
        public string LowMaxText
        {
            get => _lowMaxText;
            set { _lowMaxText = value; OnPropertyChanged(); }
        }

        private string _mediumMaxText = "66";
        public string MediumMaxText
        {
            get => _mediumMaxText;
            set { _mediumMaxText = value; OnPropertyChanged(); }
        }

        private string _questionsCountText = string.Empty;
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
            set { _isEditingQuestion = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ExistingTests { get; } = new();
        public ObservableCollection<string> QuestionsPreview { get; } = new();

        public ICommand AddQuestionCommand { get; }
        public ICommand SaveQuestionChangesCommand { get; }
        public ICommand DeleteQuestionCommand { get; }
        public ICommand EditQuestionCommand { get; }
        public ICommand SaveTestCommand { get; }
        public ICommand DeleteTestCommand { get; }
        public ICommand BackCommand { get; }

        public AddTestViewModel(MainViewModel main)
        {
            _main = main;

            AddQuestionCommand = new RelayCommand(_ => AddQuestion());
            SaveQuestionChangesCommand = new RelayCommand(_ => SaveQuestionChanges());
            DeleteQuestionCommand = new RelayCommand(_ => DeleteQuestion());
            EditQuestionCommand = new RelayCommand(_ => EditQuestion());
            SaveTestCommand = new RelayCommand(_ => SaveTest());
            DeleteTestCommand = new RelayCommand(_ => DeleteTest());
            BackCommand = new RelayCommand(_ => _main.ShowParticipantPage());

            LoadExistingTests();
        }

        private void AddQuestion()
        {
            if (!TryGetQuestionsCount(out int maxQuestions))
                return;

            if (_questions.Count >= maxQuestions)
            {
                MessageBox.Show($"Нельзя добавить больше {maxQuestions} вопросов.");
                return;
            }

            if (!TryBuildQuestion(out Question question))
                return;

            _questions.Add(question);
            QuestionsPreview.Add(BuildQuestionPreview(_questions.Count, question));

            ClearQuestionFields();
        }

        private void EditQuestion()
        {
            int index = GetSelectedQuestionIndex();

            if (index < 0)
                return;

            FillQuestionFields(_questions[index]);
            IsEditingQuestion = true;
        }

        private void SaveQuestionChanges()
        {
            int index = GetSelectedQuestionIndex();

            if (index < 0)
                return;

            if (!TryBuildQuestion(out Question question))
                return;

            _questions[index] = question;
            QuestionsPreview[index] = BuildQuestionPreview(index + 1, question);

            ClearQuestionFields();

            MessageBox.Show("Вопрос изменён");
        }

        private void DeleteQuestion()
        {
            int index = GetSelectedQuestionIndex();

            if (index < 0)
                return;

            _questions.RemoveAt(index);
            QuestionsPreview.RemoveAt(index);

            RefreshQuestionsPreview();
            ClearQuestionFields();

            MessageBox.Show("Вопрос удалён");
        }

        private void SaveTest()
        {
            try
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

                if (_questions == null || _questions.Count != requiredCount)
                {
                    MessageBox.Show($"Нужно добавить {requiredCount} вопросов. Сейчас добавлено: {_questions?.Count ?? 0}.");
                    return;
                }

                foreach (var q in _questions)
                {
                    if (string.IsNullOrWhiteSpace(q.Text))
                    {
                        MessageBox.Show("У одного из вопросов пустой текст");
                        return;
                    }

                    if (q.Answers == null || q.Answers.Count == 0)
                    {
                        MessageBox.Show($"У вопроса \"{q.Text}\" нет вариантов ответа");
                        return;
                    }
                }

                var tests = LoadAllTests() ?? new List<Test>();

                var newTest = new Test
                {
                    Name = TestName.Trim(),
                    LowMax = 10,
                    MediumMax = 20,
                    Questions = _questions.ToList()
                };

                var existing = tests.FirstOrDefault(t =>
                    string.Equals(t.Name, newTest.Name, StringComparison.OrdinalIgnoreCase));

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
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при сохранении теста:\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

            int removed = tests.RemoveAll(t =>
                string.Equals(t.Name, SelectedExistingTest, StringComparison.OrdinalIgnoreCase));

            if (removed == 0)
            {
                MessageBox.Show("Тест не найден в Data/tests.json");
                return;
            }

            SaveAllTests(tests);
            DeleteTestResults(SelectedExistingTest);

            ClearTestEditor();
            LoadExistingTests();

            MessageBox.Show("Тест удалён");
        }

        private void DeleteTestResults(string testName)
        {
            try
            {
                _db.DeleteTestResultsByName(testName);
            }
            catch
            {
                MessageBox.Show(
                    "Тест удалён из JSON, но результаты из базы данных удалить не удалось.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void LoadExistingTests()
        {
            ExistingTests.Clear();

            foreach (var test in LoadAllTests())
                ExistingTests.Add(test.Name);
        }

        private void LoadTestIntoEditor(string testName)
        {
            var test = LoadAllTests()
                .FirstOrDefault(x => string.Equals(x.Name, testName, StringComparison.OrdinalIgnoreCase));

            if (test == null)
                return;

            TestName = test.Name;
            LowMaxText = test.LowMax.ToString();
            MediumMaxText = test.MediumMax.ToString();
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

        private List<Test> LoadAllTests()
        {
            return TestLoader.LoadTests();
        }

        private void SaveAllTests(List<Test> tests)
        {
            var directory = Path.GetDirectoryName(_testsPath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonConvert.SerializeObject(
                tests,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            File.WriteAllText(_testsPath, json);
        }

        private bool TryValidateTest(out int requiredCount, out int lowMax, out int mediumMax)
        {
            requiredCount = 0;
            lowMax = 0;
            mediumMax = 0;

            if (string.IsNullOrWhiteSpace(TestName))
            {
                MessageBox.Show("Введите название теста");
                return false;
            }

            if (!int.TryParse(QuestionsCountText, out requiredCount) || requiredCount <= 0)
            {
                MessageBox.Show("Введите корректное количество вопросов");
                return false;
            }

            if (!int.TryParse(LowMaxText, out lowMax) || lowMax < 0)
            {
                MessageBox.Show("Введите корректную границу низкого уровня");
                return false;
            }

            if (!int.TryParse(MediumMaxText, out mediumMax) || mediumMax <= lowMax)
            {
                MessageBox.Show("Введите корректную границу среднего уровня");
                return false;
            }

            return true;
        }

        private bool TryGetQuestionsCount(out int count)
        {
            if (!int.TryParse(QuestionsCountText, out count) || count <= 0)
            {
                MessageBox.Show("Введите корректное количество вопросов");
                return false;
            }

            return true;
        }

        private bool TryBuildQuestion(out Question question)
        {
            question = null;

            if (string.IsNullOrWhiteSpace(QuestionText))
            {
                MessageBox.Show("Введите текст вопроса");
                return false;
            }

            var answers = BuildAnswers();

            if (answers.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один ответ");
                return false;
            }

            question = new Question
            {
                Text = QuestionText.Trim(),
                Answers = answers
            };

            return true;
        }

        private List<Answer> BuildAnswers()
        {
            var answers = new List<Answer>();

            AddAnswerIfValid(answers, Answer1Text, Answer1ValueText);
            AddAnswerIfValid(answers, Answer2Text, Answer2ValueText);
            AddAnswerIfValid(answers, Answer3Text, Answer3ValueText);

            return answers;
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

        private int GetSelectedQuestionIndex()
        {
            if (string.IsNullOrWhiteSpace(SelectedAddedQuestion))
            {
                MessageBox.Show("Выберите вопрос");
                return -1;
            }

            int index = QuestionsPreview.IndexOf(SelectedAddedQuestion);

            if (index < 0 || index >= _questions.Count)
                return -1;

            return index;
        }

        private void ClearTestEditor()
        {
            _selectedExistingTest = null;
            OnPropertyChanged(nameof(SelectedExistingTest));

            TestName = string.Empty;
            LowMaxText = "32";
            MediumMaxText = "66";
            QuestionsCountText = string.Empty;

            _questions.Clear();
            QuestionsPreview.Clear();

            ClearQuestionFields();
        }

        private void ClearQuestionFields()
        {
            SelectedAddedQuestion = null;
            IsEditingQuestion = false;

            QuestionText = string.Empty;
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
                ClearAnswerFields();
                return;
            }

            QuestionText = question.Text;

            Answer1Text = GetAnswerText(question, 0);
            Answer1ValueText = GetAnswerValue(question, 0);

            Answer2Text = GetAnswerText(question, 1);
            Answer2ValueText = GetAnswerValue(question, 1);

            Answer3Text = GetAnswerText(question, 2);
            Answer3ValueText = GetAnswerValue(question, 2);
        }

        private void ClearAnswerFields()
        {
            QuestionText = string.Empty;
            Answer1Text = "Нет";
            Answer1ValueText = "0";
            Answer2Text = "Иногда";
            Answer2ValueText = "1";
            Answer3Text = "Да";
            Answer3ValueText = "2";
        }

        private string GetAnswerText(Question question, int index)
        {
            return question.Answers != null && question.Answers.Count > index
                ? question.Answers[index].Text
                : string.Empty;
        }

        private string GetAnswerValue(Question question, int index)
        {
            return question.Answers != null && question.Answers.Count > index
                ? question.Answers[index].Value.ToString()
                : string.Empty;
        }

        private void RefreshQuestionsPreview()
        {
            QuestionsPreview.Clear();

            for (int i = 0; i < _questions.Count; i++)
                QuestionsPreview.Add(BuildQuestionPreview(i + 1, _questions[i]));
        }

        private string BuildQuestionPreview(int number, Question question)
        {
            var answers = question.Answers == null
                ? string.Empty
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