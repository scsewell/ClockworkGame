public struct PlayerInput
{
    public readonly float moveH;
    public readonly bool jump;
    public readonly bool interact;

    public PlayerInput(float moveH, bool jump, bool interact)
    {
        this.moveH = moveH;
        this.jump = jump;
        this.interact = interact;
    }
}
