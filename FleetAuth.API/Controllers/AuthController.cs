using FleetAuth.Core.DTOs;
using FleetAuth.Core.Entities;
using FleetAuth.Infrastructure.Data;
using FleetAuth.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetAuth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _db;

    public AuthController(UserManager<AppUser> userManager, TokenService tokenService, AppDbContext db)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var validRoles = new[] { "Admin", "FleetManager", "Driver" };
        if (!validRoles.Contains(dto.Role))
            return BadRequest("Geçersiz rol. Geçerli roller: Admin, FleetManager, Driver");

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest("Bu email zaten kayıtlı.");

        var user = new AppUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, dto.Role);
        return Ok(new { message = "Kayıt başarılı.", userId = user.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Email veya şifre hatalı.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

        return Ok(new AuthResponseDto(accessToken, refreshToken.Token));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshDto dto)
    {
        var token = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == dto.RefreshToken);

        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Geçersiz veya süresi dolmuş refresh token.");

        token.IsRevoked = true;
        token.RevokedReason = "Rotated";

        var roles = await _userManager.GetRolesAsync(token.User);
        var newAccessToken = _tokenService.GenerateAccessToken(token.User, roles);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(token.User.Id);

        await _db.SaveChangesAsync();
        return Ok(new AuthResponseDto(newAccessToken, newRefreshToken.Token));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(RefreshDto dto)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == dto.RefreshToken);

        if (token != null)
        {
            token.IsRevoked = true;
            token.RevokedReason = "Logout";
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Çıkış yapıldı." });
    }
}