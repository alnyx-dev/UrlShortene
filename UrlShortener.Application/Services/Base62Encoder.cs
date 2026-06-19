namespace UrlShortener.Application.Services;

public static class Base62Encoder
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string Encode(long value)
    {
        if (value == 0) return Alphabet[0].ToString();

        var sb = new System.Text.StringBuilder();
        while (value > 0)
        {
            sb.Insert(0, Alphabet[(int)(value % 62)]);
            value /= 62;
        }
        return sb.ToString();
    }
}