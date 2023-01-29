namespace TodoApp.Bot;

internal static class ArrayExtensions
{
    internal static void Deconstruct<T>(this T[] src, out T first, out T second)
    {
        first = src[0];
        second = src[1];
    }
}