using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Common;
namespace Tourism.Status{
    public enum StatusEnum{
        Saved,
        Posted,
        InReview,
        Accepted,
        Cancelled
    }

    public static class StatusHelper
    {
        static readonly Dictionary<StatusEnum, string> _EnumToString = new Dictionary<StatusEnum, string>{
            { StatusEnum.Saved, "Збережено"},
            { StatusEnum.Posted, "Відправлено"}, 
            { StatusEnum.InReview, "Опрацьовується"},
            { StatusEnum.Accepted, "Прийнято"},
            { StatusEnum.Cancelled, "Скасовано"},
        };
        static readonly Dictionary<string, StatusEnum> _StringToEnum = new Dictionary<string, StatusEnum>{
            { "Збережено", StatusEnum.Saved},
            { "Відправлено", StatusEnum.Posted}, 
            { "Опрацьовується", StatusEnum.InReview},
            { "Прийнято", StatusEnum.Accepted},
            { "Скасовано", StatusEnum.Cancelled},
        };

        public static string GetStatus(StatusEnum status)
        {
            if(_EnumToString.ContainsKey(status))
            {
                return _EnumToString[status];
            } else
            {
                throw new ArgumentException($"Enum {status} is not in allowed interval");
            }
        }

        public static StatusEnum GetStatus(string status)
        {
            if(_StringToEnum.ContainsKey(status))
            {
                return _StringToEnum[status];
            } else
            {
                throw new ArgumentException($"status {status} is not in allowed interval");
            }
        }
    }
}
