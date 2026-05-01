# FleetAuth — JWT Auth API

ASP.NET Core 9 ile geliştirilmiş, çok rol destekli JWT kimlik doğrulama sistemi.

## Teknolojiler
- ASP.NET Core 9
- EF Core + PostgreSQL
- ASP.NET Core Identity
- JWT Bearer Authentication
- AspNetCoreRateLimit

## Kurulum

### Gereksinimler
- .NET 9 SDK
- PostgreSQL 16

### Çalıştırma
```bash
git clone <repo-url>
cd FleetAuth

# appsettings.json içindeki bağlantı bilgilerini düzenle
# ConnectionStrings > Default

dotnet ef database update --project FleetAuth.Infrastructure --startup-project FleetAuth.API
dotnet run --project FleetAuth.API
```

Swagger: `http://localhost:5123/swagger`

## Endpoint'ler

| Method | Endpoint | Açıklama | Auth |
|--------|----------|----------|------|
| POST | /api/auth/register | Kayıt | — |
| POST | /api/auth/login | Giriş | — |
| POST | /api/auth/refresh | Token yenile | — |
| POST | /api/auth/logout | Çıkış | ✅ |
| GET | /api/test/admin | Admin paneli | Admin |
| GET | /api/test/manager | Yönetici paneli | Admin, FleetManager |
| GET | /api/test/driver | Sürücü paneli | Admin, FleetManager, Driver |

## Roller
- **Admin** — Tüm endpoint'lere erişim
- **FleetManager** — Manager ve Driver endpoint'leri
- **Driver** — Sadece Driver endpoint'i

## Özellikler
- ✅ JWT access token (15 dk) + refresh token (7 gün)
- ✅ Refresh token DB'de saklanır ve geçersizleştirilebilir
- ✅ Token rotasyonu (refresh sonrası eski token iptal)
- ✅ IP bazlı rate limiting (login: 10 dakikada 5 istek)
- ✅ Audit log (kullanıcı, endpoint, zaman damgası)
- ✅ 401 token yok/geçersiz, 403 yetersiz rol