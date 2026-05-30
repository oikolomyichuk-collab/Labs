using Domain;

namespace TelegramBot
{
    public class EntityTextServant
    {
        public string FormatDetail(Detail detail)
        {
            return $"ID: {detail.DetailCode}\n" +
                   $"Назва деталі: {detail.DetailName}\n" +
                   $"Децимальний номер: {detail.DecimalNumber}\n" +
                   $"Марка сплаву: {detail.AlloyGrade}\n" +
                   $"Маса, кг: {detail.Mass}\n";
        }

        public string FormatOperation(Operation operation)
        {
            return $"ID: {operation.OperationCode}\n" +
                   $"Номер цеху: {operation.WorkshopNumber}\n" +
                   $"Тривалість операції, годин: {operation.DurationHours}\n" +
                   $"Вартість виконання операції, грн: {operation.Cost}\n";
        }

        public string FormatProduction(Production production)
        {
            return $"ID деталі: {production.DetailCode}\n" +
                   $"Номер операції в технологічному процесі: {production.OperationNumberInProcess}\n" +
                   $"ID операції: {production.OperationCode}\n";
        }
    }
}