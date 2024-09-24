using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*
    破坏像素的物理类
    方法：
        根据碰撞点和武器的破坏Mask，计算出被碰撞Texture
*/

public static class PixelPhysics
{
    /// <summary>
    /// texture图形-武器mask图形
    /// </summary>
    public static void DestroyByMask(Texture2D texture, SpriteRenderer origin, SpriteRenderer targetMask)
    {
        Texture2D maskTexture = targetMask.sprite.texture;

        bool isChange = false;

        Vector2 halfSize = new(texture.width / 2, texture.height / 2);
        Vector2 halfMaskSize = new(maskTexture.width / 2, maskTexture.height / 2);

        // 遍历maskTexture的每个像素
        for (int x = 0; x < maskTexture.width; x ++) {
            for (int y = 0; y < maskTexture.height; y ++) {
                if (maskTexture.GetPixel(x, y).a == 0) {
                    continue;
                }

                Vector3 worldPos = targetMask.transform.TransformPoint(new((x - halfMaskSize.x) / targetMask.sprite.pixelsPerUnit, (y - halfMaskSize.y) / targetMask.sprite.pixelsPerUnit));
                var orginLocalPos = origin.transform.InverseTransformPoint(worldPos);
                Vector2Int texturePos = Vector2Int.FloorToInt(new(orginLocalPos.x * origin.sprite.pixelsPerUnit + halfSize.x, orginLocalPos.y * origin.sprite.pixelsPerUnit + halfSize.y));

                if (texturePos.x < 0 || texturePos.x >= texture.width || texturePos.y < 0 || texturePos.y >= texture.height) {
                    continue;
                }


                if (texture.GetPixel(texturePos.x, texturePos.y).a > 0) {
                    texture.SetPixel(texturePos.x, texturePos.y, Color.clear);
                    isChange = true;
                }
            }
        }
        if (isChange) {
            texture.Apply();
        }
    }

    // public static void UpdatePolygonCollider(Sprite sprite, PolygonCollider2D polygonCollider)
    // {
    //     List<Vector2[]> points = ExtractPathFromTexture(sprite);

    //     polygonCollider.pathCount = points.Count;

    //     for (int i = 0; i < points.Count; i++) {
    //         polygonCollider.SetPath(i, points[i]);
    //     }
    // }

    /// <summary>
    /// 提取非透明像素的轮廓路径
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    // public static List<Vector2[]> ExtractPathFromTexture(Sprite sprite)
    // {
    //     List<Vector2[]> points = new();

    //     // TODO 暂时使用Sprite的轮廓
    //     // Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);

    //     for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++) {
    //         List<Vector2> path = new();
    //         sprite.GetPhysicsShape(i, path);
    //         points.Add(path.ToArray());
    //     }

    //     return points;
    // }

}