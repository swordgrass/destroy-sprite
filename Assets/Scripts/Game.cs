using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public GameObject prefab;
    public Stack<GameObject> pool = new();
    public static Game instance;

    public Planet planet;

    private void Awake() {
        instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject go = Instantiate(prefab);
            go.SetActive(false);
            pool.Push(go);
        }
    }

    GameObject Fetch(Vector3 position)
    {
        GameObject go;
        if (pool.Count > 0)
        {
            go = pool.Pop();
            go.transform.position = position;
        }else
        {
            go = Instantiate(prefab, position, Quaternion.identity);
        }
        return go;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 hitPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = ((Vector3)hitPoint - planet.transform.position).normalized;
            Vector2 targetPosition = hitPoint + direction * 5;

            GameObject go = Fetch(targetPosition);
            go.GetComponent<Weapon>().pool = pool;
            go.SetActive(true);
        }
    }
}
