using System.Collections.Concurrent;

using Application.Repositories;
using Application.Services;

namespace Application.Test;

public class KeyGenerationServiceTests
{
  [Fact]
  public async Task ConcurrentAddAndRetrieveKeys_ShouldMaintainConsistency()
  {
    // Arrange
    var analyticsRepository = new AnalyticsRepository();
    var analyticsService = new AnalyticsService(analyticsRepository);

    var keyCodec = new KeyCodec();
    var keyGenerationService = new KeyGenerationService(keyCodec);
    var shortUrlRepository = new ShortUrlRepository();
    var shortUrlService = new ShortUrlService(keyGenerationService, shortUrlRepository);

    var app = new FacadeService(analyticsService, shortUrlService);

    // var kgs = new KeyGenerationService();
    int numberOfTasks = 1000;
    int keysPerTask = 10_000;
    var tasks = new List<Task>();
    var originalUrls = new ConcurrentBag<string>();
    var shortOriginalPairs = new ConcurrentDictionary<string, string>();

    // Act
    for (int i = 0; i < numberOfTasks; i++)
    {
      int taskNum = i;
      tasks.Add(Task.Run(async () =>
      {
        for (int j = 0; j < keysPerTask; j++)
        {
          string original = $"Task{taskNum}_Key{j}";
          originalUrls.Add(original);

          var shortUrl = await app.Create(original);
          shortOriginalPairs[shortUrl] = original;
        }
      }));
    }

    await Task.WhenAll(tasks);

    // Assert
    Assert.Equal(numberOfTasks * keysPerTask, shortOriginalPairs.Count);

    // Now, retrieve all keys concurrently
    var retrievalTasks = new List<Task>();
    foreach (var kv in shortOriginalPairs)
    {
      retrievalTasks.Add(Task.Run(async () =>
      {
        string original = await app.Get(kv.Key);

        Assert.True(!string.IsNullOrEmpty(original), $"Key {kv.Key} should've return {kv.Value}.");
      }));
    }

    await Task.WhenAll(retrievalTasks);
  }

  [Fact]
  public async Task ConcurrentGets_ShouldUpdateAnalytics()
  {
    // Arrange
    var analyticsRepository = new AnalyticsRepository();
    var analyticsService = new AnalyticsService(analyticsRepository);

    var keyCodec = new KeyCodec();
    var keyGenerationService = new KeyGenerationService(keyCodec);
    var shortUrlRepository = new ShortUrlRepository();
    var shortUrlService = new ShortUrlService(keyGenerationService, shortUrlRepository);

    var app = new FacadeService(analyticsService, shortUrlService);

    var numberOfRecords = 1_000_000;
    var shortUrls = new List<string>();
    var shortOriginalPairs = new ConcurrentDictionary<string, string>();
    var hitCounter = new ConcurrentDictionary<string, int>();
    for (int i = 0; i < numberOfRecords; i++)
    {
      var originalUrl = $"https://tinyishurl.com/my_long_url_{i}";
      var shortUrl = await app.Create(originalUrl);
      shortUrls.Add(shortUrl);
      hitCounter[shortUrl] = 0;
      shortOriginalPairs[shortUrl] = originalUrl;
    }

    int numberOfTasks = 1000;
    int hitsPerTask = 10_000;
    var tasks = new List<Task>();

    // Act
    Random rand = new();
    for (int i = 0; i < numberOfTasks; i++)
    {
      int taskNum = i;
      tasks.Add(Task.Run(async () =>
      {
        for (int j = 0; j < hitsPerTask; j++)
        {
          var shortUrl = shortUrls[rand.Next(hitCounter.Count)];
          hitCounter[shortUrl] += 1;
          await app.Get(shortUrl);
        }
      }));
    }

    await Task.WhenAll(tasks);

    // Assert
    Assert.Equal(numberOfTasks * hitsPerTask, hitCounter.Values.Sum());

    // Now, retrieve all keys concurrently
    var retrievalTasks = new List<Task>();
    foreach (var kv in shortOriginalPairs)
    {
      retrievalTasks.Add(Task.Run(async () =>
      {
        var count = await app.Analytics(kv.Key);

        Assert.True(count == hitCounter[kv.Key], $"Analytics {kv.Key} should've return {hitCounter[kv.Key]} but returned {count}.");
      }));
    }

    await Task.WhenAll(retrievalTasks);
  }

}