public interface IRigConstraint
{
    /// <summary>
    /// The update order for the constraint. Lower values are evaluated first.
    /// </summary>
    int UpdateOrder { get; }

    /// <summary>
    /// Updates the contrained bones.
    /// </summary>
    void UpdateConstraint();
}
