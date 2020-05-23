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
    public float dropForce = 500f;
    public float jumpHookAngle = 60f;
    

    bool isActive = false;
    bool isJumping = false;
    float currentJumpHookAngle = 60f;
    float currentExtension = 0f;

    float rotation = 0f;
    const float rightRotationalGoal = 0f;
    const float leftRotationalGoal = 180f;

    float currentHorizontal;
    float rotationSign = 0f;
    

    public DistanceJoint2D joint;
    RaycastHit2D hit;
    const float deg2Rad = Mathf.PI / 180;

    Collider2D hitEntity;
    Rigidbody2D hitRigidbody2D;
    Vector2 anchorPoint;


    void Start()
    {
        transform.position = new Vector2(entity.position.x + offsetX, entity.position.y + offsetY);
        transform.localScale = new Vector2(0, transform.localScale.y);
        joint.enabled = false;
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

        isActive = Input.GetKey(KeyCode.F);

        if (isActive)
        {
            if (hitEntity == null)
            {
                hit = Physics2D.Raycast(entity.position, new Vector2(Mathf.Cos(rotation * deg2Rad), Mathf.Sin(rotation * deg2Rad)), currentExtension, mask);
                if (hit.collider != null)
                {
                    hitEntity = hit.collider;
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
                // smash
                hit.transform.position = new Vector2(entity.position.x + currentExtension * Mathf.Cos(rotation * deg2Rad), entity.position.y + currentExtension * Mathf.Sin(rotation * deg2Rad));
            }
            else
            {
                // swing

            }
        }

    }

    private void FixedUpdate()
    {
        currentExtension = Clamp(0,
            maxExtension,
            currentExtension + Time.fixedDeltaTime * (isActive ? extensionSpeed : -contractionSpeed));

        transform.position = new Vector2(entity.position.x + offsetX, entity.position.y + offsetY);
        transform.localScale = new Vector2(currentExtension, transform.localScale.y);

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
}
