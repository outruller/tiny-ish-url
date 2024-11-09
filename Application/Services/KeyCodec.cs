namespace Application.Services;

using Codec = Base62.EncodingExtensions;

public sealed class KeyCodec(int length = 7)
{
  public string Encode(long key)
  {
    if (key <= 0)
      throw new ArgumentException("Key should be > 0!");
    
    return Codec.ToBase62(key).PadLeft(length, '0');
  }

  public long Decode(string key)
  {
    if (key.Length != length)
      throw new ArgumentException("Wrong key length!");

    return Codec.FromBase62<long>(key);
  }
}