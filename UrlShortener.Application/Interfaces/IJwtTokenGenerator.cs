using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}