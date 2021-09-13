/*
    Ander Ingvarsson 2021
    
    Класс встречи
    Закрытый конструктор, ибо нужно проверять переданные параметры на правильность:
        время начала - позже текущего времени
        время окончания - позже времени начала
    Значения времени начала и окончания нельзя изменить после создания.
    Описание встречи не обязательно.
    Время оповещения по умолчанию - за 10 минут до.
*/
using System;

class Meeting {
    /// Время начала
    private DateTime _startTime = default;
    /// Время окончания
    private DateTime _endTime = default;
    /// Время оповещения
    private DateTime _alert = default;
    /// Краткое описание
    private string _desc;
    /// Статус оповещения
    private bool _alertState = true;

    private Meeting(DateTime start, DateTime end,
                    DateTime alert = default, string desc = "") {
        _startTime = start;
        _endTime = end;
        _alert = alert == default ?_startTime.AddMinutes(-10) : alert;
        _desc = desc;
    }
    public static Meeting CreateInstance(DateTime start,
                                         DateTime end,
                                         DateTime alert,
                                         string desc) {
        if ((start > DateTime.Now) && (end > start))
            return new Meeting(start, end, alert, desc);
        return null;
    }

    public DateTime Start {
        get { return _startTime; }
        set { }
    }
    public DateTime End {
        get { return _endTime; }
        set { }
    }
    public DateTime Alert {
        get { return _alert; }
        set {
            if (value < _startTime)
                _alert = value;
        }
    }
    public string Description {
        get { return _desc; }
        set { _desc = value; }
    }
    public bool AlertState {
        get { return _alertState; }
        set { _alertState = value; }    
    }
    public override string ToString() {
        return string.Format("{0} -> {1}", Start.ToString(), End.ToString());
    }
}