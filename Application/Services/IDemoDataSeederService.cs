namespace Application.Services
{
    public interface IDemoDataSeederService
    {
        Task EnsureSeededAsync(bool force = false, CancellationToken cancellationToken = default);
    }
}
