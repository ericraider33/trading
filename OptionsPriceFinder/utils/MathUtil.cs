namespace OptionsPriceFinder.utils;

public static class MathUtil
{
    public static decimal average(params decimal[] numbers)
    {
        if (numbers.Length == 0)
            return 0m;

        decimal sum = 0m;
        foreach (decimal num in numbers)
            sum += num;

        // Return the sum divided by the number of elements
        return sum / numbers.Length;
    }
}