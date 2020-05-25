using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{

    public Transform entity;
    public Sprite hookImage;
    public float offsetX = 0f;
    public float offsetY = 0f;
    public float maxExtension = 5f;
    public float extensionSpeed = 30f;
    public float contractionSpeed = 100f;
    public float rotationSpeed = 0.3f;
    public LayerMask mask;
    public LayerMask attackMask;
    public float dropForce = 500f;
    public float jumpHookAngle = 60f;
    public Animator grappleAnimator;
    public float attackIntervalSeconds = 0.1f;
    public GameObject redGlove;
    public GameObject blueGlove;
    public float baseAttack = 500f;
    public float attackAngle;

    bool isActive = false;
    bool isJumping = false;
    bool isAttacking = false;
    float secondsSinceLastAttack = 0f;

    float currentJumpHookAngle = 60f;
    float currentExtension = 0f;

    float rotation = 0f;
    const float rightRotationalGoal = 0f;
    const float leftRotationalGoal = 180f;

    float currentHorizontal;
    float rotationSign = 0f;
    

    public DistanceJoint2D joint;
    RaycastHit2D grabHit;
    const float deg2Rad = Mathf.PI / 180;

    Collider2D hitEntity;
    Rigidbody2D hitRigidbody2D;
    Vector2 anchorPoint;

    GameObject currentGlove;
    CircleCollider2D gloveCollider;
    ContactFilter2D filter = new ContactFilter2D();
    List<int> attackedBodiesHashCode = new List<int>();

    enum GloveType
    {
        RED, BLUE
    }

    void Start()
    {
        transform.position = new Vector2(entity.position.x + offsetX, entity.position.y + offsetY);
        transform.localScale = new Vector2(0, transform.localScale.y);
        joint.enabled = false;

        redGlove.GetComponent<SpriteRenderer>().enabled = false;
        blueGlove.GetComponent<SpriteRenderer>().enabled = false;
        currentGlove = redGlove;
        filter.SetLayerMask(attackMask);
    }

    void Update()
    {
        currentHorizontal = Input.GetAxisRaw("Horizontal");


        if (!currentHorizontal.Equals(0.0f))
        {
            if (currentHorizontal > 0.0f)
            {
                rotationSign = -1;
                currentJumpHookAngle = jumpHookAngle;
            }
            else
            {
                rotationSign = 1;
                currentJumpHookAngle = 180f - jumpHookAngle;
            }
        }

        secondsSinceLastAttack += Time.deltaTime;
        isAttacking = Input.GetKey(KeyCode.F);
        bool startedAttack = Input.GetKeyDown(KeyCode.F);
        if (secondsSinceLastAttack > attackIntervalSeconds && startedAttack)
        {
            secondsSinceLastAttack = 0;
            currentGlove.GetComponent<SpriteRenderer>().enabled = true;
            grappleAnimator.SetTrigger("GrappleAttack");
            attackedBodiesHashCode.Clear();
        }

        isActive = Input.GetKey(KeyCode.G);
        bool startedGrab = Input.GetKeyDown(KeyCode.G);
        if (isAttacking)
        {
            gloveCollider = currentGlove.GetComponent<CircleCollider2D>();
            Collider2D[] results = new Collider2D[5];
            int collisions = gloveCollider.OverlapCollider(filter, results);
            for (int i = 0; i < collisions; i++)
            {
                Rigidbody2D body = results[i].gameObject.GetComponent<Rigidbody2D>();
                if (attackedBodiesHashCode.Contains(body.GetHashCode()))
                {
                    Debug.Log("Duplicate hash " + body.GetHashCode());
                    continue;
                }
                attackedBodiesHashCode.Add(body.GetHashCode());
                float angle = rotationSign < 0
                    ? attackAngle
                    : 180 - attackAngle;

                Vector2 forceDirection = new Vector2(Mathf.Cos(angle * deg2Rad), Mathf.Sin(angle * deg2Rad));
                float attackForceScale = baseAttack;
                Vector3 attackForceVector = new Vector3(attackForceScale, attackForceScale, attackForceScale);
                body.AddForce(Vector3.Scale(forceDirection, attackForceVector));
            }

        }
        else if (isActive)
        {
            if (startedGrab)
            {
                grappleAnimator.SetTrigger("GrappleGrab");
            }
            
            if (hitEntity == null)
            {
                grabHit = Physics2D.Raycast(entity.position, new Vector2(Mathf.Cos(rotation * deg2Rad), Mathf.Sin(rotation * deg2Rad)), currentExtension, mask);
                if (grabHit.collider != null)
                {
                    hitEntity = grabHit.collider;
                    hitRigidbody2D = hitEntity.GetComponent<Rigidbody2D>();
                    if (hitRigidbody2D == null) // Fixed structure, not an entity
                    {
                        anchorPoint = new Vector2(entity.position.x + maxExtension * Mathf.Cos(rotation * deg2Rad), entity.position.y + maxExtension * Mathf.Sin(rotation * deg2Rad));
                        joint.connectedAnchor = anchorPoint;
                        joint.enabled = true;
                        joint.distance = maxExtension;
                        Debug.Log("hit joint: " + hitEntity.name);
                    }

                }
            }

        }
        else
        {
            if (hitRigidbody2D != null)
            {
                Vector2 rotationDirection = new Vector2(Mathf.Cos(rotation * deg2Rad), Mathf.Sin(rotation * deg2Rad));
                Vector3 forceDirection = Vector3.Cross(rotationDirection, Vector3.forward).normalized;

                float dropForceDirection = -dropForce * rotationSign;
                Vector3 forceScale = new Vector3(dropForceDirection, dropForceDirection, dropForceDirection);
                hitRigidbody2D.AddForce(Vector3.Scale(forceDirection, forceScale));
            }
            hitEntity = null;
            hitRigidbody2D = null;
            joint.enabled = false;
        }

        if (hitEntity != null)
        {
            if (hitRigidbody2D != null)
            {
                grabHit.transform.position = new Vector2(entity.position.x + currentExtension * Mathf.Cos(rotation * deg2Rad), entity.position.y + currentExtension * Mathf.Sin(rotation * deg2Rad));
            }
            else
            {
                // swing

            }
        }

        if (!isAttacking)
        {
            currentGlove.GetComponent<SpriteRenderer>().enabled = false;
        }

    }

    private void FixedUpdate()
    {
        currentExtension = isAttacking
            ? maxExtension
            : Clamp(0,
                maxExtension,
                currentExtension + Time.fixedDeltaTime * (isActive ? extensionSpeed : -contractionSpeed));

        transform.position = new Vector2(entity.position.x + offsetX, entity.position.y + offsetY);
        transform.localScale = new Vector2(isActive ? maxExtension : currentExtension, transform.localScale.y);

        if (hitEntity != null && hitRigidbody2D == null)
        {
            rotation = Mathf.Atan((anchorPoint.y - entity.transform.position.y) / (anchorPoint.x - entity.transform.position.x)) / deg2Rad;
            if (rotation < 0)
            {
                rotation += 180f;
            }
            Debug.Log("rotation: " + rotation);
        }
        else if (isJumping)
        {
            rotation = currentJumpHookAngle;
        }
        else
        {
            rotation = Clamp(rightRotationalGoal, leftRotationalGoal, rotation + Time.fixedDeltaTime * rotationSpeed * rotationSign);
        }

        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    private float Clamp(float min, float max, float value)
    {
        if (value > max)
        {
            return max;
        }

        if (value < min)
        {
            return min;
        }
        return value;
    }

    private void setIsJumping(bool isJumping)
    {
        this.isJumping = isJumping;
    }

    public void inAir()
    {
        setIsJumping(true);
    }

    public void onGround()
    {
        setIsJumping(false);
    }

    private void setGlove(GloveType gloveType)
    {
        Debug.Log("setting glove: " + gloveType);
        currentGlove.GetComponent<SpriteRenderer>().enabled = false;
        switch (gloveType)
        {
            case GloveType.BLUE:
                currentGlove = blueGlove;
                break;
            case GloveType.RED:
                currentGlove = redGlove;
                break;
        }
    }

    public void setRedGlove()
    {
        setGlove(GloveType.RED);
    }

    public void setBlueGlove()
    {
        setGlove(GloveType.BLUE);
    }
}
