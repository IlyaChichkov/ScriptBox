public static class SBDebugger
{
    public static void Log<T>(string header, T val)
    {
        UnityEngine.Debug.Log($"> {header}: " + val);
    }
    public static void Log<T>(T val)
    {
        UnityEngine.Debug.Log("> " + val);
    }
    public static void Split()
    {
        UnityEngine.Debug.Log("<---------------->");
    }

    public static void Warning<T>(T val)
    {
        UnityEngine.Debug.LogWarning("> " + val);
    }
}