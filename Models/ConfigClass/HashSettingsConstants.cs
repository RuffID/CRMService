namespace CRMService.Models.ConfigClass
{
    public static class HashSettingsConstants
    {
        public const int SALT_SIZE = 16;
        public const int KEY_SIZE = 32;
        public const int ITERATIONS = 10000;
        public const char SEPARATOR = ':';
        public const string ALHORITHM = "SHA256";
    }
}
