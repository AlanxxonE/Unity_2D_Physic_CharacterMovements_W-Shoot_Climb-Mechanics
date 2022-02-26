using UnityEngine;
using System.Collections.Generic;

public class Game : MonoBehaviour {

    Main main;
    int myRes;
    Gfx gfx;
    Snd snd;

    static string PLAY = "play";

    string gameStatus;

    int   camWidth;
    int   camHeight;
    float camX;
    float camY;

    Player player;
    GameObject bulletObject; //Var for Instantiate the Bullets

    float playerHorizontal, playerVertical; //Vars for detect player movements
    bool playerShoot; //Var to check if player fired the gun

    bool playerAim = false; //Var to check if player is aiming

    List<GeneralObject> gameObjects;
    int gameObjectLength;

    public void Init(Main inMain) {

        main  = inMain;
        gfx   = main.gfx;
        myRes = gfx.myRes;
        snd   = main.snd;

        camWidth  = gfx.screenWidth / myRes;
        camHeight = gfx.screenHeight / myRes;

        gameObjects = new List<GeneralObject>();
        gameObjectLength = 0;

        player = new Player(main);

        AddLevelObject(new Enemy(main, 600, 560));
        AddLevelObject(new Enemy(main, 480, 624));

        //Load the bullet asset from resources
        bulletObject = Resources.Load<GameObject>("Props/Bullet");

        gameStatus  = PLAY;

    }

    public Player GetPlayerInfo()
    {
        return player;
    }

    private void FixedUpdate()
    {
        if (gameStatus == PLAY)
        {
            //Run in FixedUpdate to stabilise Input Framerate
            GoPlayer();
        }
    }

    void Update() {
       
        if (gameStatus==PLAY) {

            GoKeys();

            GoCam();

            GoObjects();
        }

    }



    void GoPlayer()
    {
        player.FrameEvent(playerHorizontal, playerVertical, playerAim, playerShoot);

        //Reset var after player fired
        if (playerShoot)
            playerShoot = false;
    }



    private void GoKeys() {
       
        // ---------------------------------------------------------------
        // KEYBOARD AND CONTROLLER INPUTS
		// ---------------------------------------------------------------

        playerHorizontal = Input.GetAxisRaw("Horizontal");

        if(Input.GetButtonDown("Jump"))
        {
            if (playerVertical != 1)
                playerVertical = 1;
        }
        else if (Input.GetAxisRaw("Vertical") < 0)
        {
            if (playerVertical != -1) 
                playerVertical = -1;
        }
        else
        {
            if (playerVertical != 0)
                playerVertical = 0;
        }

        if(Input.GetButton("Aim") || Input.GetAxisRaw("Aim") > 0)
        {
            playerAim = true;
        }
        else
        {
            playerAim = false;
        }

        if(Input.GetButtonDown("Fire1"))
        {
            playerShoot = true;
        }

        if(Input.GetButton("Controls"))
        {
            if (gfx.canvas.transform.GetChild(1).gameObject.GetComponent<TMPro.TMP_Text>().color.a <= 1)
            {
                gfx.canvas.transform.GetChild(1).gameObject.GetComponent<TMPro.TMP_Text>().color += new Color(0, 0, 0, Time.deltaTime);
            }
        }
        else
        {
            if (gfx.canvas.transform.GetChild(1).gameObject.GetComponent<TMPro.TMP_Text>().color.a >= 0)
            {
                gfx.canvas.transform.GetChild(1).gameObject.GetComponent<TMPro.TMP_Text>().color -= new Color(0, 0, 0, Time.deltaTime);
            }
        }

        if(Input.GetButtonDown("Cancel"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }


    
    void GoCam() {

        camX = 480 - camWidth/2;
        camY = 600 - camHeight/2;

        gfx.MoveLevel(camX, camY);

        // ---------------------------------------------------------------
        // LEVEL PARALLAX EFFECT
        // ---------------------------------------------------------------

        gfx.particleEffect.transform.position = new Vector3((main.cam.transform.position.x * 0.3f), gfx.particleEffect.transform.position.y, gfx.particleEffect.transform.position.z);

    }



    public void AddLevelObject(GeneralObject inObj) {

        gameObjects.Add(inObj);
        gameObjectLength++;

    }



    void GoObjects(bool inDoActive=true) {

        for (int i = 0; i<gameObjectLength; i++) {

            if (!gameObjects[i].FrameEvent()) {
                gameObjects.RemoveAt(i);
                i--;
                gameObjectLength--;
            }
        }

    }

    public void SpawnBullet(string owner, Vector3 spawnPos, int direction)
    {
        // ---------------------------------------------------------------
        // GENERATE BULLET
        // ---------------------------------------------------------------

        GameObject go = Instantiate(bulletObject, spawnPos, Quaternion.identity);
        go.GetComponent<Bullet>().SetBullet(owner, direction, 20);
    }

}