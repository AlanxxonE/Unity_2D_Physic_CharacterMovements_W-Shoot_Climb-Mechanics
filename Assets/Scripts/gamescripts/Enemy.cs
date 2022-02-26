using UnityEngine;

public class Enemy : GeneralObject {

    GameObject gameObject;
    float cooldown; //Var for reloading gun
    bool fireGun; //Var to check if gun can be fired

    public Enemy(Main inMain, int inX, int inY) {

        SetGeneralVars(inMain, inX, inY);

        sprites = gfx.GetLevelSprites("Enemies/Enemy3_2");

        gameObject = gfx.MakeGameObject("Enemy", sprites[22], x, y);

        SetDirection(-1);

        // ---------------------------------------------------------------
        // SET ENEMY VALUES AND NECESSARY COMPONENTS
        // ---------------------------------------------------------------

        gameObject.tag = "Enemy";

        gameObject.AddComponent<Rigidbody2D>().isKinematic = true;

        gameObject.AddComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Enemies/EnemyController");
    }



    public override bool FrameEvent() {


        // enemy logic here


        // not enough time to build an A.I. x)
        //------------------------------------------------------------

        //Check if enemy is alive and well
        if (gameObject.activeSelf && !gameObject.GetComponent<Animator>().GetBool("EnemyDead"))
        {
            //Check if gun can be fired
            if (cooldown < 5)
            {
                cooldown += Time.deltaTime;
            }
            else
            {
                if(!fireGun)
                    fireGun = true;
            }

            //Check if player is nearby and alive
            if (Vector2.Distance(gameObject.transform.position, game.GetPlayerInfo().GetPlayerPos()) < 600 && !game.GetPlayerInfo().GetPlayerLife())
            {
                //Look at player to check if its dodging left or right
                if(game.GetPlayerInfo().GetPlayerPos().x < gameObject.transform.position.x && direction == 1)
                {
                    SetDirection(-1);
                }
                else if (game.GetPlayerInfo().GetPlayerPos().x > gameObject.transform.position.x && direction == -1)
                {
                    SetDirection(1);
                }

                //Stop moving to aim
                if (gameObject.GetComponent<Animator>().GetBool("EnemyMoving"))
                    gameObject.GetComponent<Animator>().SetBool("EnemyMoving", false);

                //Fire the gun if ready
                if (fireGun)
                {
                    gameObject.GetComponent<Animator>().SetTrigger("EnemyShooting");

                    //Spawn enemy bullet
                    GenerateBullet();
                }
            }
            else
            {
                //If player is not nearby or dead resume patrolling
                if(!gameObject.GetComponent<Animator>().GetBool("EnemyMoving"))
                    gameObject.GetComponent<Animator>().SetBool("EnemyMoving", true);

                //Provided movements
                x = x + .4f * direction;
                if ((direction == 1 && x > 600) || (direction == -1 && x < 480))
                {
                    SetDirection(-direction);
                }
            }
        }
        else
        {
            //If enemy is dead collision is disabled
            if(gameObject.GetComponent<CapsuleCollider2D>().enabled)
                gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
        }

        //------------------------------------------------------------

        UpdatePos();


        return isOK;

    }


    void UpdatePos() {

        gfx.SetPos(gameObject, x, y);

    }



    void SetDirection(int inDirection) {

        direction = inDirection;
        gfx.SetDirX(gameObject, direction);

    }



    public override void Kill() {
       
    }

    //Method to generate bullet created from enemy
    void GenerateBullet()
    {
        //Set gun to reload
        cooldown = 0;
        fireGun = false;
        snd.PlayAudioClip("Gun");
        game.SpawnBullet("Enemy", gameObject.transform.position + new Vector3(0, gameObject.GetComponent<SpriteRenderer>().size.y * 3), (int)gameObject.transform.localScale.x);
    }
}