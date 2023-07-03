using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;

    LayerMask obstacleMask; // the mask for the obstacles that block the player
    Vector2 targetPos;      // the target position for the player to move to
    Transform GFX;          // the transform of the child object's sprite renderer component
    float flipX;            // scale of the sprite on the x-axis for flip
    bool isMoving;          // a flag to indicate if the player is moving

    void Start()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
        GFX = GetComponentInChildren<SpriteRenderer>().transform;
        flipX = GFX.localScale.x;
    }

    void Update()
    {
        Move();
    }

    /// <summary>
    /// Player movement
    /// </summary>
    void Move()
    {
        // return only -1, 0 or 1.
        float horz = System.Math.Sign(Input.GetAxis("Horizontal"));
        float vert = System.Math.Sign(Input.GetAxis("Vertical"));

        if (Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0)
        {
            if (Mathf.Abs(horz) > 0)
            {
                GFX.localScale = new Vector2(flipX * horz, GFX.localScale.y);   // flip the sprite according to the input direction
            }
            if (!isMoving)
            {
                if (Mathf.Abs(horz) > 0)
                {
                    targetPos = new Vector2(transform.position.x + horz, transform.position.y);     // set the target position to one unit to the left or right
                }
                else if (Mathf.Abs(vert) > 0)
                {
                    targetPos = new Vector2(transform.position.x, transform.position.y + vert);     // set the target position to one unit up or down
                }

                // check obstacle at the target position
                Vector2 hitSize = Vector2.one * 0.8f;
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, obstacleMask);
                if (!hit)
                {
                    StartCoroutine(SmoothMove());
                }
            }
        }
    }

    /// <summary>
    /// Coroutine to move smoothly to the target position
    /// </summary>
    IEnumerator SmoothMove()
    {
        isMoving = true;
        while(Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }
}