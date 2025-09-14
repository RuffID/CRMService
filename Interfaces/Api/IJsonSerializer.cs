namespace CRMService.Interfaces.Api
{
    public interface IJsonSerializer
    {
        string MediaType { get; }
        string Serialize(object value);
        T? Deserialize<T>(Stream stream, CancellationToken ct);
    }
}