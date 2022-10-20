using System.Buffers;
using System.Diagnostics;
using System.Numerics;

/// <summary>
/// Класс для работы с 13-ричной системой.
/// </summary>
public static class Base13
{
    public static readonly string Alphabet = "0123456789ABC";
    public static readonly string MinBase13 = "0000000000000";
    public static readonly string MaxBase13 = "CCCCCCCCCCCCC";
    
    public static int GetAsDecimal(this char symbol) => GetAsDecimalFast(symbol);
    
    private static int GetAsDecimalSlow(this char symbol) => Alphabet.IndexOf(symbol);

    private static int GetAsDecimalFast(this char symbol)
    {
        // Assumes that symbol is only from the Alphabet
        //0=48-48=0
        //1=49-48=1
        //2=50-48=2
        //3=51-48=3
        //4=52-48=4
        //5=53-48=5
        //6=54-48=6
        //7=55-48=7
        //8=56-48=8
        //9=57-48=9
        //----------
        //A=65-55=10
        //B=66-55=11
        //C=67-55=12

        return symbol < 58 ? symbol - 48 : symbol - 55;
    }

    public static decimal ToBase10(this string base13)
    {
        decimal base10 = 0;
        double pow = 1;
        var length = base13.Length;
        for (int i = 0; i < length; i++)
        {
            int rank = length - 1 - i;
            var symbol = base13[rank];
            int num = symbol.GetAsDecimal();
            if (i > 0)
                pow *= 13;
            base10 += (decimal)(num * pow);
        }

        return base10;
    }

    public static string ToBase13(this decimal base10, int padding = 13) =>
        base10
            .ToBase13(Alphabet)
            .PadLeft(padding, '0');
    
    public static IEnumerable<(char, char, char, char, char, char)> EnumerateDigits6()
    {
        foreach (var symbol6 in Alphabet)
        {
            foreach (var symbol5 in Alphabet)
            {
                foreach (var symbol4 in Alphabet)
                {
                    foreach (var symbol3 in Alphabet)
                    {
                        foreach (var symbol2 in Alphabet)
                        {
                            foreach (var symbol1 in Alphabet)
                            {
                                yield return (symbol6, symbol5, symbol4, symbol3, symbol2, symbol1);
                            }
                        }
                    }
                }
            }
        }
    }
    
    public static int GetSumBase10((char, char, char, char, char, char) base13)
        => base13.Item6.GetAsDecimal() +
           base13.Item5.GetAsDecimal() +
           base13.Item4.GetAsDecimal() +
           base13.Item3.GetAsDecimal() +
           base13.Item2.GetAsDecimal() +
           base13.Item1.GetAsDecimal();
    
    public static string ToBase13(this decimal base10, string alphabet)
    {
        int encodingBase = alphabet.Length;
        int resultMaxLength = 16;
        var outputChars = ArrayPool<char>.Shared.Rent(resultMaxLength);
        int outputIndex = outputChars.Length;

        // To BigInteger
        var bigInt = new BigInteger(base10);
            
        // Encode BigInteger to Base58 string
        while (bigInt > 0 && outputIndex > 0)
        {
            bigInt = BigInteger.DivRem(bigInt, encodingBase, out var remainder);
            outputChars[--outputIndex] = alphabet[(int)remainder];
        }
        
        var encode = new string(outputChars[outputIndex..]);
        ArrayPool<char>.Shared.Return(outputChars);
        return encode;
    }
}

/// <summary>
/// Реализация алгоритмс "Хороших" чисел для Base13.
/// </summary>
public static class Good13
{
    public static long GetBase13GoodNumberCount()
    {
        // 123456 A 123456
        
        long good = Base13
            .EnumerateDigits6()
            .Select(Base13.GetSumBase10)
            .GroupBy(sum => sum)
            .Output(sums => { })
            .Select(sums => (Sum: sums.Key, Count: (long)sums.Count()))
            .Output(sums => { sums.Print("(Sum, Count)"); })
            .Select(sums => (sums.Sum, GoodCount: sums.Count*sums.Count))
            .Output(sums => { sums.Print("(Sum, Count^2)");})
            .Sum(sums => sums.GoodCount);
        
        // Еще в комбинации с 13-ю серединками
        good *= 13;
        
        return good;
    }
    
    public static bool IsGood13(this string base13Number)
    {
        if (base13Number.Length != 13)
            throw new ArgumentException("Хорошие числа вычисляем только для base13 чисел длиной 13");
        
        int sumLeft = 0;
        for (int i = 0; i < 6; i++)
        {
            char symbol = base13Number[i];
            sumLeft += symbol.GetAsDecimal();
        }
        
        int sumRight = 0;
        for (int i = 7; i < 13; i++)
        {
            char symbol = base13Number[i];
            sumRight += symbol.GetAsDecimal();
        }

        return sumLeft == sumRight;
    }
    
    public static decimal GetBase13GoodNumberCountOld(int max = -1)
    {
        Stopwatch stopwatchTotal = Stopwatch.StartNew();
        Stopwatch stopwatch = Stopwatch.StartNew();
        decimal good = 0;

        decimal maxBase13InBase10 = Base13.MaxBase13.ToBase10();
        // Перечисляем все числа от 0 до MaxBase13 в десятичных числах
        for (decimal base10 = 0; base10 < maxBase13InBase10; base10++)
        {
            // Переводим в Base13
            var base13 = base10.ToBase13(padding: 13);
            
            // Проверка на Good13 и инкремент
            if (base13.IsGood13())
            {
                good++;

                if (stopwatch.Elapsed.TotalSeconds > 5)
                {
                    stopwatch = Stopwatch.StartNew();
                    double progress = (double)(base10 / maxBase13InBase10);
                    double progressInPercents = progress * 100;

                    var estimated = new TimeSpan((long)(stopwatchTotal.Elapsed.Ticks / progress));
                    
                    Console.WriteLine($"Good: {good}, Base13: {base13}, Progress: {progressInPercents:00.000000}%, Elapsed: {stopwatchTotal.Elapsed}, Estimated: {estimated}");
                }
                
                if (good == max)
                    break;
            }
        }

        return good;
    }
}

internal static class EnumerableExtensions
{
    /// <summary>
    /// Метериализация последовательности.
    /// Используется для отладки, в релизе отключено. 
    /// </summary>
    public static IEnumerable<T> Output<T>(this IEnumerable<T> source, Action<T[]>? output)
    {
#if DEBUG
        var materialized = source.ToArray();
        output?.Invoke(materialized);
        return materialized;
#endif
        return source;
    }
    
    public static void Print<T>(this IEnumerable<T> values, string header)
    {
        Console.WriteLine(header);
        foreach (var value in values)
        {
            Console.WriteLine(value);
        }
    }
}