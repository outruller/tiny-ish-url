namespace Application.Services;

using Application.Model;
using Application.Repositories;

public sealed class ShortUrlService(
  KeyGenerationService _keyGenerationService,
  ShortUrlRepository _shortUrlRepository
  )
{
  public async Task<string> Get(string shortUrl)
  {
    // bloom filter?
    var entity = await _shortUrlRepository.GetByIdAsync(shortUrl);
    if (entity == null)
      return "";

    // track retreivals
    return entity.OriginalUrl;
  }

  public async Task<string> Create(string originalUrl, string? alias = null)
  {
    // bloom filter to check if generated/alias shortUrl exists
    // get unique ID multi-threaded
    // use Base62 to get generated short Url
    // generate short url

    var key = _keyGenerationService.GetNextId();
    await _shortUrlRepository.AddAsync(
      new ShortUrl {
        Id = key,
        OriginalUrl = originalUrl,
        Timestamp = DateTime.UtcNow
      }
    );

    return key;
  }

  public async Task<bool> Delete(string key)
  {
    return await _shortUrlRepository.DeleteAsync(key);
  }
}