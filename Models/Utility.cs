namespace Dashboard.Models;

public static class Utility
{
    public static DateTime FromHtmlDate(string value)
    {
        try
        {
            string[] buffer = value.Split('-');
            int y = int.Parse(buffer[0]);
            int m = int.Parse(buffer[1]);
            int d = int.Parse(buffer[2]);
            return new DateTime(y, m, d);
        }
        catch
        {
            return DateTime.Today;
        }
    }

    public static string ToHtmlDate(DateTime date)
    {
        return $"{date.Year:0000}-{date.Month:00}-{date.Day:00}";
    }
}
