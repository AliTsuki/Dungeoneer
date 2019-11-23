using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 8f;
    private Rigidbody2D rigidBody;
    private CapsuleCollider2D playerCollider;
    private PlayerInput input;
    private Vector2 velocity = new Vector2();

    // Start is called before the first frame update
    private void Start()
    {
        this.input = this.transform.GetComponent<PlayerInput>();
        this.rigidBody = this.transform.GetComponent<Rigidbody2D>();
        this.playerCollider = this.transform.GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        this.ProcessMovement();
    }

    //
    private void ProcessMovement()
    {
        this.velocity = new Vector2(this.input.Horizontal * this.MoveSpeed, this.input.Vertical * this.MoveSpeed);
        this.rigidBody.velocity = this.velocity;
    }
}
