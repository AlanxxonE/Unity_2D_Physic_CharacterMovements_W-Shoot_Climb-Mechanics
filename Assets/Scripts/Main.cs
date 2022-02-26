using UnityEngine;

public class Main : MonoBehaviour {

	public Game game;
	public Gfx gfx;
    public Snd snd;
    public Camera cam;
    public GameObject go;

    public int screenWidth;
    public int screenHeight;
	

    void Start() {

        screenWidth  = Screen.width;
        screenHeight = Screen.height;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Application.targetFrameRate = 60;
        Cursor.visible = false;
          
        QualitySettings.vSyncCount = 1;
        if (Screen.currentResolution.refreshRate > 65) {
            QualitySettings.vSyncCount = Mathf.FloorToInt(Screen.currentResolution.refreshRate/60);
        }
        
        cam = GetComponent<Camera>();

        go = new GameObject("Main");

        StartGfx();
        StartSnd();

        StartGame();
             
    }

   
    public void Trace(string inString) {

		Debug.Log ("Trace:"+inString);

	}


    void StartGfx() {

        gfx = go.AddComponent<Gfx>();
        gfx.Init(this);

    }

    public void StartSnd() {

        snd = go.AddComponent<Snd>();
        snd.Init(this);

    }

    public void StartGame() {

        game = go.AddComponent<Game>();
        game.Init(this);

    }

    public void EndGame() {

        Destroy(go.GetComponent<Game>());
        game = null;

    }


}