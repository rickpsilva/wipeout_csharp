namespace WipeoutRewrite
{
    public static class SystemManager
    {
        public static float GetCycleTime() => (float)(System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency);
        // Add system functions as needed
    }
}
