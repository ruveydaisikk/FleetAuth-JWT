namespace FleetAuth.Core.DTOs;

public record RegisterDto(string FullName, string Email, string Password, string Role);
public record LoginDto(string Email, string Password);
public record RefreshDto(string RefreshToken);
public record AuthResponseDto(string AccessToken, string RefreshToken);