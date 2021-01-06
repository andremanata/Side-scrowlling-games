using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    public float speed = 0;
    public Text score;
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += speed * Vector3.right;
        if (speed< .25f)
        {
            speed += Time.deltaTime * 0.01f;
        }
        score.text = "Score: " + (int)(gameObject.transform.position.x*10); 
    }
}
