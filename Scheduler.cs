/*
    Ander Ingvarsson 2021
    
    Класс ежедневника
    Создание, удаление, редактирование и хранение встреч.
*/
using System;
using System.Linq;
using System.Collections.Generic;

class Scheduler {
    /// Запланированные встречи
    private List<Meeting> _meetings;
    /// Используемый валидатор
    private IValidator _validator;
    /// Выбранная в данный момент встреча
    private Meeting _selected;


    public Scheduler(IValidator validator) {
        _meetings = new List<Meeting>();
        _validator = validator;
    }

    public Meeting[] Meetings {
        get { return _meetings.ToArray(); }
        set {}
    }
    public Meeting Selected {
        get { return _selected; }
        set { _selected = value; }
    }
    /// Добавление встречи
    public bool AddMeeting(Meeting meeting) {
        // проверка валидатором и сортировка
        if (_validator.Validate(meeting, _meetings.ToArray())) {
            _meetings.Add(meeting);
            _meetings = _meetings.OrderBy(x => x.Start).ToList();
            _selected = meeting;
            return true;
        } 
        return false;
    }
    /// Удаление встречи по индексу в списке
    public bool CancelMeeting(int index) {
        if ((index >= 0) && (index < _meetings.Count)) {
            _meetings.RemoveAt(index);
            return true;
        }
        return false;
    }
    /// Удаление встречи по времени начала
    public bool CancelMeeting(DateTime startTime) {
        var meeting = _meetings.Find(m => m.Start == startTime);
        if (meeting != null)
            return _meetings.Remove(meeting);
        return false;
    }
    /// Удаление встречи по ссылке
    public bool CancelMeeting(Meeting meeting) {
        return _meetings.Remove(meeting);
    }
    /// Изменение выбранной встречи
    public bool UpdateMeeting(Meeting meeting){
        // выбранная встреча удаляется из списка и заменяется на новую
        // если валидатор не позволит - выбранная опять добавляется в список
        if (_selected != null) {
            _meetings.Remove(_selected);
            if (AddMeeting(meeting)) {
                _selected = meeting;        
                return true;
            }
            AddMeeting(_selected);
        }
        return false;
    }
    
    public enum MeetingTime { Start, End }
    /// Изменение времени (начала или конца) для встречи по индексу в списке
    public bool ChangeMeetingTime(int index, DateTime newTime, MeetingTime mt) {
        bool result = false;
        if ((index >= 0) && (index < _meetings.Count)) {
            // получаем ссылку на встречу и удаляем её из списка
            var temp = _meetings[index];
            _meetings.RemoveAt(index);
            // сохраняем значение времени и изменяем его
            DateTime oldTime = default;
            switch (mt) {
                case MeetingTime.Start:
                    oldTime = temp.Start;
                    temp.Start = newTime;
                    break;
                case MeetingTime.End:
                    oldTime = temp.End;
                    temp.End = newTime;
                    break;
            }
            // проверяем валидатором и если не пропустил - возвращаем старое время
            if (!_validator.Validate(temp, _meetings.ToArray())) {
                switch (mt) {
                    case MeetingTime.Start:
                        temp.Start = oldTime;
                        break;
                    case MeetingTime.End:
                        temp.End = oldTime;
                        break;
                }
            // если пропустил - регистрируем результат как успешный
            } else result = true;
            // возвращаем ссылку в список
            _meetings.Add(temp);
        }
        // возвращаем результат
        return result;
    }
}