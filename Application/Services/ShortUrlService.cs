namespace Application.Services;

using Application.Model;
using Application.Repositories;

public class ShortUrlService(
  KeyGenerationService _keyGenerationService,
  ShortUrlRepository _shortUrlRepository,
  CustomShortUrlRepository _customShortUrlRepository
  )
{
  public async Task<string> Get(string shortUrl)
  {
    // Use bloom filter?

    if (shortUrl.Length == _keyGenerationService.KeyLength) // Yep, no 7-char custom short URLs in this POC :)
    {
      var shortUrlEntity = await _shortUrlRepository.GetByIdAsync(shortUrl);

      if (shortUrlEntity == null)
        return "";

      return shortUrlEntity.OriginalUrl;
    }

    var customShortUrlEntity = await _customShortUrlRepository.GetByIdAsync(shortUrl);

    if (customShortUrlEntity == null)
      return "";

    return customShortUrlEntity.OriginalUrl;

  }

  public async Task<string> Create(string originalUrl, string? alias = null)
  {
    if (alias?.Length == _keyGenerationService.KeyLength)
      throw new ArgumentException($"Yep, no {_keyGenerationService.KeyLength}-char custom short URLs in this POC :)");

    if (!string.IsNullOrWhiteSpace(alias))
    {
      await _customShortUrlRepository.AddAsync(new ShortUrl
      {
        Id = alias,
        OriginalUrl = originalUrl,
        Created = DateTime.UtcNow
      });

      return alias;
    }

    var key = _keyGenerationService.GetNextId();
    await _shortUrlRepository.AddAsync(
      new ShortUrl
      {
        Id = key,
        OriginalUrl = originalUrl,
        Created = DateTime.UtcNow
      }
    );

    return key;
  }

  public async Task<bool> Delete(string shortUrl)
  {
    // Right now we actually delete them, but need to consider marking them as deleted and never reuse.

    if (shortUrl.Length == _keyGenerationService.KeyLength) // Yep, no 7-char custom short URLs in this POC :)
    {
      var generatedShortUrlDeleted = await _shortUrlRepository.DeleteAsync(shortUrl);
      return generatedShortUrlDeleted;
    }

    var customShortUrlDeleted = await _customShortUrlRepository.DeleteAsync(shortUrl);
    return customShortUrlDeleted;
  }
}