using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*****************************************************************************
 * IMPORTANT NOTES - PLEASE READ
 * 
 * This is where all the code needed for the Ghost AI goes. There should not
 * be any other place in the code that needs your attention.
 * 
 * There are several sets of variables set up below for you to use. Some of
 * those settings will do much to determine how the ghost behaves. You don't
 * have to use this if you have some other approach in mind. Other variables
 * are simply things you will find useful, and I am declaring them for you
 * so you don't have to.
 * 
 * If you need to add additional logic for a specific ghost, you can use the
 * variable ghostID, which is set to 1, 2, 3, or 4 depending on the ghost.
 * 
 * Similarly, set ghostID=ORIGINAL when the ghosts are doing the "original" 
 * PacMan ghost behavior, and to CUSTOM for the new behavior that you supply. 
 * Use ghostID and ghostMode in the Update() method to control all this.
 * 
 * You could if you wanted to, create four separate ghost AI modules, one per
 * ghost, instead. If so, call them something like BlinkyAI, PinkyAI, etc.,
 * and bind them to the correct ghost prefabs.
 * 
 * Finally there are a couple of utility routines at the end.
 * 
 * Please note that this implementation of PacMan is not entirely bug-free.
 * For example, player does not get a new screenful of dots once all the
 * current dots are consumed. There are also some issues with the sound 
 * effects. By all means, fix these bugs if you like.
 * 
 *****************************************************************************/

public class GhostAI : MonoBehaviour {

    const int BLINKY = 1;   // These are used to set ghostID, to facilitate testing.
    const int PINKY = 2;
    const int INKY = 3;
    const int CLYDE = 4;
    public int ghostID;     // This variable is set to the particular ghost in the prefabs,

    const int ORIGINAL = 1; // These are used to set ghostMode, needed for the assignment.
    const int CUSTOM = 2;
    public int ghostMode;   // ORIGINAL for "original" ghost AI; CUSTOM for your unique new AI

    Movement move;
    private Vector3 startPos;
    private bool[] dirs = new bool[4];
	private bool[] prevDirs = new bool[4];

    public bool alter_Behvior = false;

    public float fleeTime = 0f;
    private float fleeTimeReset = 10f;
    public float releaseTime = 0f;          // This could be a tunable number
	private float releaseTimeReset = 1f;
	public float waitTime = 0f;             // This could be a tunable number
    private const float ogWaitTime = .1f;
	public int range = 0;                   // This could be a tunable number

    public bool dead = false;               // state variables
	public bool fleeing = false;

	//Default: base value of likelihood of choice for each path
	public float Dflt = 1f;

	//Available: Zero or one based on whether a path is available
	int A = 0;

	//Value: negative 1 or 1 based on direction of pacman
	int V = 1;

	//Fleeing: negative if fleeing
	int F = 1;

	//Priority: calculated preference based on distance of target in one direction weighted by the distance in others (dist/total)
	float P = 0f;

    // Variables to hold distance calcs
	float distX = 0f;
	float distY = 0f;
	float total = 0f;

    float flankx = 0f;
    float flanky = 0f;
    // Percent chance of each coice. order is: up < right < 0 < down < left for random choice
    // These could be tunable numbers. You may or may not find this useful.
    public float[] directions = new float[4];
    
	//remember previous choice and make inverse illegal!
	private int[] prevChoices = new int[4]{1,1,1,1};

    // This will be PacMan when chasing, or Gate, when leaving the Pit
	public GameObject target;
	GameObject gate;
	GameObject pacMan;

	public bool chooseDirection = true;
	public int[] choices ;
	public float choice;

    public float TurnTimer = 0f;

    private static Vector2 up = new Vector2(0f, 1f);
    private static Vector2 down = new Vector2(0f, -1f);
    private static Vector2 right = new Vector2(1f, 0f);
    private static Vector2 left = new Vector2(-1f, 0f);


    public enum State{
		waiting,
		entering,
		leaving,
		active,
		fleeing,
        scatter         // Optional - This is for more advanced ghost AI behavior
	}

	public State _state = State.waiting;

    // Use this for initialization
    private void Awake()
    {
        startPos = this.gameObject.transform.position;
    }

    void Start () {
		move = GetComponent<Movement> ();
		gate = GameObject.Find("Gate(Clone)");
		pacMan = GameObject.Find("PacMan(Clone)") ? GameObject.Find("PacMan(Clone)") : GameObject.Find("PacMan 1(Clone)");
		releaseTimeReset = releaseTime;
	}

	public void restart(){
		releaseTime = releaseTimeReset;
		transform.position = startPos;
		_state = State.waiting;
	}
	
    public void Chase(float target_x, float target_y)
    {
        //Current Ghost Position
        int y = -1 * Mathf.RoundToInt(transform.position.y);
        int x = Mathf.RoundToInt(transform.position.x);
        //CHECK TURNS
        bool ahead = false;
        bool turn_right = false;
        bool turn_left = false;
        switch (move._dir)
        {
            case Movement.Direction.still:
                move._dir = Movement.Direction.left;
                return;
            case Movement.Direction.down:
                ahead = move.checkDirectionClear(down);
                turn_left = move.checkDirectionClear(right);
                turn_right = move.checkDirectionClear(left);
                break;
            case Movement.Direction.up:
                ahead = move.checkDirectionClear(up);
                turn_left = move.checkDirectionClear(left);
                turn_right = move.checkDirectionClear(right);
                break;
            case Movement.Direction.left:
                ahead = move.checkDirectionClear(left);
                turn_left = move.checkDirectionClear(down);
                turn_right = move.checkDirectionClear(up);
                break;
            case Movement.Direction.right:
                ahead = move.checkDirectionClear(right);
                turn_left = move.checkDirectionClear(up);
                turn_right = move.checkDirectionClear(down);
                break;
        }
        //Debug.Log("ahead:" + ahead);
        //Debug.Log("left:" + turn_left);
        //Debug.Log("right:" + turn_right);

        if ((ahead && turn_right) || (turn_left && ahead) || (turn_left && turn_right))
        {
            //if superpellet is in bound run randomly

            //Debug.Log("In if statement");
            //Debug.Log("ahead: " + ahead + ", turn_left: " + turn_left + ", turn_right: " + turn_right);
            float[] dists = new float[3];
            dists[0] = 999f;
            dists[1] = 999f;
            dists[2] = 999f;
            if (this.fleeing) {
                System.Random rng = new System.Random();
                if (ahead) {
                    dists[0] = (float)rng.NextDouble() * 100;
                }
                if (turn_left) {
                    dists[1] = (float)rng.NextDouble() * 100;
                }
                if (turn_right){
                    dists[2] = (float)rng.NextDouble() * 100;
                }
            } 
            else
            {
                if (ahead) {
                    switch (move._dir) {
                        case Movement.Direction.down:
                            y = -1 * Mathf.CeilToInt(transform.position.y);
                            dists[0] = Mathf.Sqrt(Mathf.Pow(target_x - x, 2) + Mathf.Pow(target_y - (y + 1), 2));
                            break;
                        case Movement.Direction.up:
                            y = -1 * Mathf.FloorToInt(transform.position.y);
                            dists[0] = Mathf.Sqrt(Mathf.Pow(target_x - x, 2) + Mathf.Pow(target_y - (y - 1), 2));
                            break;
                        case Movement.Direction.left:
                            x = Mathf.CeilToInt(transform.position.x);
                            dists[0] = Mathf.Sqrt(Mathf.Pow(target_x - (x - 1), 2) + Mathf.Pow(target_y - y, 2));
                            break;
                        case Movement.Direction.right:
                            x = Mathf.FloorToInt(transform.position.x);
                            dists[0] = Mathf.Sqrt(Mathf.Pow(target_x - (x + 1), 2) + Mathf.Pow(target_y - y, 2));
                            break;
                    }
                }
                if (turn_left) {
                    switch (move._dir) {
                        case Movement.Direction.down:
                            y = -1 * Mathf.CeilToInt(transform.position.y);
                            dists[1] = Mathf.Sqrt(Mathf.Pow(target_x - (x + 1), 2) + Mathf.Pow(target_y - y, 2));
                            break;
                        case Movement.Direction.up:
                            y = -1 * Mathf.FloorToInt(transform.position.y);
                            dists[1] = Mathf.Sqrt(Mathf.Pow(target_x - (x - 1), 2) + Mathf.Pow(target_y - y, 2));
                            break;
                        case Movement.Direction.left:
                            x = Mathf.CeilToInt(transform.position.x);
                            dists[1] = Mathf.Sqrt(Mathf.Pow(target_x - x, 2) + Mathf.Pow(target_y - (y + 1), 2));
                            break;
                        case Movement.Direction.right:
                            x = Mathf.FloorToInt(transform.position.x);
                            dists[1] = Mathf.Sqrt(Mathf.Pow(target_x - x, 2) + Mathf.Pow(target_y - (y - 1), 2));
                            break;
                    }
                }
                if (turn_right) {
                    switch (move._dir) {
                        case Movement.Direction.down:
                            y = -1 * Mathf.CeilToInt(transform.position.y);
                            dists[2] = Mathf.Sqrt(Mathf.Pow(target_x - (x - 1), 2) + Mathf.Pow(target_y - y, 2));
                            break;
                        case Movement.Direction.up:
                            y = -1 * Mathf.FloorToInt(transform.position.y);
                            dists[2] = Mathf.Sqrt(Mathf.Pow(target_x - (x + 1), 2) + Mathf.Pow(target_y - y, 2));
                            break;
                        case Movement.Direction.left:
                            x = Mathf.CeilToInt(transform.position.x);
                            dists[2] = Mathf.Sqrt(Mathf.Pow(target_x - x, 2) + Mathf.Pow(target_y - (y - 1), 2));
                            break;
                        case Movement.Direction.right:
                            x = Mathf.FloorToInt(transform.position.x);
                            dists[2] = Mathf.Sqrt(Mathf.Pow(target_x - x, 2) + Mathf.Pow(target_y - (y + 1), 2));
                            break;
                    }
                }
            }
            int index = Array.IndexOf(dists, dists.Min());
            //Debug.Log("dist:" + dists[0] + "," + dists[1] +"," + dists[2]);
            //Debug.Log("index" + index);
            switch (index)
            {
                case 0:
                    //move._dir = move._dir;
                    break;
                case 1:
                    
                    if (TurnTimer <= 0f)
                    {
                        TurnTimer = 0.2f;
                        switch (move._dir)
                        {
                            case Movement.Direction.down:
                                move._dir = Movement.Direction.right;
                                break;
                            case Movement.Direction.up:
                                move._dir = Movement.Direction.left;
                                break;
                            case Movement.Direction.left:
                                move._dir = Movement.Direction.down;
                                break;
                            case Movement.Direction.right:
                                move._dir = Movement.Direction.up;
                                break;
                        }
                    }
                    break;
                case 2:
                    if (TurnTimer <= 0f)
                    {
                        TurnTimer = 0.2f;
                        switch (move._dir)
                        {
                            case Movement.Direction.down:
                                move._dir = Movement.Direction.left;
                                break;
                            case Movement.Direction.up:
                                move._dir = Movement.Direction.right;
                                break;
                            case Movement.Direction.left:
                                move._dir = Movement.Direction.up;
                                break;
                            case Movement.Direction.right:
                                move._dir = Movement.Direction.down;
                                break;
                        }
                    }
                    
                    break;
            }
        }
        else
        {
            if(ahead != true)
            {
                TurnTimer = 0.2f;
            }
            if (move.checkDirectionClear(up) && move._dir != Movement.Direction.down) {
                move._dir = Movement.Direction.up;
            } else if (move.checkDirectionClear(left) && move._dir != Movement.Direction.right) {
                move._dir = Movement.Direction.left;
            } else if (move.checkDirectionClear(down) && move._dir != Movement.Direction.up) {
                move._dir = Movement.Direction.down;
            } else if (move.checkDirectionClear(right) && move._dir != Movement.Direction.left) {
                move._dir = Movement.Direction.right;
            }
        }
    }

    private void PriorityTurn(int[] priority) {
        //Current Ghost Position
        int y = -1 * Mathf.RoundToInt(transform.position.y);
        int x = Mathf.RoundToInt(transform.position.x);
        //CHECK TURNS
        bool ahead = false;
        bool turn_right = false;
        bool turn_left = false;
        int back = 0;
        switch (move._dir) {
            case Movement.Direction.still:
                move._dir = Movement.Direction.left;
                return;
            case Movement.Direction.down:
                ahead = move.checkDirectionClear(down);
                turn_left = move.checkDirectionClear(right);
                turn_right = move.checkDirectionClear(left);
                back = 1;
                break;
            case Movement.Direction.up:
                ahead = move.checkDirectionClear(up);
                turn_left = move.checkDirectionClear(left);
                turn_right = move.checkDirectionClear(right);
                back = 3;
                break;
            case Movement.Direction.left:
                ahead = move.checkDirectionClear(left);
                turn_left = move.checkDirectionClear(down);
                turn_right = move.checkDirectionClear(up);
                back = 2;
                break;
            case Movement.Direction.right:
                ahead = move.checkDirectionClear(right);
                turn_left = move.checkDirectionClear(up);
                turn_right = move.checkDirectionClear(down);
                back = 4;
                break;
        }

        if ((ahead && turn_right) || (turn_left && ahead) || (turn_left && turn_right)) {
            if (TurnTimer <= 0f) {
                TurnTimer = .2f;
                foreach (int i in priority) {
                    if (i != back) {
                        Vector2 target_dir = new Vector2(0f, 0f);
                        Movement.Direction dir = Movement.Direction.still;
                        switch (i) {
                            case 1:
                                target_dir = up;
                                dir = Movement.Direction.up;
                                break;
                            case 2:
                                target_dir = right;
                                dir = Movement.Direction.right;
                                break;
                            case 3:
                                target_dir = down;
                                dir = Movement.Direction.down;
                                break;
                            case 4:
                                target_dir = left;
                                dir = Movement.Direction.left;
                                break;
                        }
                        if (move.checkDirectionClear(target_dir)) {
                            move._dir = dir;
                            break;
                        }
                    }
                }
            }

            
        } else {
            if (ahead != true) {
                TurnTimer = 0.2f;
            }
            if (move.checkDirectionClear(up) && move._dir != Movement.Direction.down) {
                move._dir = Movement.Direction.up;
            } else if (move.checkDirectionClear(left) && move._dir != Movement.Direction.right) {
                move._dir = Movement.Direction.left;
            } else if (move.checkDirectionClear(down) && move._dir != Movement.Direction.up) {
                move._dir = Movement.Direction.down;
            } else if (move.checkDirectionClear(right) && move._dir != Movement.Direction.left) {
                move._dir = Movement.Direction.right;
            }
        }

    }

    /// <summary>
    /// This is where most of the work will be done. A switch/case statement is probably 
    /// the first thing to test for. There can be additional tests for specific ghosts,
    /// controlled by the GhostID variable. But much of the variations in ghost behavior
    /// could be controlled by changing values of some of the above variables, like
    /// 
    /// </summary>
	void Update () {
        if(TurnTimer > 0f)
        {
            TurnTimer -= Time.deltaTime;
        }
        if(fleeTime > 0f) {
            fleeTime -= Time.deltaTime;
        }
        if(fleeTime <= 0f) {
            fleeing = false;
        }
		switch (_state) {
		case(State.waiting):

            // below is some sample code showing how you deal with animations, etc.
			move._dir = Movement.Direction.still;
			if (releaseTime <= 0f) {
				chooseDirection = true;
				gameObject.GetComponent<Animator>().SetBool("Dead", false);
				gameObject.GetComponent<Animator>().SetBool("Running", false);
				gameObject.GetComponent<Animator>().SetInteger ("Direction", 0);
				gameObject.GetComponent<Movement> ().MSpeed = 5f;

				_state = State.leaving;

                // etc.
			}
			gameObject.GetComponent<Animator>().SetBool("Dead", false);
			gameObject.GetComponent<Animator>().SetBool("Running", false);
			gameObject.GetComponent<Animator>().SetInteger ("Direction", 0);
			gameObject.GetComponent<Movement> ().MSpeed = 5f;
			releaseTime -= Time.deltaTime;
            // etc.
			break;

		case(State.leaving):
                
                if(transform.position.y > -11.02 && transform.position.y < -10.98) {
                    this.gameObject.GetComponent<CircleCollider2D>().enabled = true;
                    move._dir = Movement.Direction.up;
                    _state = State.active;
                }else if(transform.position.x > 13.48 && transform.position.x < 13.52) {
                    transform.position = Vector3.Lerp(transform.position, new Vector3(13.5f, -11f, transform.position.z), 3f * Time.deltaTime);
                }else{
                    transform.position = Vector3.Lerp(transform.position, new Vector3(13.5f, transform.position.y, transform.position.z), 3f * Time.deltaTime);
                }
                break;

		case(State.active):
                if (dead)
                {
                    releaseTime = 2f;
                    this.gameObject.transform.position = new Vector3(13, -14, -2);
                    _state = State.entering;
                }

                //BLINKY
                if (ghostID == 1) {
                    if (alter_Behvior) {
                        // 1 -> up, 2 -> right, 3 -> down, 4 -> left
                        float target_x = pacMan.transform.position.x;
                        float target_y = -1 * pacMan.transform.position.y;
                        float dist_x = target_x - this.gameObject.transform.position.x;
                        float dist_y = target_y - Mathf.Abs(this.gameObject.transform.position.y);
                        int[] priority = new int[4]; 
                        if(Mathf.Abs(dist_x) >= Mathf.Abs(dist_y)) {
                            if(dist_x >=0) {
                                if(dist_y >= 0) {
                                    priority[0] = 2;
                                    priority[1] = 3;
                                    priority[2] = 1;
                                    priority[3] = 4;
                                } else {
                                    priority[0] = 2;
                                    priority[1] = 1;
                                    priority[2] = 3;
                                    priority[3] = 4;
                                }
                            } else {
                                if (dist_y >= 0) {
                                    priority[0] = 4;
                                    priority[1] = 3;
                                    priority[2] = 1;
                                    priority[3] = 2;
                                } else {
                                    priority[0] = 4;
                                    priority[1] = 1;
                                    priority[2] = 3;
                                    priority[3] = 2;
                                }
                            }
                        } else {
                            if (dist_y >= 0) {
                                if (dist_x >= 0) {
                                    priority[0] = 3;
                                    priority[1] = 2;
                                    priority[2] = 4;
                                    priority[3] = 1;
                                } else {
                                    priority[0] = 3;
                                    priority[1] = 4;
                                    priority[2] = 2;
                                    priority[3] = 1;
                                }
                            } else {
                                if (dist_x >= 0) {
                                    priority[0] = 1;
                                    priority[1] = 2;
                                    priority[2] = 4;
                                    priority[3] = 3;
                                } else {
                                    priority[0] = 1;
                                    priority[1] = 4;
                                    priority[2] = 2;
                                    priority[3] = 3;
                                }
                            }
                        }
                        PriorityTurn(priority);

                    } else {
                    //CHASE
                    Chase(pacMan.transform.position.x, -1 * pacMan.transform.position.y);
                    }   
                }


                //PINKY

                else if(ghostID == 2)
                {
                    if (alter_Behvior) {//alternating behavior here
                        //GET TARGET
                        float pac_x = pacMan.transform.position.x;
                        float pac_y = -1 * pacMan.transform.position.y;
                        Vector3 target = new Vector3(pac_x, pac_y, pacMan.transform.position.z);

                        float dist = (target - new Vector3(gameObject.transform.position.x, -gameObject.transform.position.y, gameObject.transform.position.z)).magnitude;
                        if (dist >= 8f)
                        {
                            pac_x = pacMan.transform.position.x;
                            pac_y = -1 * pacMan.transform.position.y;
                            flankx = pac_x;
                            flanky = pac_y;
                        }
                        else
                        {
                            if (flankx == pac_x && flanky == pac_y)
                            {
                                flanky = pac_y + 4;
                            }
                            else
                            {
                                float flankDist = (new Vector3(flankx, flanky, pacMan.transform.position.z) - new Vector3(gameObject.transform.position.x, -gameObject.transform.position.y, gameObject.transform.position.z)).magnitude;
                                if (flankDist <= 2f && flankx == pac_x && flanky == pac_y + 4)
                                {
                                    flankx = pac_x + 2;
                                    flanky = pac_y;
                                }
                                else if (flankDist <= 4f && flankx == pac_x + 4 && flanky == pac_y)
                                {
                                    flankx = pac_x;
                                    flanky = pac_y - 2;
                                }
                                else if (flankDist <= 4f && flankx == pac_x && flanky == pac_y - 4)
                                {
                                    flankx = pac_x - 2;
                                    flanky = pac_y;
                                }
                                else if (flankDist <= 4f && flankx == pac_x - 4 && flanky == pac_y)
                                {
                                    flankx = pac_x + 2;
                                    flanky = pac_y;
                                }
                            }
                        }
                        //CHASE
                        Chase(flankx, flanky);


                    } else {
                        //GET TARGET
                        float target_x = pacMan.transform.position.x;
                        float target_y = -1 * pacMan.transform.position.y;
                        switch (pacMan.GetComponent<Movement>()._dir) {
                            case Movement.Direction.up:
                                target_x = pacMan.transform.position.x - 4;
                                target_y = - pacMan.transform.position.y - 4;
                                break;
                            case Movement.Direction.down:
                                target_x = pacMan.transform.position.x;
                                target_y = -pacMan.transform.position.y + 4;
                                break;
                            case Movement.Direction.left:
                                target_x = pacMan.transform.position.x - 4;
                                target_y = -pacMan.transform.position.y;
                                break;
                            case Movement.Direction.right:
                                target_x = pacMan.transform.position.x + 4;
                                target_y = -pacMan.transform.position.y;
                                break;

                        }

                        //CHASE
                        Chase(target_x, target_y);
                    }
                }
                else if (ghostID == 3)
                {
                    if (alter_Behvior) {//alternating behavior here




                    } else {
                        //GET TARGET
                        float target_x = pacMan.transform.position.x;
                        float target_y = -1 * pacMan.transform.position.y;
                        switch (pacMan.GetComponent<Movement>()._dir) {
                            case Movement.Direction.up:
                                target_x = pacMan.transform.position.x - 1;
                                target_y = pacMan.transform.position.y + 1;
                                break;
                            case Movement.Direction.down:
                                target_x = pacMan.transform.position.x;
                                target_y = pacMan.transform.position.y - 1;
                                break;
                            case Movement.Direction.left:
                                target_x = pacMan.transform.position.x - 1;
                                target_y = pacMan.transform.position.y;
                                break;
                            case Movement.Direction.right:
                                target_x = pacMan.transform.position.x + 1;
                                target_y = pacMan.transform.position.y;
                                break;

                        }
                        GameObject Red = GameObject.Find("Blinky(Clone)") ? GameObject.Find("Blinky(Clone)") : GameObject.Find("Blinky 1(Clone)");
                        Vector2 direction = new Vector2(target_x, target_y) - new Vector2(Red.transform.position.x, Red.transform.position.y);
                        target_x += direction.x;
                        target_y += direction.y;
                        //CHASE
                        Chase(target_x, target_y);
                    }
                } else if (ghostID == 4)
                {
                    if (alter_Behvior) {//alternating behavior here
                        

                    } else {
                        //GET TARGET
                        float target_x = pacMan.transform.position.x;
                        float target_y = -1 * pacMan.transform.position.y;
                        Vector3 target = new Vector3(target_x, target_y, pacMan.transform.position.x);
                        if ((target - new Vector3(gameObject.transform.position.x, -gameObject.transform.position.y, gameObject.transform.position.z)).magnitude >= 8f) {
                            target_x = pacMan.transform.position.x;
                            target_y = pacMan.transform.position.y;
                        } else {
                            target_x = 1f;
                            target_y = -32f;
                        }
                        //CHASE
                        Chase(target_x, target_y);
                    }
                }

                break;

        //Ghost respawn
		case State.entering:

            // Leaving this code in here for you.
			move._dir = Movement.Direction.still;

			if (transform.position.x < 13.48f || transform.position.x > 13.52) {
				//print ("GOING LEFT OR RIGHT");
				transform.position = Vector3.Lerp (transform.position, new Vector3 (13.5f, transform.position.y, transform.position.z), 3f * Time.deltaTime);
			} else if (transform.position.y > -13.99f || transform.position.y < -14.01f) {
				gameObject.GetComponent<Animator>().SetInteger ("Direction", 2);
				transform.position = Vector3.Lerp (transform.position, new Vector3 (transform.position.x, -14f, transform.position.z), 3f * Time.deltaTime);
			} else {
				fleeing = false;
				dead = false;
				gameObject.GetComponent<Animator>().SetBool("Running", true);
				_state = State.waiting;
			}

            break;
		}
	}

    // Utility routines

	Vector2 num2vec(int n){
        switch (n)
        {
            case 0:
                return new Vector2(0, 1);
            case 1:
    			return new Vector2(1, 0);
		    case 2:
			    return new Vector2(0, -1);
            case 3:
			    return new Vector2(-1, 0);
            default:    // should never happen
                return new Vector2(0, 0);
        }
	}

	bool compareDirections(bool[] n, bool[] p){
		for(int i = 0; i < n.Length; i++){
			if (n [i] != p [i]) {
				return false;
			}
		}
		return true;
	}
}
