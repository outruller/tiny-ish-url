using Application.Model;

namespace Application.Repositories;

public sealed class ShortUrlRepository : VolatileRepository<ShortUrl, string>;
