using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsyDiagnostics.Models
{
    public enum Citizenship
    {
        НеВыбрано = 0,
        РФ,
        Беларусь,
        Казахстан,
        Украина,
        Другое
    }
    public enum EducationLevel
    {
        НеВыбрано = 0,
        Нет,
        Среднее,
        СреднееСпециальное,
        Высшее
    }

    public enum MaritalStatus
    {
        НеВыбрано = 0,
        НеЖенат,
        Женат,
        Разведен
    }

    public enum CrimeType
    {
        НеВыбрано = 0,
        Кража,
        Мошенничество,
        Разбой,
        Убийство
    }

    public enum Recidivism
    {
        НеВыбрано = 0,
        Нет,
        Да
    }

    public enum Category
    {
        НеВыбрано = 0,
        Первоход,
        Второход
    }
}