namespace Application.Services;

public sealed class KeyGenerationService(
  KeyCodec _keyCodec,
  long offset = 0
  )
{
  private long _currentId = offset;

  public int KeyLength => _keyCodec.KeyLength;

  public string GetNextId() => _keyCodec.Encode(Interlocked.Increment(ref _currentId));
}
