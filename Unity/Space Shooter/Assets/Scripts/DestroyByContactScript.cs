using UnityEngine;
using System.Collections;

public class DestroyByContactScript : MonoBehaviour {

    public GameObject explosion;
    public GameObject playerExplosion;
    public int scoreValue;
    private GameControllerScript gameController;

    void Start()
    {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");

        if (gameControllerObject)
            gameController = gameControllerObject.GetComponent<GameControllerScript>();

        if (!gameController)
            Debug.Log("Cannot find 'GameControllerScript' script");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Boundary" || other.CompareTag("Enemy"))
            return;

        
        if(explosion)
            Instantiate(explosion, transform.position, transform.rotation);

        if (other.tag == "Player")
        {
            Instantiate(playerExplosion, transform.position, transform.rotation);
            gameController.GameOver();
        }

        gameController.AddScore(scoreValue);

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
