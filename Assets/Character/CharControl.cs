using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CharControl : MonoBehaviour
{
    public float maxSpeed = 30f;
    public float jumpForce = 30f;
    private bool jump;
    private float move;
    private Vector3 m_Velocity = Vector3.zero;
    private bool m_FacingRight;
    private readonly float m_MovementSmoothing = .01f;
    private bool isGrounded;
    public LayerMask ground;
    public LayerMask ladder;
    public LayerMask enemy;
    public LayerMask signLayer;
    public GameObject spawnpoint;
    private float climb;
    private bool canClimb;
    private GameObject sign;
    public TextMeshProUGUI ui;
    public bool paused;
    public int stage = 1;
    private bool flag = false;
    private float moveButton;
    private bool jumpButton;
    private float climbButton;
    private bool interactButton;
    private bool interact;
    public Image right;
    public Image chain;
    public Image up;
    public Image space;
    public Image E;
    private Stopwatch stopwatch;
    public TextMeshProUGUI timer;
    public AudioSource audioSource;
    public AudioClip R;
    public AudioClip L;
    public AudioClip Chain;
    private void Start()
    {
        transform.position = spawnpoint.transform.position;
        stopwatch = new Stopwatch();
    }
    public void StartGame()
    {

        paused = false;
        stopwatch.Start();
    }
    public void StepR()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = R;
            audioSource.Play();
        }
    }
    public void StepL()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = L;
            audioSource.Play();
        }
    }
    // Update is called once per frame
    void Update()
    {
        moveButton = paused ? 0 : Input.GetAxis("Horizontal");
        jumpButton = !paused && Input.GetButton("Jump");
        climbButton = paused ? 0 : Input.GetAxis("Vertical");
        interactButton = Input.GetButtonDown("Interact");
        timer.text = stopwatch.Elapsed.ToString();
        switch (stage)
        {
            case 0:
                move = moveButton;
                jump = jumpButton;
                climb = climbButton;
                interact = interactButton;
                break;
            case 1:
                move = moveButton < 0 ? moveButton : paused || sign != null ? 0 : interactButton ? 1 : moveButton;
                jump = jumpButton;
                climb = climbButton;
                interact = interactButton || Input.GetAxis("Horizontal") > 0;
                break;
            case 2:
                move = Mathf.Min(1, Mathf.Max(0, climbButton) + moveButton);
                jump = jumpButton;
                climb = Mathf.Min(1, Mathf.Max(0, moveButton) + climbButton);
                interact = interactButton;
                break;
            case 3:
                move = jumpButton ? 1 : moveButton;
                jump = moveButton != 0 || jumpButton;
                climb = climbButton;
                interact = interactButton;
                break;
        }
        isGrounded = GetComponent<EdgeCollider2D>().IsTouchingLayers(ground);
        canClimb = GetComponent<CircleCollider2D>().IsTouchingLayers(ladder);
        GetComponent<Animator>().SetBool("Air", !isGrounded);
        GetComponent<Animator>().SetBool("Jump", jump);
        GetComponent<Animator>().SetBool("Run", Mathf.Abs(move) > 0.1 && Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) >= 0.1);
        if (GetComponent<CircleCollider2D>().IsTouchingLayers(enemy))
        {
            transform.position = spawnpoint.transform.position;
            paused = false;
        }
        if (interact)
        {
            if (sign != null)
            {
                if (!paused)
                {
                    ui.text = sign.GetComponent<sign>().text + "\n Press E to stop reading sign";
                    paused = true;
                }
                else
                {
                    ui.text = "Press E to read sign";
                    paused = false;
                }
            }
        }
        if ((Input.GetButtonDown("Enter") && flag&&stage!=3)||(Input.GetButtonDown("Restart")&&flag&&stage==3))
        {
            flag = false;
            ui.enabled = false;
            paused = false;
            if (stage == 3)
            {
                stopwatch.Restart();
            }
            stage = (stage + 1) % 4;
            transform.position = spawnpoint.transform.position;
            audioSource.clip = Chain;
            audioSource.Play();
            switch (stage)
            {
                case 0:
                    right.enabled = false;
                    chain.enabled = false;
                    space.enabled = false;
                    break;
                case 1:
                    right.enabled = true;
                    chain.enabled = true;
                    E.enabled = true;
                    break;
                case 2:
                    E.enabled = false;
                    up.enabled = true;
                    break;
                case 3:
                    up.enabled = false;
                    space.enabled = true;
                    break;

            }
        }
        if (Input.GetButton("Cancel"))
        {
            Application.Quit();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("Sign") && GetComponent<CircleCollider2D>().IsTouchingLayers(signLayer))
        {
            sign = collision.gameObject;
            ui.enabled = true;
            ui.text = "Press E to read sign";
        }
        else if (collision.gameObject.CompareTag("Flag"))
        {
            ui.enabled = true;
            paused = true;
            flag = true;
            ui.text = "Stage Clear! Now do it again.\nPress Enter to go to the next stage";
            if (stage == 3)
            {
                ui.text = $"You won the game! Final Time: {stopwatch.Elapsed}\npress R to restart";
                stopwatch.Stop();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Sign") && !GetComponent<CircleCollider2D>().IsTouchingLayers(signLayer))
        {
            sign = null;
            ui.enabled = false;
        }
    }
    private void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().gravityScale = canClimb ? 0 : 2;
        float targetYVelocity = GetComponent<Rigidbody2D>().velocity.y;
        if (canClimb)
        {
            targetYVelocity = Mathf.Abs(climb) > 0.1 ? climb * maxSpeed / 1.5f : 0;
        }
        // Move the character by finding the target velocity
        Vector3 targetVelocity = new Vector2(move * maxSpeed, targetYVelocity);
        // And then smoothing it out and applying it to the character
        GetComponent<Rigidbody2D>().velocity = Vector3.SmoothDamp(GetComponent<Rigidbody2D>().velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
        // If the input is moving the player right and the player is facing left...
        if (move < 0 && !m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (move > 0 && m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
        if (jump && isGrounded)
        {
            Vector2 v = GetComponent<Rigidbody2D>().velocity;
            v.y = jumpForce;
            GetComponent<Rigidbody2D>().velocity = v;
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
