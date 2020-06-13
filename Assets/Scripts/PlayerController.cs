﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Start() variables
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D bodyColl;
    private Collider2D attackColl;
    private Collider2D feetColl;
    public TextMeshProUGUI characterDialogue;
    public GameObject dialogueBubble;



    // Finite state machine
    private enum State { idle, running, jumping, falling, hurt, climb, attack };
    [SerializeField] private State state = State.idle;

    //Fighting
    [SerializeField] bool isAttacking;
    [SerializeField] GameObject attackHitBox;
    [SerializeField] bool isHurt;

    //Ladder variables
    [SerializeField] private float naturalGravity;
    [SerializeField] float climbSpeed = 3f;
    [SerializeField] LayerMask whatIsLadder;
    [SerializeField] bool isClimbing;

    // Inspector variables
    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 7f;
    [SerializeField] private static float beginningJumpForce = 20f;
    [SerializeField] private float jumpForce = beginningJumpForce;
    [SerializeField] private float hurtForce = 10f;
    [SerializeField] private AudioSource collectKey;
    [SerializeField] private AudioSource collectHeal;
    [SerializeField] private AudioSource footstep;
    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private AudioSource swordSwing;
    [SerializeField] public AudioSource hurtSound;
    [SerializeField] GameObject feetCollider;
    [SerializeField] private bool invincible;
    [SerializeField] private bool isColliding;
    public static int collectedCorn;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bodyColl = GetComponent<Collider2D>();
        attackColl = attackHitBox.GetComponent<Collider2D>();
        feetColl = feetCollider.GetComponent<Collider2D>();
        naturalGravity = rb.gravityScale;
        PermanentUI.perm.health = 3;
        invincible = false;
        collectedCorn = 0;
        


        attackHitBox.SetActive(false);

    }

    private void Update()
    {
       
            Attack();
            if(!isAttacking)
            {
                Idle();
                Run();
                FlipSprite();
                Jump();
                Climb();
            }
        
        

        Recovery();

        anim.SetInteger("state", (int)state); // Sets animation based on enumeratror state
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (isColliding) return;
        isColliding = true;

        if (collision.tag == "Collectable")
        {
            collectKey.Play();
            Destroy(collision.gameObject);
            PermanentUI.perm.keys += 1;
            PermanentUI.perm.keyAmount.text = PermanentUI.perm.keys.ToString();
            //Dialogue:
            if (PermanentUI.perm.keys == 1)
            {
                StartCoroutine(DialogueText("That's the first key."));
            }
            else if (PermanentUI.perm.keys == 2)
            {
                StartCoroutine(DialogueText("Only one more key."));
            }
            else if (PermanentUI.perm.keys == 3)
            {
                StartCoroutine(DialogueText("That's all the keys. Now to find the door."));
            }

        }

        if(collision.tag == "Dialogue")
        {
            StartCoroutine(DialogueText("I have to find the three keys!"));
            Destroy(collision.gameObject);
        }

        if (collision.tag == "Ending" && PermanentUI.perm.keys < 3)
        {
            StartCoroutine(DialogueText("I need three keys to open this door."));
        }

        if (collision.tag == "Heal")
        {
            collectHeal.Play();
            Destroy(collision.gameObject);
            PermanentUI.perm.numOfHearts += 1;
            PermanentUI.perm.health = PermanentUI.perm.numOfHearts;
            collectedCorn++;
            if(PermanentUI.perm.numOfHearts == 4)
            {
                StartCoroutine(DialogueText("That's sacred corn stolen by the witch. If I collect more the gods will grant me strength."));
            }
            
        }

        if (collision.tag == "Powerup")
        {
            jumpForce = jumpForce + 10f;
            Destroy(collision.gameObject);
            GetComponent<SpriteRenderer>().color = Color.yellow;
            StartCoroutine(ResetPower());
        }

        if (collision.tag == "Attack")
        {
            state = State.hurt;

            StartCoroutine(HandleHealth()); // Dealts with health, updading UI

            

            if (collision.gameObject.transform.position.x > transform.position.x)
            {
                //Enemy is to my right therefore I should be damaged and move left
                rb.velocity = new Vector2(-hurtForce, hurtForce);
            }
            else
            {
                //Enemy is to my left therefore I should be damaged and move right
                rb.velocity = new Vector2(hurtForce, hurtForce);
            }
        }

        if (collision.gameObject.tag == "Fall")
        {
            hurtSound.Play();
            StartCoroutine(HandleHealth());

        }

        StartCoroutine(Reset());


    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Trap" || other.gameObject.tag == "Arrow")
        {
            if (other.gameObject.tag == "Enemy" && state == State.attack)
            {
                Enemy enemy = other.gameObject.GetComponent<Enemy>();
            }
            else
            {
                state = State.hurt;
                hurtSound.Play();
                StartCoroutine(HandleHealth()); // Dealts with health, updading UI

                if (other.gameObject.tag == "Arrow")
                {
                    Destroy(other.gameObject);
                }

                if (other.gameObject.transform.position.x > transform.position.x)
                {
                    //Enemy is to my right therefore I should be damaged and move left
                    rb.velocity = new Vector2(-hurtForce, hurtForce);
                }
                else
                {
                    //Enemy is to my left therefore I should be damaged and move right
                    rb.velocity = new Vector2(hurtForce, hurtForce);
                }

                
            }
        }

        if(other.gameObject.tag == "Platform")
        {
            this.transform.parent = other.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if(other.gameObject.tag == "Platform")
        {
            this.transform.parent = null;
        }    
    }
    IEnumerator HandleHealth()
    {
        if (invincible == false)
        {
            PermanentUI.perm.health -= 1;
            if (PermanentUI.perm.health <= 0)
            {
                SceneManager.LoadScene("Death");
            }
        }
        invincible = true;
        yield return new WaitForSeconds(2f);
        invincible = false;
    } 

    private void Climb()
    {

        if (!feetColl.IsTouchingLayers(LayerMask.GetMask("Ladder"))) {
            rb.gravityScale = naturalGravity;
            
            anim.speed = 1f;
            return;
        }
        
        float vDirection = Input.GetAxis("Vertical");
        Vector2 climbVelocity = new Vector2(rb.velocity.x, vDirection * climbSpeed);
        rb.velocity = climbVelocity;
        rb.gravityScale = 0f;
        state = State.climb;
        isClimbing = Mathf.Abs(vDirection) > 0.1f;
        if (feetColl.IsTouchingLayers(LayerMask.GetMask("Ladder")))
        {
            
            
            if (Mathf.Abs(vDirection) > 0.5f)
            {
                anim.speed = 1f;
            } else
            {
                anim.speed = 0f;
            }
        }


    }

    private void Run()
    {
        float hDirection = Input.GetAxis("Horizontal");
        Vector2 playerVelocity = new Vector2(hDirection * speed, rb.velocity.y);
        rb.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed && Mathf.Abs(hDirection) > Mathf.Epsilon && Mathf.Abs(hDirection) > Mathf.Epsilon)
        {
            state = State.running;
        }
        
        
    }

    private void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
            dialogueBubble.transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }
    }


    private void Jump()
    {
        
        if (Input.GetButtonDown("Jump") && feetColl.IsTouchingLayers(ground) )
        {
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpForce);
            rb.velocity += jumpVelocityToAdd;
            jumpSound.Play();
        }

        if (!feetColl.IsTouchingLayers(ground))
        {
            if (rb.velocity.y > 0f)
            {
                state = State.jumping;
            }
            else if (rb.velocity.y < 0f)
            {
                state = State.falling;
            }
        }
    }

    private void Idle()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
        if (!playerHasHorizontalSpeed && !isAttacking && feetColl.IsTouchingLayers(ground))
        {
            state = State.idle;
        }

        if(Input.GetKeyDown(KeyCode.X))
        {
            state = State.idle;
        }
    }

    private void Recovery()
    {
        if (state == State.hurt)
        {
            isAttacking = false;
            if (Mathf.Abs(rb.velocity.x) < Mathf.Epsilon)
            {
                state = State.idle;
                
            }
        }
    }

    private void Attack()
    {
        if (Input.GetButtonDown("Fire3") && !isAttacking && feetColl.IsTouchingLayers(ground) && state != State.running)
        {
            isAttacking = true;
            state = State.attack;
            
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    void attackHitboxOn()
    {
        attackHitBox.SetActive(true);
        swordSwing.Play();
    }

    void attackHitboxOff()
    {
        attackHitBox.SetActive(false);
    }


    private void FootStep()
    {
        footstep.Play();
    }

    private IEnumerator ResetPower()
    {
        yield return new WaitForSeconds(10);
        jumpForce = beginningJumpForce;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    IEnumerator Reset()
    {
        yield return new WaitForEndOfFrame();
        isColliding = false;
    }

    public IEnumerator DialogueText(string dialogueText)
    {
        dialogueBubble.SetActive(true);
        characterDialogue.text = dialogueText;
        yield return new WaitForSeconds(5f);
        characterDialogue.text = "";
        dialogueBubble.SetActive(false);
    }

}
