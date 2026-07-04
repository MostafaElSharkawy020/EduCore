namespace EduCore.Helpers
{
    public static class PlatformSettings
    {
        // Revenue split: the platform keeps this fraction, the teacher earns the rest.
        // 0.20 => platform 20%, teacher 80%. Change this one value to adjust the split.
        public const decimal PlatformFeeRate = 0.20m;

        public static decimal TeacherShareRate => 1m - PlatformFeeRate;

        public static int PlatformPercent => (int)(PlatformFeeRate * 100);
        public static int TeacherPercent => (int)(TeacherShareRate * 100);
    }
}
