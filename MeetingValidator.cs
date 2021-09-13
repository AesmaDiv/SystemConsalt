/*
    Ander Ingvarsson 2021
    
    Класс валидатора
    Проверка встречи на соответствие списку:
    1. Не равно null
    2. Не присутствует в списке
    3. Не пересекается с другими встречами списка
*/
using System;

class MeetingValidator : IValidator {
    public bool Validate(object item, object[] items) {
        var meeting = item as Meeting;
        var meetings = items as Meeting[];
        
        if (meeting != null)
            if (!CheckExist(meeting, meetings))
                return CheckNoCollisions(meeting, meetings);
        return false;
    }
    /// Проверка на присутствие в списке    
    private bool CheckExist(Meeting meeting, Meeting[] meetings) {
        return Array.Find(
            meetings, 
            m => (m.Start == meeting.Start) && (m.End == meeting.End)
        ) != null;
    }
    /// Проверка на пересечение с другими встречами
    private bool CheckNoCollisions(Meeting meeting, Meeting[] meetings) {
        var next = Array.Find(meetings, m =>  m.End > meeting.Start);
        if (next == null) return true;
        else return (next.Start > meeting.End);
    }
}