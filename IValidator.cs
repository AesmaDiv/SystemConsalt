/*
    Ander Ingvarsson 2021
    
    Интерфейс валидатора
    Проверка объекта соответствию коллекции по каким-либо критериям
*/
interface IValidator {
    /// Основная функция валидатора
    bool Validate(object item, object[] collection);
}