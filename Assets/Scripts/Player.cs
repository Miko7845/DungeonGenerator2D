using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;

    LayerMask obstacleMask;
    Vector2 targetPos;
    Transform GFX;
    float flipX;
    bool isMoving;

    void Start()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
        GFX = GetComponentInChildren<SpriteRenderer>().transform;
        flipX = GFX.localScale.x;
    }

    void Update()
    {
        float horz = System.Math.Sign(Input.GetAxis("Horizontal"));
        float vert = System.Math.Sign(Input.GetAxis("Vertical"));

        if(Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0)
        {
            if (Mathf.Abs(horz) > 0)
            {
                GFX.localScale = new Vector2(flipX * horz, GFX.localScale.y);
            }
            if(!isMoving)
            {
                if (Mathf.Abs(horz) > 0)
                {
                    targetPos = new Vector2(transform.position.x + horz, transform.position.y);
                }
                else if (Mathf.Abs(vert) > 0)
                {
                    targetPos = new Vector2(transform.position.x, transform.position.y + vert);
                }

                // check for collisions
                Vector2 hitSize = Vector2.one * 0.8f;
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, obstacleMask);
                if (!hit)
                {
                    StartCoroutine(SmoothMove());
                }
            }
        }
    }

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