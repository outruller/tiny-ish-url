namespace Application.Model;

using Application.Repositories;

public sealed class ShortUrl : IEntity<string>
{
    public required string Id { get ; set; }

    public required string OriginalUrl { get; set; }

    public required DateTime Created { get; set; }
}