using TVProgram.Domain;
using System;
using System.Collections.Generic;

namespace TVProgram.Domain;

/// <summary>
/// Представляє окрему телевізійну передачу.
/// </summary>
public record TVShow(
    Guid Id,
    string Title,
    DateTime StartTime,
    TimeSpan Duration,
    Genre Genre) : IComparable<TVShow>
{
    /// <summary>
    /// Обчислювана властивість часу закінчення передачі.
    /// </summary>
    public DateTime EndTime => StartTime + Duration;

    /// <summary>
    /// Реалізація інтерфейсу для сортування передач за часом початку.
    /// </summary>
    public int CompareTo(TVShow? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return StartTime.CompareTo(other.StartTime);
    }
}