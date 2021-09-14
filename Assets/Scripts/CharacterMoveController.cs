using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveController : MonoBehaviour
{
    [Header("Movement")]
    public float moveAccel;
    public float maxSpeed;


    [Header("Jump")]
    public float jumpAccel;
    private bool availableForDoubleJump = false;
    private bool isJumping;
    private bool isOnGround;
    private bool isDoubleJump;

    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;

    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;
    private float lastPositionX;

    [Header("GameOver")]
    public GameObject gameOverScreen;
    public float fallPositionY;

    [Header("Camera")]
    public CameraMoveController gameCamera;

    private Rigidbody2D rig;
    private Animator anim;
    private CharacterSoundController sound;

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * groundRaycastDistance), Color.white);
    }


    // Start is called before the first frame update
    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<CharacterSoundController>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Input
        if (Input.GetMouseButtonDown(0))
        {
            // untuk double jump
            if (!isOnGround && availableForDoubleJump==true)
            {
                isDoubleJump = true;
                sound.PlayJump();
                availableForDoubleJump = false;
            }
            // untuk jump
            if (isOnGround)
            {
                isJumping = true;
                sound.PlayJump();
                availableForDoubleJump = true;
            }
        }
        // Animation
        anim.SetBool("isOnGround", isOnGround);

        // Score 
        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPositionX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);

        if (scoreIncrement > 0)
        {
            score.IncreaseCurrentScore(scoreIncrement);
            lastPositionX += distancePassed;
        }

        if (transform.position.y < fallPositionY)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        // Simpan high score
        score.FinishScoring();

        // Stop camera movement
        gameCamera.enabled = false;

        // Show gameover
        gameOverScreen.SetActive(true);
        
        this.enabled = false;
    }

    // fixed update untuk physics
    private void FixedUpdate()
    {
        // raycast ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if (hit)
        {
            if (!isOnGround && rig.velocity.y <= 0)
            {
                isOnGround = true;
                availableForDoubleJump = false;
            }
        }
        else
        {
            isOnGround = false;
        }

        // calculate velocity vector
        Vector2 velocityVector = rig.velocity;

        if (isDoubleJump)
        {
            velocityVector.y += jumpAccel;
            isDoubleJump = false;
        }

        if (isJumping)
        {
            velocityVector.y += jumpAccel;
            isJumping = false;
        }


        velocityVector.x = Mathf.Clamp(velocityVector.x + moveAccel * Time.deltaTime, 0.0f, maxSpeed);

        rig.velocity = velocityVector;
    }
}
