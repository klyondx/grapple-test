using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{

    public Transform camera;
    public float relativeMovement = 0.3f;
    
    void Update()
    {
        transform.position = new Vector2(camera.position.x * relativeMovement, camera.position.y * relativeMovement);    
    }
}
