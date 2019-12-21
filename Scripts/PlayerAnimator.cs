using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // Editor fields
    public GameObject[] PlayerSpritesObjects = new GameObject[SpriteSheetCount];
    // Private fields
    private Animator[] animator = new Animator[SpriteSheetCount];
    private PlayerController controller;
    private float CurrentSpeed = 0.000f;
    private bool IsWalking = false;
    private FacingDirection PreviousFacing;
    private FacingDirection CurrentlyFacing = FacingDirection.Left;
    private int CurrentAnimator = 0;
    private const int SpriteSheetCount = 3;
    private enum FacingDirection
    {
        Left,
        Right,
        Up,
        Down
    }


    // Start is called before the first frame update
    void Start()
    {
        // Get reference to player controller
        this.controller = this.transform.GetComponent<PlayerController>();
        // Get all animators
        for(int i = 0; i < SpriteSheetCount; i++)
        {
            this.animator[i] = this.PlayerSpritesObjects[i].GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.UpdateIsWalking();
        if(this.IsWalking == true)
        {
            this.UpdateFacingDirection();
        }
        this.UpdateCurrentSpeed();
        this.ShowAndHideSprites();
        this.ApplyAnimation();
    }

    //
    private void UpdateIsWalking()
    {
        if(this.controller.Velocity.magnitude > 0.01)
        {
            this.IsWalking = true;
        }
        else
        {
            this.IsWalking = false;
        }
    }

    //
    private void UpdateFacingDirection()
    {
        // Get previous facing direction
        this.PreviousFacing = this.CurrentlyFacing;
        // Check if moving horizontally...
        if(Mathf.Abs(this.controller.Velocity.x) > Mathf.Abs(this.controller.Velocity.y))
        {
            // Right...
            if(this.controller.Velocity.x > 0)
            {
                this.CurrentlyFacing = FacingDirection.Right;
                this.CurrentAnimator = 0;
            }
            // Or Left...
            else
            {
                this.CurrentlyFacing = FacingDirection.Left;
                this.CurrentAnimator = 0;
            }
        }
        // Or vertically...
        else
        {
            // Up...
            if(this.controller.Velocity.y > 0)
            {
                this.CurrentlyFacing = FacingDirection.Up;
                this.CurrentAnimator = 2;
            }
            // Or Down...
            else
            {
                this.CurrentlyFacing = FacingDirection.Down;
                this.CurrentAnimator = 1;
            }
        }
    }

    //
    private void UpdateCurrentSpeed()
    {
        // Get current speed as magnitude of velocity
        this.CurrentSpeed = this.controller.Velocity.magnitude;
    }

    //
    private void ShowAndHideSprites()
    {
        if(this.CurrentlyFacing != this.PreviousFacing)
        {
            for(int i = 0; i < SpriteSheetCount; i++)
            {
                if(i == this.CurrentAnimator)
                {
                    this.PlayerSpritesObjects[i].SetActive(true);
                }
                else
                {
                    this.PlayerSpritesObjects[i].SetActive(false);
                }
            }
        }
    }

    //
    private void ApplyAnimation()
    {
        if(this.CurrentlyFacing == FacingDirection.Right)
        {
            // Flip sprite over x axis to face the right, set IsWalking to true, set anim speed to current speed
            this.PlayerSpritesObjects[this.CurrentAnimator].transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(this.CurrentlyFacing == FacingDirection.Left)
        {
            // Flip sprite to original scale, set IsWalking to true, set anim speed to current speed
            this.PlayerSpritesObjects[this.CurrentAnimator].transform.localScale = Vector3.one;
        }
        if(this.IsWalking == false)
        {
            this.animator[this.CurrentAnimator].speed = 1f;
        }
        else
        {
            this.animator[this.CurrentAnimator].speed = this.CurrentSpeed;
        }
        // Send IsWalking to animator
        this.animator[this.CurrentAnimator].SetBool("IsWalking", this.IsWalking);
    }
}
