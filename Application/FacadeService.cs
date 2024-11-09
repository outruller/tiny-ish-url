using Application.Services;

namespace Application;

public class FacadeService(
  AnalyticsService _analyticsService,
  ShortUrlService _shortUrlService
)
{
  public async Task<string> Get(string key)
  {
    var originalUrl = await _shortUrlService.Get(key);
    if (string.IsNullOrEmpty(originalUrl))
      return "";

    _analyticsService.Hit(key);
    return originalUrl;
  }

  public async Task<string> Create(string originalUrl, string? alias = null)
  {
    return await _shortUrlService.Create(originalUrl);
  }

  public async Task<bool> Delete(string shortUrl)
  {
    return await _shortUrlService.Delete(shortUrl);
  }

  public async Task<int> Analytics(string shortUrl)
  {
    return await _analyticsService.Count(shortUrl);
  }
}