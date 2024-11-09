namespace Application.Services;

public sealed class KeyGenerationService(
  KeyCodec _keyCodec,
  long offset = 0
  )
{
  private long _currentId = offset;

  public string GetNextId() => _keyCodec.Encode(Interlocked.Increment(ref _currentId));

  private long GetCurrentId() => Interlocked.Read(ref _currentId);

  private void Reset(long offset = 0) => Interlocked.Exchange(ref _currentId, offset);
}
