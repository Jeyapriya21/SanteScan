namespace santeScan.Server.Services.Interfaces;

public interface ISessionService
{
    Task<Guid> GetOrCreateGuestUserAsync(string sessionId);
    Task<bool> MigrateGuestAnalysesToUserAsync(string sessionId, Guid permanentUserId);
    Task<int> GetGuestAnalysisCountAsync(string sessionId);
}