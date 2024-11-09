namespace Application.Services;

using Application.Model;
using Application.Repositories;

public class AnalyticsService(
  AnalyticsRepository _analyticsRepository
)
{
  public async Task<int> Count(string key)
  {
    var entity = await _analyticsRepository.GetByIdAsync(key);
    if (entity != null)
      return entity.HitCount;
    return 0;
  }

  public async void Hit(string key)
  {
    var entity = await _analyticsRepository.GetByIdAsync(key);
      if (entity != null) {
        entity.HitCount += 1;
        await _analyticsRepository.UpdateAsync(entity);
      } else {
        entity = new Analytics { Id = key, HitCount = 1};
        await _analyticsRepository.AddAsync(entity);
      }
    // Task.Factory.StartNew(async () => {
    //   var entity = await _analyticsRepository.GetByIdAsync(key);
    //   if (entity != null) {
    //     entity.HitCount += 1;
    //     await _analyticsRepository.UpdateAsync(entity);
    //   } else {
    //     entity = new Analytics { Id = key, HitCount = 1};
    //     await _analyticsRepository.AddAsync(entity);
    //   }
    // });
  }
}