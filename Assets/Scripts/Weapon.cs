using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer destroySpriteRenderer;

    public Stack<GameObject> pool;

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Planet"))
        {
            other.gameObject.GetComponent<Planet>().TakeDamage(destroySpriteRenderer);
            gameObject.SetActive(false);

            pool.Push(gameObject);
        }
    }

    private void Update() {
        Vector2 targetPosition = Game.instance.planet.transform.position;
        transform.SetPositionAndRotation(Vector2.MoveTowards(transform.position, targetPosition, 2f * Time.deltaTime), Quaternion.FromToRotation(Vector2.right, targetPosition - (Vector2)transform.position));
        if (Vector2.Distance(transform.position, targetPosition) < 0.001f)
        {
            gameObject.SetActive(false);
            pool.Push(gameObject);
        }
    }
}
