namespace Application.Model;

using Application.Repositories;

public sealed class Analytics : IEntity<string>
{
    public required string Id { get ; set; }

    public int HitCount { get; set; } = 0;
}