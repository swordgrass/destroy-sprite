using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Texture2D weaponMask;
    public Vector2 destroyScale;
    public Vector2 destroyOffset;

    private void Start() {
        weaponMask = spriteRenderer.sprite.texture;

        Transform maskTrans = spriteRenderer.transform;
        destroyScale = maskTrans.lossyScale;
    }
}
