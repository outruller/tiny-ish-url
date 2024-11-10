using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;

using Application.Repositories;
using Application.Services;

namespace Application.Test;

[Collection("Sequential")]
public class ConcurrentTests
{
  private static readonly Random _random = new();

  private readonly int _keyLength;
  private readonly FacadeService _api;

  public ConcurrentTests()
  {
    var analyticsEventStore = new VolatileAnalyticsEventStore();
    var analyticsService = new AnalyticsService(analyticsEventStore);

    var keyCodec = new KeyCodec();
    var keyGenerationService = new KeyGenerationService(keyCodec);
    var shortUrlRepository = new ShortUrlRepository();
    var customShortUrlRepository = new CustomShortUrlRepository();
    var shortUrlService = new ShortUrlService(keyGenerationService, shortUrlRepository, customShortUrlRepository);

    _api = new FacadeService(analyticsService, shortUrlService);
    _keyLength = keyCodec.KeyLength;
  }

  [Fact]
  public async Task ConcurrentCreateAndGets_ShouldBeConsistent()
  {
    // Arrange
    int numberOfTasks = 64;
    var numberOfRecordsPerTask = 10_000;

    var originalUrls = new ConcurrentBag<string>();
    var shortOriginalPairs = new ConcurrentDictionary<string, string>();

    var customShortUrls = GenerateCustomShortUrls(numberOfTasks).ToImmutableArray();

    // Act
    var tasks = new List<Task>();
    for (int i = 0; i < numberOfTasks; i++)
    {
      var taskNum = i;
      tasks.Add(Task.Run(async () =>
      {
        for (int j = 0; j < numberOfRecordsPerTask; j++)
        {
          var originalUrl = $"https://tinyishurl.com/my_long_url_{taskNum}";
          var shortUrl = await _api.Create(originalUrl);

          originalUrls.Add(originalUrl);
          shortOriginalPairs.GetOrAdd(shortUrl, originalUrl);
        }

        var customOriginalUrl = $"https://tinyishurl.com/my_custom_long_url_{taskNum}";
        var customShortUrl = await _api.Create(customOriginalUrl, customShortUrls[taskNum]);

        originalUrls.Add(customOriginalUrl);
        shortOriginalPairs.GetOrAdd(customShortUrl, customOriginalUrl);
      }));
    }

    await Task.WhenAll(tasks);

    // Assert
    Assert.Equal(numberOfTasks + numberOfTasks * numberOfRecordsPerTask, shortOriginalPairs.Count);

    // Now, retrieve all keys concurrently
    var retrievalTasks = new List<Task>();
    foreach (var (shortUrl, expectedOriginalUrl) in shortOriginalPairs)
    {
      retrievalTasks.Add(Task.Run(async () =>
      {
        string original = await _api.Get(shortUrl);

        Assert.True(original == expectedOriginalUrl, $"Get for {shortUrl} should've return {expectedOriginalUrl}, but returned {original}.");
      }));
    }

    await Task.WhenAll(retrievalTasks);
  }

  [Fact]
  public async Task ConcurrentGets_ShouldUpdateAnalytics()
  {
    // Arrange
    var numberOfRecords = 8000;
    var numberOfCustomRecords = 2000;
    int numberOfTasks = 64;
    int hitsPerTask = 1_000;

    var shortUrls = new List<string>();
    var customShortUrls = GenerateCustomShortUrls(numberOfCustomRecords).ToImmutableArray();
    var shortOriginalPairs = new Dictionary<string, string>();
    var hitCounter = new ConcurrentDictionary<string, int>();

    for (int i = 0; i < numberOfRecords; i++)
    {
      var originalUrl = $"https://tinyishurl.com/my_long_url_{i}";
      var shortUrl = await _api.Create(originalUrl);

      shortUrls.Add(shortUrl);
      shortOriginalPairs[shortUrl] = originalUrl;
    }

    for (int i = 0; i < numberOfCustomRecords; i++)
    {
      var customOriginalUrl = $"https://tinyishurl.com/my_custom_long_url_{i}";
      var customShortUrl = await _api.Create(customOriginalUrl, customShortUrls[i]);

      shortUrls.Add(customShortUrl);
      shortOriginalPairs[customShortUrl] = customOriginalUrl;
    }

    // Act
    var tasks = new List<Task>();
    for (int i = 0; i < numberOfTasks; i++)
    {
      int taskNum = i;
      tasks.Add(Task.Run(async () =>
      {
        for (int j = 0; j < hitsPerTask; j++)
        {
          var shortUrl = shortUrls[_random.Next(shortUrls.Count)];

          hitCounter.AddOrUpdate(shortUrl, (k) => 1, (k, v) => v += 1);

          var longUrl = await _api.Get(shortUrl);
        }
      }));
    }

    await Task.WhenAll(tasks);

    // Assert
    Assert.Equal(numberOfTasks * hitsPerTask, hitCounter.Values.Sum());

    // Now, retrieve all hit counts concurrently
    var retrievalTasks = new List<Task>();
    foreach (var (shortUrl, originalUrl) in shortOriginalPairs)
    {
      retrievalTasks.Add(Task.Run(() =>
      {
        hitCounter.TryGetValue(shortUrl, out int expectedCount); // 0 by default

        var count = _api.Analytics(shortUrl);

        Assert.True(count == expectedCount, $"Analytics for {shortUrl} should've return {expectedCount}, but returned {count}.");
      }));
    }

    await Task.WhenAll(retrievalTasks);
  }


  private IEnumerable<string> GenerateCustomShortUrls(
    int count = 100,
    int minLength = 2,
    int maxLength = 16)
  {
    var uniqueCustomeUrls = new HashSet<string>();
    while (uniqueCustomeUrls.Count < count)
      uniqueCustomeUrls.Add(GenerateRandomString(_keyLength));
    return uniqueCustomeUrls;
  }

  private string GenerateRandomString(
    int generatedKeyLength,
    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
    int minLength = 2,
    int maxLength = 16)
  {
    var length = _random.Next(minLength, maxLength + 1);
    if (length == generatedKeyLength)
      length++;

    StringBuilder result = new(length);
    for (int i = 0; i < length; i++)
      result.Append(alphabet[_random.Next(alphabet.Length)]);

    return result.ToString();
  }
}
