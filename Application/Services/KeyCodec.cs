namespace Application.Services;

using Codec = Base62.EncodingExtensions;

public sealed class KeyCodec(int lenght = 7)
{
  public int KeyLength { get; } = lenght;

  public int MaxNumber { get; } = (lenght ^ 62) - 1;

  public string Encode(long key)
  {
    if (key <= 0)
      throw new ArgumentException("Key should be > 0!");

    return EnsureLength(Codec.ToBase62(key));
  }

  public long Decode(string key)
  {
    if (key.Length != KeyLength)
      throw new ArgumentException("Wrong key length!");

    return Codec.FromBase62<long>(key);
  }

  private string EnsureLength(string encodedKey)
  {
    if (encodedKey.Length == KeyLength)
      return encodedKey;

    return (encodedKey.Length > KeyLength)
            ? encodedKey[^KeyLength..]
            : encodedKey.PadLeft(KeyLength, '0');
  }
}