using Newtonsoft.Json;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace PsyDiagnostics.ViewModels
{
    public class AddTestViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
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

        private void AddQuestion()
        {
            if (!int.TryParse(QuestionsCountText, out int maxQuestions) || maxQuestions <= 0)
            {
                MessageBox.Show("Введите корректное количество вопросов");
                return;
            }

            if (_questions.Count >= maxQuestions)
            {
                MessageBox.Show($"Нельзя добавить больше {maxQuestions} вопросов."); return;
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

            for (int i = 0; i < QuestionsPreview.Count; i++)
                QuestionsPreview[i] = BuildQuestionPreview(i + 1, _questions[i]);

            ClearQuestionFields();

            MessageBox.Show("Вопрос удалён");
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

        private string GetQuestionWord(int count)
        {
            if (count % 10 == 1 && count % 100 != 11)
                return "вопрос";

            if (count % 10 >= 2 && count % 10 <= 4 &&
                !(count % 100 >= 12 && count % 100 <= 14))
                return "вопроса";

            return "вопросов";
        }

        private string BuildQuestionPreview(int number, Question question)
        {
            var answers = question.Answers == null
                ? ""
                : string.Join("; ", question.Answers.Select(a => $"{a.Text} ({a.Value})"));

            return $"{number}. {question.Text}\nОтветы: {answers}";
        }

        private IEnumerable<string> GetTestsFolders()
        {
            var folders = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "Tests"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests"),
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Tests"))
            };

            return folders.Distinct();
        }

        private string GetWritableTestsFolder()
        {
            foreach (var folder in GetTestsFolders())
            {
                if (Directory.Exists(folder))
                    return folder;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "Tests");
        }

        private List<Test> LoadAllTestsFromFolder()
        {
            var result = new List<Test>();

            foreach (var folder in GetTestsFolders())
            {
                if (!Directory.Exists(folder))
                    continue;

                foreach (var file in Directory.GetFiles(folder, "*.json"))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var test = System.Text.Json.JsonSerializer.Deserialize<Test>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (test != null)
                        {
                            if (string.IsNullOrWhiteSpace(test.Name))
                                test.Name = Path.GetFileNameWithoutExtension(file);

                            result.Add(test);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return result
                .GroupBy(x => x.Name)
                .Select(g => g.First())
                .OrderBy(x => x.Name)
                .ToList();
        }

        private void LoadExistingTests()
        {
            ExistingTests.Clear();

            var tests = LoadAllTestsFromFolder();

            foreach (var test in tests)
            {
                if (!string.IsNullOrWhiteSpace(test.Name))
                    ExistingTests.Add(test.Name);
            }
        }
        private void LoadTestIntoEditor(string testName)
        {
            var test = LoadAllTestsFromFolder()
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

            var first = test.Questions?.FirstOrDefault();
            FillQuestionFields(first);
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

            Answer1Text = question.Answers != null && question.Answers.Count > 0 ? question.Answers[0].Text : "";
            Answer1ValueText = question.Answers != null && question.Answers.Count > 0 ? question.Answers[0].Value.ToString() : "";

            Answer2Text = question.Answers != null && question.Answers.Count > 1 ? question.Answers[1].Text : "";
            Answer2ValueText = question.Answers != null && question.Answers.Count > 1 ? question.Answers[1].Value.ToString() : "";

            Answer3Text = question.Answers != null && question.Answers.Count > 2 ? question.Answers[2].Text : "";
            Answer3ValueText = question.Answers != null && question.Answers.Count > 2 ? question.Answers[2].Value.ToString() : "";
        }

        private void DeleteTest()
        {
            if (string.IsNullOrWhiteSpace(SelectedExistingTest))
            {
                MessageBox.Show("Выберите тест для удаления");
                return;
            }

            var result = MessageBox.Show(
                $"Удалить тест \"{SelectedExistingTest}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            bool deleted = false;

            foreach (var folder in GetTestsFolders())
            {
                if (!Directory.Exists(folder))
                    continue;

                var directPath = Path.Combine(folder, $"{SelectedExistingTest}.json");

                if (File.Exists(directPath))
                {
                    File.Delete(directPath);
                    deleted = true;
                    continue;
                }

                foreach (var file in Directory.GetFiles(folder, "*.json"))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var test = System.Text.Json.JsonSerializer.Deserialize<Test>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (test != null && test.Name == SelectedExistingTest)
                        {
                            File.Delete(file);
                            deleted = true;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (!deleted)
            {
                MessageBox.Show("Файл теста не найден");
                return;
            }

            _selectedExistingTest = null;
            OnPropertyChanged(nameof(SelectedExistingTest));

            TestName = "";
            QuestionsCountText = "";
            QuestionText = "";
            Answer1Text = "Нет";
            Answer1ValueText = "0";
            Answer2Text = "Иногда";
            Answer2ValueText = "1";
            Answer3Text = "Да";
            Answer3ValueText = "2";

            _questions.Clear();
            QuestionsPreview.Clear();

            LoadExistingTests();

            MessageBox.Show("Тест удалён");
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
                    $"Нужно добавить {requiredCount} {word}. Сейчас добавлено: {_questions.Count}."
                );
                return;
            }

            var newTest = new Test
            {
                Name = TestName.Trim(),
                LowMax = 32,
                MediumMax = 66,
                Questions = _questions.ToList()
            };

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tests.json");

            if (!File.Exists(path))
            {
                MessageBox.Show("Файл Data/tests.json не найден");
                return;
            }

            var json = File.ReadAllText(path);

            var tests = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Test>>(json)
                        ?? new List<Test>();

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

            var updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                tests,
                Newtonsoft.Json.Formatting.Indented
            );

            File.WriteAllText(path, updatedJson);

            LoadExistingTests();

            MessageBox.Show("Тест успешно сохранён");

            _main.ShowParticipantPage();
        }
    }
}