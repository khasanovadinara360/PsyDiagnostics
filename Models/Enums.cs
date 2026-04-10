using System;
using System.ComponentModel;

namespace PsyDiagnostics.Models
{
    public enum Citizenship
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("РФ")]
        РФ,

        [Description("Беларусь")]
        Беларусь,

        [Description("Казахстан")]
        Казахстан,

        [Description("Украина")]
        Украина,

        [Description("Другое")]
        Другое
    }

    public enum EducationLevel
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Нет образования")]
        Нет,

        [Description("Среднее образование")]
        Среднее,

        [Description("Среднее специальное образование")]
        СреднееСпециальное,

        [Description("Высшее образование")]
        Высшее
    }

    public enum MaritalStatus
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Не женат / не замужем")]
        НеЖенат,

        [Description("Женат / замужем")]
        Женат,

        [Description("Разведен(а)")]
        Разведен
    }

    public enum CrimeType
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Кража")]
        Кража,

        [Description("Мошенничество")]
        Мошенничество,

        [Description("Разбой")]
        Разбой,

        [Description("Убийство")]
        Убийство
    }

    public enum Recidivism
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Нет рецидива")]
        Нет,

        [Description("Есть рецидив")]
        Да
    }

    public enum Category
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Первоход")]
        Первоход,

        [Description("Второход")]
        Второход
    }

    public enum Gender
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Мужской")]
        Мужской,

        [Description("Женский")]
        Женский
    }

    public enum FamilyUpbringing
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("В полной семье")]
        ПолнаяСемья,

        [Description("Сирота")]
        Сирота,

        [Description("Воспитывала только мать")]
        ТолькоМать,

        [Description("Воспитывал только отец")]
        ТолькоОтец,

        [Description("Воспитывали бабушка и дедушка")]
        БабушкаИДедушка,

        [Description("В детском доме")]
        ДетскийДом,

        [Description("Иное")]
        Иное
    }

    public enum YesNo
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Да")]
        Да,

        [Description("Нет")]
        Нет
    }

    public enum ChildrenPresence
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Детей нет")]
        Нет,

        [Description("Есть дети")]
        Да
    }

    public enum EducationSurvey
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Нет образования")]
        НетОбразования,

        [Description("Начальное общее образование")]
        НачальноеОбщее,

        [Description("Основное общее образование")]
        ОсновноеОбщее,

        [Description("Среднее общее образование")]
        СреднееОбщее,

        [Description("Среднее профессиональное образование")]
        СреднееПрофессиональное,

        [Description("Высшее образование")]
        Высшее
    }

    public enum ProfessionPresence
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Имею профессию")]
        Да,

        [Description("Не имею профессии")]
        Нет
    }

    public enum Religion
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Не верю в Бога (атеист)")]
        Атеист,

        [Description("Православие")]
        Православие,

        [Description("Ислам")]
        Ислам,

        [Description("Буддизм")]
        Буддизм,

        [Description("Иудаизм")]
        Иудаизм,

        [Description("Иное")]
        Иное
    }

    public enum ArmyService
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Не служил(а) в армии")]
        Нет,

        [Description("Служил(а) в армии")]
        Да,
    }

    public enum CombatParticipation
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Не участвовал(а) в боевых действиях")]
        Нет,

        [Description("Участвовал(а) в боевых действиях")]
        Да,
    }

    public enum SomaticDiseases
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Нет тяжелых соматических заболеваний")]
        Нет,

        [Description("Туберкулез")]
        Туберкулез,

        [Description("ВИЧ")]
        ВИЧ,

        [Description("Гепатит")]
        Гепатит,

        [Description("Иное")]
        Иное
    }

    public enum Disability
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,


        [Description("Инвалидности нет")]
        Нет,

        [Description("Есть инвалидность")]
        Да,
    }

    public enum MentalDiseases
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,


        [Description("Психических заболеваний нет")]
        Нет,

        [Description("Есть психические заболевания")]
        Да,
    }

    public enum PsychiatristRegistry
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,


        [Description("Не состоял(а) на учете у психиатра")]
        Нет,

        [Description("Состоял(а) на учете у психиатра")]
        Да
    }

    public enum Gambling
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,


        [Description("Не принимал(а) участия в азартных играх")]
        Нет,

        [Description("Принимал(а) участие в азартных играх")]
        Да,
    }

    public enum Obligations
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Нет обязательств")]
        Нет,

        [Description("Потребительский кредит")]
        ПотребительскийКредит,

        [Description("Ипотечный кредит")]
        ИпотечныйКредит,

        [Description("Микрозайм в МФО")]
        МикрозаймВМФО,

        [Description("Кредитная карта")]
        КредитнаяКарта,

        [Description("Долг физическому лицу (займ)")]
        ДолгФизическомуЛицу,

        [Description("Иное")]
        Иное
    }

    public enum NarcologistRegistry
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,


        [Description("Не состоял(а) на учете у нарколога")]
        Нет,

        [Description("Состоял(а) на учете у нарколога")]
        Да,
    }

    public enum DrugUse
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,


        [Description("Не употреблял(а) наркотические средства")]
        Нет,

        [Description("Употреблял(а) наркотические средства")]
        Да,

        [Description("Пробовал(а) наркотические средства")]
        Пробовал,
    }

    public enum SuicideAttempts
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Нет попыток суицида")]
        Нет,

        [Description("Да, в ИУ")]
        ДаВИУ,

        [Description("Да, на свободе")]
        ДаНаСвободе,

        [Description("Да, в СИЗО")]
        ДаВСИЗО,
    }

    public enum SelfHarmScars
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Нет шрамов от самопорезов")]
        Нет,

        [Description("Есть шрамы от самопорезов")]
        Да,
    }

    public enum RelativesSuicide
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,


        [Description("Не было суицидов/попыток у родственников")]
        Нет,

        [Description("Были суициды/попытки у родственников")]
        Да,
    }

    public enum CurrentFeelings
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Одиночество, тоска")]
        ОдиночествоТоска,

        [Description("Раздражение, напряжение, агрессия")]
        РаздражениеНапряжениеАгрессия,

        [Description("Оптимизм, надежда на будущее")]
        ОптимизмНадежда,

        [Description("Решимость улучшить свою жизнь")]
        РешимостьУлучшитьЖизнь,

        [Description("Подавленность, безысходность")]
        ПодавленностьБезысходность,

        [Description("Спокойствие")]
        Спокойствие,

        [Description("Готовность постоять за себя")]
        ГотовностьПостоятьЗаСебя,

        [Description("Тревога, страх, растерянность")]
        ТревогаСтрахРастерянность,

        [Description("Иное")]
        Иное
    }

    public enum AttitudeToUIS
    {
        [Description("Не выбрано")]
        НеВыбрано = 0,

        [Description("Отношусь спокойно")]
        ОтношусьСпокойно,

        [Description("Думаю, что зря я вновь сюда попал")]
        ЗряВновьПопал,

        [Description("Сожалею о совершенном преступлении")]
        СожалеюОПреступлении,

        [Description("Мне здесь лучше, чем на свободе")]
        ЗдесьЛучшеЧемНаСвободе,

        [Description("Думаю, что здесь я не в последний раз")]
        НеВПоследнийРаз,

        [Description("Думаю, что это просто стечение обстоятельств")]
        СтечениеОбстоятельств,

        [Description("Другое")]
        Другое
    }
}