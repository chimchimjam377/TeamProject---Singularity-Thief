using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    public Vector2 LookInput { get; private set; }

    public bool JumpPressed { get; private set; }

    public bool AttackPressed { get; private set; }

    public bool DodgePressed { get; private set; }

    private void Update()
    {
        MoveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        MoveInput = Vector2.ClampMagnitude(
            MoveInput,
            1f
        );

        LookInput = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        JumpPressed =
            Input.GetKeyDown(KeyCode.Space);

        AttackPressed =
            Input.GetMouseButtonDown(0);

        DodgePressed =
            Input.GetKeyDown(KeyCode.LeftShift);
    }

    public void ConsumeJump()
    {
        JumpPressed = false;
    }

    public void ConsumeAttack()
    {
        AttackPressed = false;
    }

    public void ConsumeDodge()
    {
        DodgePressed = false;
    }
}

