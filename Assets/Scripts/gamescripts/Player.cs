using UnityEngine;

public class Player{

    Main main;
    Game game;
    Gfx  gfx;
    Snd  snd;

    Sprite[] sprites;

    GameObject gameObject;

    Rigidbody2D playerRb;
    Vector2 currentVelocity = Vector2.zero;

    float x;
    float y;

    float jStrenght = 500, gForce = 100; //Vars that will affect forces for physycal behaviours
    bool isGrounded; //Var to check if player is on the ground
    bool isClimbing; //Var to check if player is climbing a wall
    int layerMask; //Var to filter layers for raycast calls
    float cooldown; //Var used for reloading the gun
    bool gunReady; //Var to check if gun is ready to fire

    public Player (Main inMain) {

        main = inMain;
        game = main.game;
        gfx  = main.gfx;
        snd  = main.snd;

        sprites = gfx.GetLevelSprites("Players/Player1");

        x = 100;
        y = 624;

        gameObject = gfx.MakeGameObject("Player", sprites[22], x, y,"Player");

        // ---------------------------------------------------------------
        // SET PLAYER VALUES AND NECESSARY COMPONENTS
        // ---------------------------------------------------------------

        gameObject.tag = "Player";
        gameObject.layer = 9;

        layerMask = LayerMask.GetMask("GroundLayer");

        SetPlayerRigidbody2d();

        //Add animator component to manage character animations
        gameObject.AddComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Players/PlayerController");
    }

    public Vector2 GetPlayerPos()
    {
        //Return current player position
        return gameObject.transform.position;
    }

    public bool GetPlayerLife()
    {
        //Return current player life status
        return gameObject.GetComponent<Animator>().GetBool("IsDead");
    }

    public void FrameEvent(float inMoveX, float inMoveY, bool inAim, bool inShoot) {


        // Player logic here

        // Definitely can be refined logic :^)
        //------------------------------------------------------------

        //Check if player is alive
        if (!gameObject.GetComponent<Animator>().GetBool("IsDead"))
        {
            //Shoot rays
            GroundRay();
            ClimbRay();

            //Check if playing is not floating
            if (!isGrounded)
            {
                if (isClimbing && playerRb.velocity.y < 0)
                {
                    if (inMoveY == 1)
                    {
                        playerRb.AddForce(new Vector2(0, Mathf.Pow(jStrenght, 1.85f)));
                        if (!gameObject.GetComponent<Animator>().GetBool("JumpAction"))
                            gameObject.GetComponent<Animator>().SetBool("JumpAction", true);
                    }
                }
                else
                {
                    Vector2 newGravity = new Vector2(0, -gForce);
                    playerRb.velocity += newGravity;
                    if (!gameObject.GetComponent<Animator>().GetBool("JumpAction"))
                        gameObject.GetComponent<Animator>().SetBool("JumpAction", true);
                }
            }
            else
            {
                //Check vertical and horizontal movements

                if (inMoveY == 1)
                {
                    playerRb.AddForce(new Vector2(0, Mathf.Pow(jStrenght, 1.8f)));
                }
                else
                {
                    if (gameObject.GetComponent<Animator>().GetBool("JumpAction"))
                        gameObject.GetComponent<Animator>().SetBool("JumpAction", false);
                }

                if (inMoveX != 0)
                {
                    float tempSpeed = inMoveX;
                    if (tempSpeed < 0)
                    {
                        tempSpeed *= -1;
                    }
                    gameObject.GetComponent<Animator>().speed = tempSpeed + 0.2f;
                    if (!gameObject.GetComponent<Animator>().GetBool("RunningVelocity"))
                        gameObject.GetComponent<Animator>().SetBool("RunningVelocity", true);
                }
                else
                {
                    if (gameObject.GetComponent<Animator>().GetBool("RunningVelocity"))
                        gameObject.GetComponent<Animator>().SetBool("RunningVelocity", false);
                }
            }

            if (inMoveX < 0)
            {
                gameObject.transform.localScale = new Vector2(-1, 1);
                if (gameObject.GetComponent<Animator>().GetBool("IsDucking"))
                    gameObject.GetComponent<Animator>().SetBool("IsDucking", false);
            }
            else if (inMoveX > 0)
            {
                gameObject.transform.localScale = new Vector2(1, 1);
                if (gameObject.GetComponent<Animator>().GetBool("IsDucking"))
                    gameObject.GetComponent<Animator>().SetBool("IsDucking", false);
            }
            else
            {
                if (inMoveY < 0)
                {
                    if (!gameObject.GetComponent<Animator>().GetBool("IsDucking"))
                        gameObject.GetComponent<Animator>().SetBool("IsDucking", true);
                }
                else
                {
                    if (gameObject.GetComponent<Animator>().GetBool("IsDucking"))
                        gameObject.GetComponent<Animator>().SetBool("IsDucking", false);
                }
            }

            //Create new forces to move body, push it away if it reaches map limits

            Vector2 newVelocity = new Vector2(inMoveX * jStrenght, playerRb.velocity.y);

            if (gfx.screenWidth == 1920)
            {
                if (gameObject.transform.position.x > -1300 && gameObject.transform.position.x < 3600)
                {
                    playerRb.velocity = Vector2.SmoothDamp(playerRb.velocity, newVelocity, ref currentVelocity, 0.05f);
                }
                else
                {
                    playerRb.velocity = Vector2.SmoothDamp(new Vector2(-playerRb.velocity.x * 2, 0), newVelocity, ref currentVelocity, 0.5f);
                }
            }
            else
            {
                if (gameObject.transform.position.x > -1000 && gameObject.transform.position.x < 2500)
                {
                    playerRb.velocity = Vector2.SmoothDamp(playerRb.velocity, newVelocity, ref currentVelocity, 0.05f);
                }
                else
                {
                    playerRb.velocity = Vector2.SmoothDamp(new Vector2(-playerRb.velocity.x * 2, 0), newVelocity, ref currentVelocity, 0.5f);
                }
            }

            main.cam.transform.position = new Vector3(Mathf.Lerp(main.cam.transform.position.x, gameObject.transform.position.x, jStrenght / 100 * Time.deltaTime), main.cam.transform.position.y, main.cam.transform.position.z);

            //Manage shoot mechanic

            if (inAim && isGrounded && !gameObject.GetComponent<Animator>().GetBool("IsDucking"))
            {
                if (!gameObject.GetComponent<Animator>().GetBool("IsAiming"))
                    gameObject.GetComponent<Animator>().SetBool("IsAiming", true);

                if (inShoot && gunReady)
                {
                    gameObject.GetComponent<Animator>().speed = 1;
                    if (!gameObject.GetComponent<Animator>().GetBool("ShootAction"))
                        gameObject.GetComponent<Animator>().SetBool("ShootAction", true);
                    snd.PlayAudioClip("Gun");
                    game.SpawnBullet("Player", gameObject.transform.position + (Vector3.up * jStrenght / 4), (int)gameObject.transform.localScale.x);
                    cooldown = 0;
                    gunReady = false;
                }
                else
                {
                    if (gameObject.GetComponent<Animator>().GetBool("ShootAction"))
                        gameObject.GetComponent<Animator>().SetBool("ShootAction", false);
                }
            }
            else
            {
                if (gameObject.GetComponent<Animator>().GetBool("IsAiming"))
                    gameObject.GetComponent<Animator>().SetBool("IsAiming", false);
            }

            //Reload mechanic

            if (cooldown < 0.5f)
            {
                cooldown += Time.deltaTime;
            }
            else
            {
                if (!gunReady)
                    gunReady = true;
            }
        }
        else
        {
            if(playerRb.velocity.x != 0)
            {
                Vector2 newRagdoll = new Vector2(-Time.deltaTime, -gForce);
                playerRb.velocity += newRagdoll;
            }
        }

        //------------------------------------------------------------
    }

    //Set values for player rigidbody
    void SetPlayerRigidbody2d()
    {
        playerRb = gameObject.AddComponent<Rigidbody2D>();
        playerRb.freezeRotation = true;
        playerRb.drag = 0;
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        playerRb.sharedMaterial = Resources.Load<PhysicsMaterial2D>("Players/PlayerMat");
    }

    //Check with ray if player is not floating
    private void GroundRay()
    {
        RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position - (Vector3.up * 10) + Vector3.left * 50 * gameObject.transform.localScale.x, -Vector2.up, 0, layerMask);

        Debug.DrawRay(gameObject.transform.position - (Vector3.up * 10) + Vector3.left * 50 * gameObject.transform.localScale.x, -Vector2.up, Color.white);

        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            if (!isGrounded)
                isGrounded = true;
        }
        else
        {
            if (isGrounded)
                isGrounded = false;
        }
    }

    //Check ray to manage Climb Mechanic
    private void ClimbRay()
    {
        RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position + (Vector3.up * jStrenght / 4) + Vector3.right * 100 * gameObject.transform.localScale.x, Vector2.right, 0, layerMask);

        Debug.DrawRay(gameObject.transform.position + (Vector3.up * jStrenght / 4) + Vector3.right * 100 * gameObject.transform.localScale.x, Vector2.right, Color.white);

        if (hit.collider != null && hit.collider.CompareTag("Ground") && !isGrounded)
        {
            if (!isClimbing)
                isClimbing = true;
            if (!gameObject.GetComponent<Animator>().GetBool("IsClimbing"))
                gameObject.GetComponent<Animator>().SetBool("IsClimbing", true);
        }
        else
        {
            if (isClimbing)
                isClimbing = false;
            if (gameObject.GetComponent<Animator>().GetBool("IsClimbing"))
                gameObject.GetComponent<Animator>().SetBool("IsClimbing", false);
        }
    }
}