using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class itemScript : MonoBehaviour
{

    public Transform transform;
    public float maxScale = 3f;
    public float bobAmount = 1f;
    public float bobSpeed = 1f;
    public LayerMask mask;

    public UnityEvent itemEvent;

    Vector2 startPosition;
    CircleCollider2D collider;
    float timer = 0f;
    Collider2D[] colliderResults = new Collider2D[1];
    ContactFilter2D filter = new ContactFilter2D();

    // Start is called before the first frame update
    void Start()
    {
        transform = gameObject.GetComponent<Transform>();
        startPosition = transform.position;

        collider = gameObject.GetComponent<CircleCollider2D>();
        if (itemEvent == null)
        {
            itemEvent = new UnityEvent();
        }
        filter.SetLayerMask(mask);
    }

    void Update()
    {
        Array.Clear(colliderResults, 0, colliderResults.Length);
        int collisions = collider.OverlapCollider(new ContactFilter2D(), colliderResults);
        if (collisions > 0)
        {
            itemEvent.Invoke();
        }
    }

    void FixedUpdate()
    {
        move(transform);
    }

    private void move(Transform transform)
    {
        timer += Time.fixedDeltaTime;
        transform.position = new Vector2(startPosition.x, startPosition.y + bobAmount * Mathf.Sin(bobSpeed * timer));
        float scale = 1 + maxScale + maxScale * Mathf.Sin(bobSpeed * timer);
        transform.localScale = new Vector2(scale, scale);
        Debug.Log("scale" + scale);
    }
}
