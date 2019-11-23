using UnityEngine;

public class CameraClearFlagNone : MonoBehaviour
{
    public string CameraClearFlag = "";
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        this.cam = this.transform.GetComponent<Camera>();
        this.cam.clearFlags = CameraClearFlags.Nothing;
        this.CameraClearFlag = this.cam.clearFlags.ToString();
    }

    private void Update()
    {
        this.CameraClearFlag = this.cam.clearFlags.ToString();
    }
}
