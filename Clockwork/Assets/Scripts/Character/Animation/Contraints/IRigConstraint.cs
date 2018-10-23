public interface IRigConstraint
{
    /// <summary>
    /// The update order for the constraint. Lower values are evaluated first.
    /// </summary>
    int UpdateOrder { get; }

    /// <summary>
    /// initializes the contrainet.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Updates the contraint.
    /// </summary>
    void UpdateConstraint();
}
