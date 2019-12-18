using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public GameObject PlayerSpritesObject;
    public Animator animator;
    public PlayerController controller;

    // Start is called before the first frame update
    void Start()
    {
        this.animator = this.PlayerSpritesObject.GetComponent<Animator>();
        this.controller = this.transform.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        this.animator.SetFloat("Velocity", this.controller.Velocity.magnitude);
        if(this.controller.Velocity.x > 0.01)
        {
            this.PlayerSpritesObject.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(this.controller.Velocity.x < -0.01)
        {
            this.PlayerSpritesObject.transform.localScale = Vector3.one;
        }
    }
}
