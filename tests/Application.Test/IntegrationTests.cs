namespace Application.Test;

using System.Collections.Concurrent;

using Application.Repositories;
using Application.Services;

[Collection("Sequential")]
public class IntegrationTests : IAsyncLifetime
{
  private readonly FacadeService _facadeService;
  private readonly string _testUrl = "https://example.com";
  private readonly ConcurrentBag<string> _createdUrls;

  public IntegrationTests()
  {
    var analyticsEventStore = new VolatileAnalyticsEventStore();
    var analyticsService = new AnalyticsService(analyticsEventStore);

    var keyCodec = new KeyCodec();
    var keyGenerationService = new KeyGenerationService(keyCodec);
    var shortUrlRepository = new ShortUrlRepository();
    var customShortUrlRepository = new CustomShortUrlRepository();
    var shortUrlService = new ShortUrlService(keyGenerationService, shortUrlRepository, customShortUrlRepository);

    _facadeService = new FacadeService(analyticsService, shortUrlService);

    _createdUrls = [];
  }

  public async Task DisposeAsync()
  {
    foreach (var url in _createdUrls)
      await _facadeService.Delete(url);

    _createdUrls.Clear();
  }

  public Task InitializeAsync() => Task.CompletedTask;

  [Fact]
  public async Task Create_WithCustomAlias_ReturnsCustomShortUrl()
  {
    // Arrange
    var customAlias = "test_" + Guid.NewGuid().ToString("N").Substring(0, 6);

    // Act
    var shortUrl = await CreateTestUrl(customAlias);
    var originalUrl = await _facadeService.Get(shortUrl);

    // Assert
    Assert.Equal(customAlias, shortUrl);
    Assert.Equal(_testUrl, originalUrl);
  }

  [Fact]
  public async Task Delete_ExistingUrl_RemovesUrlButAnalytics()
  {
    // Arrange
    var shortUrl = await CreateTestUrl();
    await _facadeService.Get(shortUrl); // Record a hit

    // Act
    var deleteResult = await _facadeService.Delete(shortUrl);
    var retrievedUrl = await _facadeService.Get(shortUrl);

    await Task.Delay(250); // Add delay to ensure hit was recorded
    var hitCount = _facadeService.Analytics(shortUrl);

    // Assert
    Assert.True(deleteResult);
    Assert.Empty(retrievedUrl);
    Assert.Equal(1, hitCount);
  }

  [Fact]
  public async Task Analytics_TracksMultipleHits()
  {
    // Arrange
    var shortUrl = await CreateTestUrl();

    // Act
    for (int i = 0; i < 3; i++)
      await _facadeService.Get(shortUrl);

    await Task.Delay(250); // Add delay to ensure hits were recorded

    var hitCount = _facadeService.Analytics(shortUrl);

    // Assert
    Assert.Equal(3, hitCount);
  }

  [Fact]
  public async Task Get_NonexistentUrl_ReturnsEmptyString()
  {
    // Arrange
    var nonexistentUrl = "nonexistent_" + Guid.NewGuid().ToString("N").Substring(0, 6);

    // Act
    var result = await _facadeService.Get(nonexistentUrl);

    // Assert
    Assert.Empty(result);
    Assert.Equal(0, _facadeService.Analytics(nonexistentUrl));
  }

  [Fact]
  public async Task Create_SameUrlMultipleTimes_GeneratesUniqueShortUrls()
  {
    // Act
    var firstShortUrl = await CreateTestUrl();
    var secondShortUrl = await CreateTestUrl();

    // Assert
    Assert.NotEqual(firstShortUrl, secondShortUrl);
    Assert.Equal(_testUrl, await _facadeService.Get(firstShortUrl));
    Assert.Equal(_testUrl, await _facadeService.Get(secondShortUrl));
  }

  [Fact]
  public async Task Get_MultipleCalls_IncrementHitCountCorrectly()
  {
    // Arrange
    var shortUrl = await CreateTestUrl();

    // Act & Assert
    for (int i = 1; i <= 5; i++)
    {
      await _facadeService.Get(shortUrl);

      await Task.Delay(250); // Add delay to ensure hit was recorded

      var hitCount = _facadeService.Analytics(shortUrl);

      Assert.Equal(i, hitCount);
    }
  }

  private async Task<string> CreateTestUrl(string? customAlias = null)
  {
    var shortUrl = await _facadeService.Create(_testUrl, customAlias);
    _createdUrls.Add(shortUrl);
    return shortUrl;
  }
}