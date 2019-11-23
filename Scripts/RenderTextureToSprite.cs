using UnityEngine;
using UnityEngine.UI;

public class RenderTextureToSprite : MonoBehaviour
{
    public RenderTexture renderTexture;
    private Texture2D texture;
    private Sprite sprite;

    // Start is called before the first frame update
    public void Start()
    {
        this.texture = new Texture2D(this.renderTexture.width, this.renderTexture.height, TextureFormat.RGB24, false);
    }

    // Update is called once per frame
    public void Update()
    {
        RenderTexture.active = this.renderTexture;
        this.texture.ReadPixels(new Rect(0, 0, this.renderTexture.width, this.renderTexture.height), 0, 0);
        this.texture.Apply();
        this.sprite = Sprite.Create(this.texture, new Rect(0, 0, this.texture.width, this.texture.height), new Vector2(0.5f, 0.5f));
        this.GetComponent<Image>().sprite = this.sprite;
    }
}
