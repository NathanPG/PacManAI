using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class pelletCollision : MonoBehaviour {

    public GameObject clyde;
    public GameObject pinky;
    public GameObject blinky;
    public GameObject inky; 

    private GameObject gameManager;

    public AudioClip pellet;
    public AudioClip eatGhost;

    AudioSource aud;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager");
		clyde = GameObject.Find("Clyde(Clone)") ? GameObject.Find("Clyde(Clone)") : GameObject.Find("Clyde 1(Clone)");
		pinky = GameObject.Find("Pinky(Clone)") ? GameObject.Find("Pinky(Clone)"): GameObject.Find("Pinky 1(Clone)");
		inky = GameObject.Find("Inky(Clone)") ? GameObject.Find("Inky(Clone)"): GameObject.Find("Inky 1(Clone)");
		blinky = GameObject.Find("Blinky(Clone)") ? GameObject.Find("Blinky(Clone)"): GameObject.Find("Blinky 1(Clone)");
        aud = GetComponent<AudioSource>();
        aud.clip = pellet;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if (collision.tag == "pellet") {
            aud.Play();
			Destroy (collision.gameObject);
			gameManager.SendMessage("updateScore");
		}

        if (collision.tag == "powerpellet")
        {
            aud.Play();
            Destroy(collision.gameObject);
            gameManager.SendMessage("updateState");
            for(int i = 0; i < 5; i++)
            {
                gameManager.SendMessage("updateScore");
            }
            clyde.GetComponent<Animator>().SetBool("Running", true);
            pinky.GetComponent<Animator>().SetBool("Running", true);
            inky.GetComponent<Animator>().SetBool("Running", true);
            blinky.GetComponent<Animator>().SetBool("Running", true);

            //set ghosts to flee. 
			clyde.GetComponent<GhostAI>().fleeing = true;
			pinky.GetComponent<GhostAI>().fleeing = true;
			inky.GetComponent<GhostAI>().fleeing = true;
			blinky.GetComponent<GhostAI>().fleeing = true;

			clyde.GetComponent<GhostAI>().chooseDirection = true;
			pinky.GetComponent<GhostAI>().chooseDirection = true;
			inky.GetComponent<GhostAI>().chooseDirection = true;
			blinky.GetComponent<GhostAI>().chooseDirection = true;

			if (!clyde.GetComponent<GhostAI> ().dead) {
                Movement move = clyde.GetComponent < Movement >();
                switch (move._dir) {
                    case Movement.Direction.down:
                        move._dir = Movement.Direction.up;
                        break;
                    case Movement.Direction.up:
                        move._dir = Movement.Direction.down;
                        break;
                    case Movement.Direction.left:
                        move._dir = Movement.Direction.right;
                        break;
                    case Movement.Direction.right:
                        move._dir = Movement.Direction.left;
                        break;
                }
				move.MSpeed = 3f;
                clyde.GetComponent<GhostAI>().fleeTime = 10f;

            }
			if (!pinky.GetComponent<GhostAI> ().dead) {
                Movement move = pinky.GetComponent<Movement>();
                switch (move._dir) {
                    case Movement.Direction.down:
                        move._dir = Movement.Direction.up;
                        break;
                    case Movement.Direction.up:
                        move._dir = Movement.Direction.down;
                        break;
                    case Movement.Direction.left:
                        move._dir = Movement.Direction.right;
                        break;
                    case Movement.Direction.right:
                        move._dir = Movement.Direction.left;
                        break;
                }
                move.MSpeed = 3f;
                pinky.GetComponent<GhostAI>().fleeTime = 10f;

            }
			if (!inky.GetComponent<GhostAI> ().dead) {
                Movement move = inky.GetComponent<Movement>();
                switch (move._dir) {
                    case Movement.Direction.down:
                        move._dir = Movement.Direction.up;
                        break;
                    case Movement.Direction.up:
                        move._dir = Movement.Direction.down;
                        break;
                    case Movement.Direction.left:
                        move._dir = Movement.Direction.right;
                        break;
                    case Movement.Direction.right:
                        move._dir = Movement.Direction.left;
                        break;
                }
                move.MSpeed = 3f;
                inky.GetComponent<GhostAI>().fleeTime = 10f;

            }
			if (!blinky.GetComponent<GhostAI> ().dead) {
                Movement move = blinky.GetComponent<Movement>();
                switch (move._dir) {
                    case Movement.Direction.down:
                        move._dir = Movement.Direction.up;
                        break;
                    case Movement.Direction.up:
                        move._dir = Movement.Direction.down;
                        break;
                    case Movement.Direction.left:
                        move._dir = Movement.Direction.right;
                        break;
                    case Movement.Direction.right:
                        move._dir = Movement.Direction.left;
                        break;
                }
                move.MSpeed = 3f;
                blinky.GetComponent<GhostAI>().fleeTime = 10f;

            }
        }
        

        if (collision.CompareTag ("ghost")) {
			gameManager.GetComponent<scoreManager>().updateLives(collision);

			if (gameManager.GetComponent<scoreManager>().powerPellet && collision.GetComponent<GhostAI>().fleeing)
            {
                collision.GetComponent<Animator>().SetBool("Dead", true);
				collision.GetComponent<GhostAI> ().dead = true;
				collision.GetComponent<GhostAI> ().fleeing = false;
				collision.GetComponent<Movement> ().MSpeed = 7.5f;
				collision.gameObject.GetComponent<CircleCollider2D> ().enabled = false;
                StartCoroutine("EatGhost");
                //set state to path find back to start
            }
        }
    }

    IEnumerator EatGhost()
    {
        Time.timeScale = 0f;
        aud.clip = eatGhost;
        aud.Play();
        yield return new WaitForSecondsRealtime(.8f);
        aud.clip = pellet;
        Time.timeScale = 1f;
    }

}
