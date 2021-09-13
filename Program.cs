/*
    Ander Ingvarsson 2021
    Тестовое задание для Систем Консалт
    В используются фреймворки .NET 5.0, Terminal.Gui
*/
using System;
using System.Threading;
using System.IO;

namespace TermGUI
{
    class Program
    {
        // ежедневник
        private static Scheduler _scheduler;
        // графический интрефейс
        private static GUI _gui;

        static void Main(string[] args)
        {
            // инициализация таймера для обновления текущего времени и проверки оповещений
            var tmr = new Timer(OnTimerEvent);
            // инициализация основных компонентов
            _scheduler = new Scheduler(new MeetingValidator());
            _gui = GUI.GetInstance();
            _gui.Init("Ежедневник");
            // регистрация обработчика событий интерфейса
            _gui.OnEvent += OnGuiEvent;
            
            // заполнение списка встречь
            Prefill();
            tmr.Change(1000, 1000);
            // запуск основного цикла графического интерфейса
            _gui.Run();
            
            // остановка и уничтожение таймера
            tmr.Change(Timeout.Infinite, Timeout.Infinite);
            tmr.Dispose();
        }

        /// Предварительное заполнение ежедневника встречами для тестрования
        private static void Prefill() {
            var meetings = new Meeting[] {
                Meeting.CreateInstance(
                    DateTime.Now.AddMinutes(1), 
                    DateTime.Now.AddMinutes(2),
                    default,
                    "проверка оповещения 1"
                    ),
                Meeting.CreateInstance(
                    DateTime.Now.AddMinutes(4), 
                    DateTime.Now.AddMinutes(5),
                    default,
                    "проверка оповещения 2"
                    ),
                Meeting.CreateInstance(
                    DateTime.Parse("13.11.2021 08:05"), 
                    DateTime.Parse("13.11.2021 09:35"),
                    default,
                    "item 5"
                    ),
                Meeting.CreateInstance(
                    DateTime.Parse("13.11.2021 09:05"), 
                    DateTime.Parse("13.11.2021 09:35"),
                    default,
                    "не добавится - коллизия с item 5"
                    ),
                Meeting.CreateInstance(
                    DateTime.Parse("11.11.2021 08:02"), 
                    DateTime.Parse("11.11.2021 09:32"),
                    default,
                    "item 2"
                    ),
                Meeting.CreateInstance(
                    DateTime.Parse("11.11.2021 18:03"), 
                    DateTime.Parse("11.11.2021 19:33"),
                    default,
                    "item 3"
                    ),
                Meeting.CreateInstance(
                    DateTime.Parse("10.11.2021 08:01"), 
                    DateTime.Parse("10.11.2021 09:31"),
                    default,
                    "item 1"
                    ),
                Meeting.CreateInstance(
                    DateTime.Parse("10.11.2021 08:01"), 
                    DateTime.Parse("10.11.2021 09:31"),
                    default,
                    "не добавится - уже есть"
                    ),
                Meeting.CreateInstance(
                    DateTime.Parse("12.11.2021 08:04"),
                    DateTime.Parse("12.11.2021 09:34"), 
                    default,
                    "item 4"
                    ),
                Meeting.CreateInstance(
                    DateTime.Parse("12.11.2021 09:35"), 
                    DateTime.Parse("12.11.2021 08:05"),
                    default,
                    "не добавится - время перепутано"
                    )
            };
            foreach (var item in meetings)
                if (item != null)
                    _scheduler.AddMeeting(item);
            _gui.RefreshMeetings(_scheduler.Meetings);
        }
        /// Обработка событий интерфейса
        private static void OnGuiEvent(object sender, GUIEventArgs args) {
            switch (args.EventType) {
                // фильтрация по дате
                case GUI.EventTypes.Filter:
                    _gui.RefreshMeetings(_scheduler.Meetings);
                    break;
                // выбор встречи из списка
                case GUI.EventTypes.Select:
                    _scheduler.Selected = args.EventData as Meeting;
                    _gui.RefreshMeetingInfo(_scheduler.Selected);
                    break;
                // создание новой встречи
                case GUI.EventTypes.Create:
                    if (_scheduler.AddMeeting(args.EventData as Meeting))
                        _gui.RefreshMeetings(_scheduler.Meetings);
                    else
                        _gui.ShowErrorMessage(
                            "Не удалось создать встречу.\n" +
                            "Проверьте введенное время." 
                        );
                    break;
                // изменение информации о встрече
                case GUI.EventTypes.Update:
                    if ((_scheduler.Selected != null) &&
                        (_gui.ShowQueryMessage("Вы уверены, что хотете обновить встречу?"))) {

                        if (_scheduler.UpdateMeeting(args.EventData as Meeting))
                            _gui.RefreshMeetings(_scheduler.Meetings);
                        else 
                            _gui.ShowErrorMessage(
                                "Не удалось обновить встречу.\n" +
                                "Проверьте введенное время." 
                            );
                    }
                    break;
                // удаление встречи
                case GUI.EventTypes.Remove:
                    if ((_scheduler.Selected != null) &&
                        (_gui.ShowQueryMessage("Вы уверены, что хотете отменить встречу?"))) {

                        _scheduler.CancelMeeting(_scheduler.Selected);
                        _gui.RefreshMeetings(_scheduler.Meetings);
                    } else {
                        _gui.ShowErrorMessage("Не удалось удалить встречу.");
                    }
                    break;
                // сохранение текущего списка встреч в файл
                case GUI.EventTypes.Save:
                    var meetings = args.EventData as Meeting[];
                    var lines = new string[meetings.Length];
                    for(var i = 0; i < lines.Length; i++)
                        lines[i] = meetings[i].ToString() + " :: " + meetings[i].Description; 
                    try {
                        File.WriteAllLines("./meetings.txt", lines);
                        _gui.ShowQueryMessage("Файл сохранен.");
                    } catch (IOException ex) {
                        Console.WriteLine("Error:\n{0}", ex.Message);
                    }
                    break;
            }
        }
        /// Обработка событий таймера
        private static void OnTimerEvent(object obj) {
            _gui.UpdateCurrentTime();
            _gui.RefreshAlerts();
            _gui.Refresh();
        }
    }
}
