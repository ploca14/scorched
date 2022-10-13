namespace Polyperfect.Common
{
    public static class StringExtensions
    {
        public static string NewlineSpaces(this string that)
        {
            if (string.IsNullOrEmpty(that))
                that = "";
            return that.Replace(' ', '\n');
        }
    }
}