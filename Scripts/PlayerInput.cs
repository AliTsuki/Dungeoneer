using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerInput : MonoBehaviour
{
    public float Horizontal;
    public float Vertical;
    public bool UseRightLight;
    public bool UseRightHeavy;
    public bool UseLeftLight;
    public bool UseLeftHeavy;

    private bool ReadyToClear;

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        this.ClearInput();
        this.ProcessInputs();
    }

    // FixedUpdate is called a fixed number of times per second
    private void FixedUpdate()
    {
        this.ReadyToClear = true;
    }

    // Clear inputs
    private void ClearInput()
    {
        if(this.ReadyToClear == false)
        {
            return;
        }
        this.Horizontal = 0f;
        this.Vertical = 0f;
        this.UseRightLight = false;
        this.UseRightHeavy = false;
        this.UseLeftLight = false;
        this.UseLeftHeavy = false;
        this.ReadyToClear = false;
    }

    // Process inputs
    private void ProcessInputs()
    {
        this.Horizontal += Input.GetAxis("Horizontal");
        this.Vertical += Input.GetAxis("Vertical");
        // TODO: other inputs
    }
}
