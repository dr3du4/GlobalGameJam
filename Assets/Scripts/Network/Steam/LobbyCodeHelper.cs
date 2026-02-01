using System;

/// <summary>
/// Konwertuje Steam Lobby ID na krótki, czytelny kod i z powrotem.
/// Steam Lobby ID to ulong (64-bit), więc potrzebujemy odpowiednio długiego kodu.
/// </summary>
public static class LobbyCodeHelper
{
    // Znaki używane w kodzie (bez mylących: 0/O, 1/I/L)
    private const string CHARS = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int CODE_LENGTH = 12; // 12 znaków = 30^12 > 2^64

    /// <summary>
    /// Konwertuje Lobby ID na kod (np. "ABCD1234EFGH")
    /// </summary>
    public static string LobbyIdToCode(ulong lobbyId)
    {
        char[] code = new char[CODE_LENGTH];
        ulong remaining = lobbyId;

        for (int i = CODE_LENGTH - 1; i >= 0; i--)
        {
            code[i] = CHARS[(int)(remaining % (ulong)CHARS.Length)];
            remaining /= (ulong)CHARS.Length;
        }

        return new string(code);
    }

    /// <summary>
    /// Konwertuje kod z powrotem na Lobby ID
    /// </summary>
    public static ulong CodeToLobbyId(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException("Kod nie może być pusty!");
        }

        // Obsłuż stare 6-znakowe kody (dla kompatybilności)
        if (code.Length == 6)
        {
            return DecodeShortCode(code);
        }

        if (code.Length != CODE_LENGTH)
        {
            throw new ArgumentException($"Kod musi mieć {CODE_LENGTH} znaków! (otrzymano: {code.Length})");
        }

        code = code.ToUpperInvariant();
        ulong result = 0;

        for (int i = 0; i < CODE_LENGTH; i++)
        {
            int index = CHARS.IndexOf(code[i]);
            if (index < 0)
            {
                throw new ArgumentException($"Nieprawidłowy znak w kodzie: {code[i]}");
            }
            result = result * (ulong)CHARS.Length + (ulong)index;
        }

        return result;
    }

    /// <summary>
    /// Dekoduj stary 6-znakowy kod (dla kompatybilności wstecznej)
    /// </summary>
    private static ulong DecodeShortCode(string code)
    {
        code = code.ToUpperInvariant();
        ulong result = 0;

        for (int i = 0; i < 6; i++)
        {
            int index = CHARS.IndexOf(code[i]);
            if (index < 0)
            {
                throw new ArgumentException($"Nieprawidłowy znak w kodzie: {code[i]}");
            }
            result = result * (ulong)CHARS.Length + (ulong)index;
        }

        return result;
    }

    /// <summary>
    /// Sprawdza czy kod ma poprawny format
    /// </summary>
    public static bool IsValidCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        // Akceptuj 6 lub 12 znaków
        if (code.Length != 6 && code.Length != CODE_LENGTH)
            return false;

        code = code.ToUpperInvariant();
        foreach (char c in code)
        {
            if (CHARS.IndexOf(c) < 0)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Pobierz długość kodu
    /// </summary>
    public static int GetCodeLength() => CODE_LENGTH;
}
