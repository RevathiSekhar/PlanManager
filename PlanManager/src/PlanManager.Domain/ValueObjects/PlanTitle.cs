namespace PlanManager.Domain.ValueObjects;

public sealed class PlanTitle : IEquatable<PlanTitle>
{
    public string Value { get; }

    private PlanTitle(string value) => Value = value;

    public static PlanTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Title cannot be empty.", nameof(value));
        if (value.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(value));

        return new PlanTitle(value.Trim());
    }

    public bool Equals(PlanTitle? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PlanTitle other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
    public static implicit operator string(PlanTitle title) => title.Value;
}
