using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public ParticleSystem dust;
    public CameraMovement camera;

    [Header("Horizontal Movement")]
    public float moveSpeed = 10;
    public Vector2 direction;
    private bool facingRight = true;
    
    [Header("Vertical Movement")]
    public float jumpForce = 15;
    public float jumpDelay = 0.25f;
    public float jumpTimer;
    private bool wasOnground;

    [Header("Crouch")]
    public bool crouch = false;
    public float crouchSpeed;
    public float crouchJump;

    [Header("Dash")]
    public float dashSpeed;
    public float startDashTime;
    public float dashDelay;
    private float timeSinceLastDash;
    private float dashTime = 0;
    private bool airDash = true;

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    public LayerMask groundLayer;
    public GameObject charcaterHolder;

    [Header("Physics")]
    public float maxSpeed = 10f;
    public float linearDrag = 6;
    public float gravity = 1;
    public float fallMultiplier = 5;

    [Header("Collision")]
    public bool onGround = false;
    public float groundLength = 0.385f;
    public Vector3 colliderOffset;

    [Header("Sound")]
    public AudioClip playerJump;
    public AudioClip playerDash;
    public AudioClip playerDeath;
    private AudioSource audioSrc;

    private void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        wasOnground = onGround;
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);

        if(!wasOnground && onGround)
        {
            StartCoroutine(JumpSqueeze(1.2f, .8f, 0.05f));
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpTimer = Time.time + jumpDelay;

        }
        animator.SetBool("onGround", onGround);
        animator.SetBool("crouch", crouch);
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        if (onGround)
        {
            airDash = true;
            if (Input.GetButtonDown("Dash") && Time.time > timeSinceLastDash)
            {
                CreateDust();
                PlaySound(playerDash);
                dashTime = startDashTime;
                timeSinceLastDash = Time.time + dashDelay;
            }
        }
        else
        {
            if (Input.GetButtonDown("Dash") && airDash)
            {
                CreateDust();
                PlaySound(playerDash);
                dashTime = startDashTime;
                airDash = false;
            }
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        GameOver();
    }

    void FixedUpdate()
    {
        MoveCharacter(direction.x);
        if (jumpTimer > Time.time && onGround)
        {
            Jump();
        }
        ModifyPhysics();

        if (direction.y < 0 && onGround)
        {
            crouch = true;
            charcaterHolder.transform.localScale = new Vector3(1.25f, .75f, 1f);
            rb.drag = linearDrag;
        }
        else
        {
            crouch = false;
            charcaterHolder.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        if (dashTime > 0)
        {
            Dash();
        }
    }

    void Dash()
    {
        StartCoroutine(JumpSqueeze(1.2f, .8f, 0.1f));
        dashTime -= Time.deltaTime;
        rb.velocity = new Vector2((facingRight ? dashSpeed : -dashSpeed)+camera.speed, rb.velocity.y);

    }

    void MoveCharacter(float horizontal)
    {
        rb.AddForce(Vector2.right * horizontal * (crouch ? crouchSpeed : moveSpeed));

        animator.SetFloat("Horizontal", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("Vertical", rb.velocity.y);

        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
        {
            Flip();
        }
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x)*maxSpeed, rb.velocity.y);
        }

    }

    void Jump()
    {
        CreateDust();
        PlaySound(playerJump);
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * (crouch ? crouchJump : jumpForce), ForceMode2D.Impulse);
        jumpTimer = 0;
        StartCoroutine(JumpSqueeze(0.8f, 1.2f, .1f));
    }

    void Flip()
    {
        CreateDust();
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0: 180, 0);
    }

    void ModifyPhysics()
    {
        bool changingDirections = (direction.x > 0 && rb.velocity.x < 0) || (direction.x < 0 && rb.velocity.x > 0);

        if (onGround)
        {
            if (Mathf.Abs(direction.x) < 0.4f || changingDirections)
            {
                rb.drag = linearDrag;
            }
            else
            {
                rb.drag = 0f;
            }
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = gravity;
            rb.drag = linearDrag * 0.5f;
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = gravity * fallMultiplier;
            }else if(rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                rb.gravityScale = gravity * (fallMultiplier / 2);
            }
        }
    }

    IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float t = 0f;
        while(t <= 1.0f)
        {
            t += Time.deltaTime / seconds;
            charcaterHolder.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            charcaterHolder.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            yield return null;
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);

    }

    void CreateDust()
    {
        dust.Play();
    }

    private void PlaySound(AudioClip sound)
    {
        audioSrc.PlayOneShot(sound);
    }

    private void GameOver()
    {
        if (Mathf.Abs(gameObject.transform.position.x - camera.transform.position.x) > 9 || (gameObject.transform.position.y-camera.transform.position.y)<-5.5)
        {
            PlaySound(playerDeath);
            camera.enabled = false;
            gameObject.GetComponent<Player>().enabled = false;

        }
    }

}
