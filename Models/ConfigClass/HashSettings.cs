namespace CRMService.Models.ConfigClass
{
    public class HashSettings
    {
        public const string HASH_CONFIGURE = "HashConfigure";
        public int SaltSize { get; set; }
        public int KeySize { get; set; }
        public int Iterations {  get; set; }
        public string Separator { get; set; } = string.Empty;
        public string Algorithm {  get; set; } = string.Empty;
    }
}
