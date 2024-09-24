using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public SpriteRenderer weaponRenderer;
    [HideInInspector]
    public Texture2D originalTexture;
    [HideInInspector]
    public Texture2D currentTexture;
    [HideInInspector]
    public SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 复制一份targetTexture
        originalTexture = spriteRenderer.sprite.texture;
        currentTexture = new Texture2D(originalTexture.width, originalTexture.height);
        currentTexture.SetPixels32(originalTexture.GetPixels32());
        currentTexture.Apply();

        // 转sprite
        spriteRenderer.sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), new Vector2(0.5f, 0.5f));
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 hitPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            weaponRenderer.transform.position = hitPoint;
            PixelPhysics.DestroyByMask(currentTexture, spriteRenderer, weaponRenderer);
            // PixelPhysics.UpdatePolygonCollider(spriteRenderer.sprite, GetComponent<PolygonCollider2D>());
            Destroy(GetComponent<PolygonCollider2D>());
            gameObject.AddComponent<PolygonCollider2D>();

            // 检查是否需要分裂
            if (CheckDetach())
            {
                Split();
            }
        }
    }

    // 检查是否需要分裂
    bool CheckDetach()
    {
        if (GetComponent<PolygonCollider2D>().pathCount <= 1)
        {
            return false;
        }

        return true;
    }

    void Split()
    {
        var collider = GetComponent<PolygonCollider2D>();
        List<Vector2[]> paths = new List<Vector2[]>();

        // 获取所有形状（碎片）
        for (int i = 0; i < collider.pathCount; i++)
        {
            paths.Add(collider.GetPath(i));
        }

        Vector2 center = Vector2.zero; // 星球中心，假设是 (0,0)
        float coreRadius = 0.05f; // 核心半径
        Vector2[] centerPiece = null;

        foreach (var path in paths)
        {
            if (IsWithinRadius(path, center, coreRadius))
            {
                centerPiece = path;
                break;
            }
        }

        if (centerPiece == null)
        {
            Debug.LogError("No central piece found!");
            return;
        }

        // 遍历所有碎片，分离为独立的 GameObject
        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i] == centerPiece) continue; // 中心的主星球不分离

            CreatePiece(paths[i]);
        }

        // 更新当前星球的 PolygonCollider
        collider.SetPath(0, centerPiece); // 仅保留中心的碎片作为主星球
    }

    bool IsWithinRadius(Vector2[] path, Vector2 center, float radius)
    {
        // 判断多边形的顶点是否有一个在核心区域内
        foreach (var point in path)
        {
            float distance = Vector2.Distance(point, center);
            if (distance < radius)
            {
                return true; // 如果有顶点在核心圆内，返回true
            }
        }

        // 判断核心圆的中心是否在多边形内部
        if (IsPointInPolygon(center, path))
        {
            return true; // 核心圆的中心点在多边形内部，返回true
        }

        // 如果没有顶点在核心圆内，则判断核心圆是否和多边形相交
        for (int i = 0; i < path.Length; i++)
        {
            Vector2 startPoint = path[i];
            Vector2 endPoint = path[(i + 1) % path.Length];

            // 检查核心圆是否与多边形的边相交
            if (LineIntersectsCircle(startPoint, endPoint, center, radius))
            {
                return true;
            }
        }

        return false;
    }

    // 检测多边形的一条边是否与核心圆相交
    bool LineIntersectsCircle(Vector2 startPoint, Vector2 endPoint, Vector2 center, float radius)
    {
        Vector2 startToCenter = center - startPoint;
        Vector2 lineDir = endPoint - startPoint;
        float lineLength = lineDir.magnitude;
        lineDir.Normalize();

        float projection = Vector2.Dot(startToCenter, lineDir);
        Vector2 closestPoint;

        if (projection < 0)
        {
            closestPoint = startPoint;
        }
        else if (projection > lineLength)
        {
            closestPoint = endPoint;
        }
        else
        {
            closestPoint = startPoint + lineDir * projection;
        }

        float distanceToCenter = Vector2.Distance(closestPoint, center);
        return distanceToCenter < radius;
    }

    // 判断点是否在多边形内的算法
    bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool isInside = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
            j = i;
        }
        return isInside;
    }


    // 创建独立的碎片
    void CreatePiece(Vector2[] path)
    {
        // 创建碎片对象
        GameObject piece = new GameObject("Piece");
        piece.transform.position = transform.position;
        piece.transform.rotation = transform.rotation;
        piece.transform.localScale = transform.localScale;

        // 添加 MeshRenderer 和 MeshFilter
        MeshFilter meshFilter = piece.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = piece.AddComponent<MeshRenderer>();
        PolygonCollider2D polyCollider = piece.AddComponent<PolygonCollider2D>();

        Vector3[] pathVertices = Array.ConvertAll(path, v => new Vector3(v.x, v.y, 0));

        // 创建 Mesh 并设置顶点
        Mesh mesh = new Mesh();
        Vector3[] vertices3D = new Vector3[pathVertices.Length];
        for (int j = 0; j < pathVertices.Length; j++)
        {
            vertices3D[j] = pathVertices[j]; // 直接将 2D 顶点转换为 3D
        }

        // 为碎片进行三角化
        int[] triangles = TriangulatePath(pathVertices); // TriangulatePath 是你已有的三角化算法
        mesh.vertices = vertices3D;
        mesh.triangles = triangles;

        // 计算 UV，利用原图的 UV 坐标
        Vector2[] uvs = new Vector2[pathVertices.Length];
        Rect textureRect = spriteRenderer.sprite.textureRect;  // 获取精灵的 textureRect
        Vector2 textureSize = new Vector2(originalTexture.width, originalTexture.height);

        for (int j = 0; j < pathVertices.Length; j++)
        {
            Vector2 localPos = path[j] - (Vector2)transform.position; // 以世界坐标计算
            Vector2 normalizedPos = new Vector2(
                (localPos.x + spriteRenderer.sprite.bounds.extents.x) / spriteRenderer.sprite.bounds.size.x,
                (localPos.y + spriteRenderer.sprite.bounds.extents.y) / spriteRenderer.sprite.bounds.size.y
            );

            uvs[j] = new Vector2(
                normalizedPos.x * textureSize.x / originalTexture.width,
                normalizedPos.y * textureSize.y / originalTexture.height
            );
        }

        mesh.uv = uvs;

        // 为碎片赋予 Mesh
        meshFilter.mesh = mesh;
        polyCollider.SetPath(0, path); // 设置碰撞器路径

        // 使用原图的材质
        meshRenderer.material = new Material(Shader.Find("Sprites/Default")); // 使用默认的 Sprite Shader
        meshRenderer.material.mainTexture = originalTexture; // 使用原始的 Texture

        // 设置渲染层次
        meshRenderer.sortingLayerID = spriteRenderer.sortingLayerID; // 碎片和原来的Sprite同一层
        meshRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // 确保碎片渲染在前
    }

    // 从 path 创建 Mesh
    Mesh CreateMeshFromPath(Vector2[] path)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            vertices[i] = path[i];
        }

        // 简单生成三角形索引（需要优化）
        int[] triangles = TriangulatePath(vertices);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    // 将凸多边形路径转为三角形索引
    int[] TriangulatePath(Vector3[] vertices)
    {
        List<int> indices = new List<int>();

        // 简单的凸多边形三角化，每三个连续顶点构成一个三角形
        for (int i = 1; i < vertices.Length - 1; i++)
        {
            indices.Add(0);     // 固定一个点作为三角形的一端
            indices.Add(i);
            indices.Add(i + 1);
        }

        return indices.ToArray();
    }

}
