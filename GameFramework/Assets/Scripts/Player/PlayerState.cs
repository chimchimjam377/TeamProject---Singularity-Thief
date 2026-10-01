using UnityEngine;

public enum PlayerStateType
{
    Normal,
    WallClimb,
    Dodge,
    Attack,
    Hit,
    Dead
}

public class PlayerState : MonoBehaviour
{
    public PlayerStateType CurrentState { get; private set; }
        = PlayerStateType.Normal;

    public bool Is(PlayerStateType state)
    {
        return CurrentState == state;
    }

    public bool CanDodge()
    {
        return CurrentState == PlayerStateType.Normal
            || CurrentState == PlayerStateType.WallClimb;
    }

    public void SetState(PlayerStateType newState)
    {
        CurrentState = newState;
    }
}
