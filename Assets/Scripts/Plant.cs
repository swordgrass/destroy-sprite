using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public SpriteRenderer weaponRenderer;
    [HideInInspector]
    public Texture2D targetTexture;
    [HideInInspector]
    public Texture2D currentTexture;
    [HideInInspector]
    public SpriteRenderer spriteRenderer;
    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        // 复制一份targetTexture
        targetTexture = spriteRenderer.sprite.texture;
        currentTexture = new Texture2D(targetTexture.width, targetTexture.height);
        currentTexture.SetPixels32(targetTexture.GetPixels32());
        currentTexture.Apply();

        // 转sprite
        spriteRenderer.sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), new Vector2(0.5f, 0.5f));
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 hitPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            weaponRenderer.transform.position = hitPoint;
            PixelPhysics.DestroyByMask(currentTexture, spriteRenderer, weaponRenderer);
            // PixelPhysics.UpdatePolygonCollider(spriteRenderer.sprite, GetComponent<PolygonCollider2D>());
            Destroy(GetComponent<PolygonCollider2D>());
            gameObject.AddComponent<PolygonCollider2D>();
        }
    }

        // 碎片分离
    void Split(){
        var collider = GetComponent<PolygonCollider2D>();
        for (int i = 0; i < collider.shapeCount; i ++) {
        
            GameObject piece = new GameObject("Piece");
            piece.transform.position = transform.position;
            piece.transform.rotation = transform.rotation;
            piece.transform.localScale = transform.localScale;

            MeshFilter meshFilter = piece.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = piece.AddComponent<MeshRenderer>();

            Mesh uMesh = meshFilter.sharedMesh;

            if (uMesh == null) 
            {
                meshFilter.mesh = new Mesh();
                uMesh = meshFilter.sharedMesh;
            }

            Vector2[] vertices = collider.GetPath(i);
            int[] triangles = new int[(vertices.Length - 2) * 3];

        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Weapon")) {
        }
    }
}
