using Freeqy_APIs.Entities;

namespace Freeqy_APIs.Repositories;

/// <summary>
/// Mock implementation of IPasswordResetTokenRepository for development.
/// Replace this with actual database implementation later.
/// </summary>
public class MockPasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly List<PasswordResetToken> _tokens = new();

    public Task<PasswordResetToken> CreateTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        token.Id = _tokens.Count + 1;
        _tokens.Add(token);
        return Task.FromResult(token);
    }

    public Task<PasswordResetToken?> GetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var resetToken = _tokens.FirstOrDefault(t => t.Token == token);
        return Task.FromResult(resetToken);
    }

    public Task MarkTokenAsUsedAsync(string token, CancellationToken cancellationToken = default)
    {
        var resetToken = _tokens.FirstOrDefault(t => t.Token == token);
        if (resetToken != null)
        {
            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        _tokens.RemoveAll(t => t.ExpiresAt < DateTime.UtcNow);
        return Task.CompletedTask;
    }
}
