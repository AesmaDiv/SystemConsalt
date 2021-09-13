/*
    Ander Ingvarsson 2021
    
    Класс графического интерфейса на фреймворке Terminal.Gui
    Используется впервые, поэтому возможны глитчи.
*/
using System;
using Terminal.Gui;


class GUI {
    public event EventHandler<GUIEventArgs> OnEvent;
    // Дата для фильтрации
    DateField filterDate;
    // Текущее время
    Label lblCurrentTime;
    // Список встреч
    ListView lstMeetings;
    // Время начала выбранной встречи
    TextField txtStartTime;
    // Время окончания выбранной встречи
    TextField txtEndTime;
    // Время оповещения о выбранной встрече
    TextField txtAlertTime;
    // Краткое описание встречи
    TextField txtDescription;
    // Контейнер для сообщений об оповещении
    FrameView frmAlerts;
    
    private Meeting[] _meetings = new Meeting[0];
    /// Типы событий интерфейса
    public enum EventTypes {
        Filter, Select, Create, Update, Remove, Save, Quit
    }
    /// Инициализация интерфейса
    public void Init(string title) {
        Application.Init();
        Application.Top.Add(CreateMainWindow(title));
    }
    /// Запуск основного цилка интерфейса
    public void Run() {
        try {
            Application.Run();
        } catch (NullReferenceException ex) {
            Console.WriteLine("Error:\n{0}", ex.Message);
        } catch (IndexOutOfRangeException ex) {
            Console.WriteLine("Error:\n{0}", ex.Message);
        }
    }
    /// Обновление отображения элементов
    public void Refresh() {
        Application.Refresh();
    }
    /// Завершение основного цикла
    public void Quit() {
        if (ShowQueryMessage("Вы уверены, что хотите выйти?"))
            Application.Top.Running = false;
    }
    /// Отображение окна с запросом
    public bool ShowQueryMessage(string message) {
        return MessageBox.Query(
            50, 7,
            "Внимание",
            message, "_Да", "_Нет"
        ) == 0;
    }
    /// Отображение окна ошибки
    public void ShowErrorMessage(string message) {
        MessageBox.ErrorQuery("Ошибка", message);
    }
    /// Обновление списка встреч
    public void RefreshMeetings(Meeting[] meetings) {
        // если дата фильтра не равна 01.01.2001 - отобразить все
        // если равна - отобразить назначенные на указанную дату
        _meetings = meetings;
        lstMeetings.SetSource(
            filterDate.Date == DateTime.Parse("01.01.01").Date ? _meetings : 
            Array.FindAll<Meeting>(_meetings, m => m.Start.Date == filterDate.Date)
        );
        Application.Refresh();
    }
    /// Обновление полей информации о встрече
    public void RefreshMeetingInfo(Meeting meeting) {
        txtStartTime.Text = meeting.Start.ToString();
        txtEndTime.Text = meeting.End.ToString();
        txtAlertTime.Text = (meeting.Start - meeting.Alert).TotalMinutes.ToString();
        txtDescription.Text = meeting.Description;
        Application.Refresh();
    }
    /// Обновление оповещений о предстоящих встречах
    public void RefreshAlerts() {
        frmAlerts.Clear();
        // получение встреч чьё время оповещения подошло
        // и оповещение включено
        var meetings = Array.FindAll(
            _meetings,
            m => (m.AlertState) && (m.Alert <= DateTime.Now)
        );
        // добавление строк оповещений
        for(var i = 0; i < meetings.Length; i++){
            frmAlerts.Add(
                new Label(0, i, 
                    string.Format(
                        "Встреча {0} начнется через {1:0} минут.",
                        meetings[i].Start.ToString(),
                        (meetings[i].Start - DateTime.Now).TotalMinutes
                    )
                )
            );
            // отключение оповещения для встречи
            meetings[i].AlertState = false;
        }
    }
    /// Обновление текущего времени
    public void UpdateCurrentTime() {
        lblCurrentTime.Text = DateTime.Now.ToString();
    } 
    /// Создание основного окна интерфейса
    private Window CreateMainWindow(string title) {
        var result = new Window(title)
        { X = 0, Y = 1, Width = Dim.Fill(), Height = 26 };

        // фильтр по дате, сброс фильтра и текущее время
        var lbl = new Label()
        { X = 2, Y = 1, Text = "Фильтр по дате: "};
        filterDate = new DateField()
        { X = Pos.Right(lbl), Y = 1, Date = default, IsShortFormat = true };
        filterDate.DateChanged += OnFilterDate_Changed;
        var btnResetFilter = new Button()
        { X = Pos.Right(filterDate), Y = 1, Text ="Сбросить" };
        btnResetFilter.Clicked += () => filterDate.Date = DateTime.Parse("01.01.01");
        lblCurrentTime = new Label()
        { X = Pos.AnchorEnd(20), Y = 1, Text = DateTime.Now.ToString() };
        
        // список встреч
        lstMeetings = new ListView()
        { X = 0, Y = 0, Width = Dim.Fill(), Height = 10 };
        lstMeetings.SelectedItemChanged += OnMeetingSelected;
        var frmMeetings = new FrameView()
        { X = 0, Y = 2, Width = Dim.Percent(50), Height = 12};
        frmMeetings.Title = "Запланированные встречи:";
        
        // информация о встрече
        var frmInfo = new FrameView()
        { X = Pos.Right(frmMeetings), Y = 2, Width = Dim.Percent(50), Height = 12};
        frmInfo.Title = "Информация о встрече:";
        txtStartTime = new TextField()
        { X = 17, Y = 0, Width = 20 };
        txtEndTime = new TextField()
        { X = 17, Y = 1, Width = 20 };
        txtAlertTime = new TextField()
        { X = 17, Y = 3, Width = 7 };
        txtDescription = new TextField()
        { X = 0, Y = 6, Width = Dim.Fill(), Height = Dim.Fill()};

        // кнопки управления встречей
        var btnCreate = new Button() {
            X = 0, Y = Pos.AnchorEnd(1),
            Width = Dim.Percent(33), Text = "Создать"
        };
        btnCreate.Clicked += OnCreate_Clicked;
        
        var btnUpdate = new Button() {
            X = Pos.Right(btnCreate), Y = Pos.AnchorEnd(1),
            Width = Dim.Percent(33),  Text = "Обновить"
        };
        btnUpdate.Clicked += OnUpdate_Clicked;
        
        var btnRemove = new Button() {
            X = Pos.Right(btnUpdate), Y = Pos.AnchorEnd(1),
            Width = Dim.Percent(33),  Text = "Удалить"
        };
        btnRemove.Clicked += OnRemove_Clicked;

        // кнопка сохранения в файл
        var btnSave = new Button()
        { X = 1, Y = 15, Text = "Сохранить в файл" };
        btnSave.Clicked += OnSave_Clicked;
        // кнопка завершения приложения
        var btnQuit = new Button()
        { X = Pos.AnchorEnd(10), Y = 15, Text = "Выход" };
        btnQuit.Clicked += Quit;

        // поле оповещений
        frmAlerts = new FrameView("Информация: ")
        { X = 0, Y = Pos.AnchorEnd(7), Width = Dim.Fill(), Height = 7 };

        // сборка окна
        frmMeetings.Add(lstMeetings);
        frmInfo.Add(
            new Label(0, 0, "Время начала: "),
            new Label(0, 1, "Время окончания: "),
            new Label(0, 3, "Предупредить за "),
            new Label(0, 5, "Описание:"),
            new Label(26, 3, "минут"),
            txtStartTime,
            txtEndTime,
            txtAlertTime,
            txtDescription,
            btnCreate,
            btnUpdate,
            btnRemove
        );
        result.Add(
            lbl,
            filterDate,
            btnResetFilter,
            lblCurrentTime,

            frmMeetings,
            frmInfo,

            btnSave,
            btnQuit,
            frmAlerts
        );
        return result;
    }

    #region ОБРАБОТКА СОБЫТИЙ
    /// Применение фильтра
    private void OnFilterDate_Changed(DateTimeEventArgs<DateTime> args) {
        BroadcastEvent(new GUIEventArgs() 
        { EventType = EventTypes.Filter, EventData = args.NewValue });
    }
    /// Изменение выбранной встречи в списке
    private void OnMeetingSelected(object obj) {
        var args = obj as ListViewItemEventArgs;
        BroadcastEvent(new GUIEventArgs() 
        { EventType = EventTypes.Select, EventData = args.Value });
    }
    /// Нажатие кнопки Создать
    private void OnCreate_Clicked() {
        OnEventType_Clicked(EventTypes.Create);
    }
    /// Нажатие кнопки Обновить
    private void OnUpdate_Clicked() {
        OnEventType_Clicked(EventTypes.Update);
    }
    /// Шаблон обработчика для кнопок Создать и Обновить
    private void OnEventType_Clicked(EventTypes eventType) {
        DateTime start = DateTime.Parse(txtStartTime.Text.ToString());
        DateTime end = DateTime.Parse(txtEndTime.Text.ToString());
        if (int.TryParse(txtAlertTime.Text.ToString(), out int minutes)) {
            DateTime alert = start.AddMinutes(-minutes);
            BroadcastEvent(new GUIEventArgs() {
                EventType = eventType,
                EventData = Meeting.CreateInstance(
                    start, end, alert, txtDescription.Text.ToString()
                )
            });
        } else ShowErrorMessage("Неверно указано время оповещения.");
    }
    /// Нажатие кнопки Удалить
    private void OnRemove_Clicked() {
        BroadcastEvent(new GUIEventArgs() {
            EventType = EventTypes.Remove,
        });
    }
    /// Нажатие кнопки Сохранить в файл
    private void OnSave_Clicked() {
        BroadcastEvent(new GUIEventArgs() {
            EventType = EventTypes.Save,
            EventData = lstMeetings.Source.ToList()
        });
    }
    /// Трансляция события во вне
    private void BroadcastEvent(GUIEventArgs args) {
        if (OnEvent != null)
            OnEvent(this, args);
    }
    #endregion
}

/// Класс аргументов для трансляции события интерфейса
class GUIEventArgs : EventArgs {
    /// Тип события
    public GUI.EventTypes EventType { get; set; }
    /// Сопутствующие данные
    public object EventData { get; set; }
}