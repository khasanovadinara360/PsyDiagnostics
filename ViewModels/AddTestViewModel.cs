using Newtonsoft.Json;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
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
        private readonly List<Question> _questions = new();

        public ObservableCollection<string> QuestionsPreview { get; set; } = new();

        private string _testName;
        public string TestName
        {
            get => _testName;
            set { _testName = value; OnPropertyChanged(); }
        }

        private string _lowMaxText = "40";
        public string LowMaxText
        {
            get => _lowMaxText;
            set { _lowMaxText = value; OnPropertyChanged(); }
        }

        private string _mediumMaxText = "70";
        public string MediumMaxText
        {
            get => _mediumMaxText;
            set { _mediumMaxText = value; OnPropertyChanged(); }
        }

        private string _questionText;
        public string QuestionText
        {
            get => _questionText;
            set { _questionText = value; OnPropertyChanged(); }
        }

        public string Answer1Text { get; set; } = "Нет";
        public string Answer1ValueText { get; set; } = "0";

        public string Answer2Text { get; set; } = "Иногда";
        public string Answer2ValueText { get; set; } = "1";

        public string Answer3Text { get; set; } = "Часто";
        public string Answer3ValueText { get; set; } = "2";

        public ICommand AddQuestionCommand { get; }
        public ICommand SaveTestCommand { get; }
        public ICommand BackCommand { get; }

        public AddTestViewModel(MainViewModel main)
        {
            _main = main;

            AddQuestionCommand = new RelayCommand(_ => AddQuestion());
            SaveTestCommand = new RelayCommand(_ => SaveTest());
            BackCommand = new RelayCommand(_ => _main.ShowParticipantPage());
        }

        private void AddQuestion()
        {
            if (string.IsNullOrWhiteSpace(QuestionText))
            {
                MessageBox.Show("Введите текст вопроса.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Answer1Text) ||
                string.IsNullOrWhiteSpace(Answer2Text) ||
                string.IsNullOrWhiteSpace(Answer3Text))
            {
                MessageBox.Show("Заполните все варианты ответов.");
                return;
            }

            if (!int.TryParse(Answer1ValueText, out int value1) ||
                !int.TryParse(Answer2ValueText, out int value2) ||
                !int.TryParse(Answer3ValueText, out int value3))
            {
                MessageBox.Show("Значения ответов должны быть числами.");
                return;
            }

            var question = new Question
            {
                Text = QuestionText.Trim(),
                Answers = new List<Answer>
                {
                    new Answer { Text = Answer1Text.Trim(), Value = value1 },
                    new Answer { Text = Answer2Text.Trim(), Value = value2 },
                    new Answer { Text = Answer3Text.Trim(), Value = value3 }
                }
            };

            _questions.Add(question);
            QuestionsPreview.Add($"{_questions.Count}. {question.Text}");

            QuestionText = "";
            OnPropertyChanged(nameof(QuestionText));
        }

        private void SaveTest()
        {
            if (string.IsNullOrWhiteSpace(TestName))
            {
                MessageBox.Show("Введите название теста.");
                return;
            }

            if (!int.TryParse(LowMaxText, out int lowMax) ||
                !int.TryParse(MediumMaxText, out int mediumMax))
            {
                MessageBox.Show("Границы уровней должны быть числами.");
                return;
            }

            if (mediumMax <= lowMax)
            {
                MessageBox.Show("Средний уровень должен быть больше низкого.");
                return;
            }

            if (_questions.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один вопрос.");
                return;
            }

            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "tests.json");

            if (!File.Exists(path))
            {
                MessageBox.Show("Файл tests.json не найден.");
                return;
            }

            List<Test> tests;

            try
            {
                string json = File.ReadAllText(path);
                tests = JsonConvert.DeserializeObject<List<Test>>(json) ?? new List<Test>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка чтения tests.json:\n" + ex.Message);
                return;
            }

            if (tests.Any(t => t.Name.Equals(TestName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Тест с таким названием уже существует.");
                return;
            }

            tests.Add(new Test
            {
                Name = TestName.Trim(),
                LowMax = lowMax,
                MediumMax = mediumMax,
                Questions = _questions
            });

            try
            {
                string updatedJson = JsonConvert.SerializeObject(tests, Formatting.Indented);
                File.WriteAllText(path, updatedJson);

                MessageBox.Show("Тест успешно добавлен.");
                _main.ShowParticipantPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения tests.json:\n" + ex.Message);
            }
        }
    }
}