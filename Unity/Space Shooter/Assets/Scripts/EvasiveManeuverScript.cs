using UnityEngine;
using System.Collections;

public class EvasiveManeuverScript : MonoBehaviour {

    public float smoothing;
    public float dodge;
    public float tilt;
    public Vector2 startWait;
    public Vector2 maneuverTime;
    public Vector2 maneuverWait;
    public Boundary boundary;
    private Transform playerTransform;

    private float targetManeuver;
    private Rigidbody rb;
    private float currentSpeed;

	// Use this for initialization
	void Start ()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        rb = GetComponent<Rigidbody>();
        StartCoroutine(Evade());
        currentSpeed = rb.velocity.z;
	}

    IEnumerator Evade()
    {
        yield return new WaitForSeconds(Random.Range(startWait.x, startWait.y));

        while(true)
        {
            if(playerTransform)
                targetManeuver = playerTransform.position.x;
            else
                targetManeuver = Random.Range(1, dodge * -Mathf.Sign(transform.position.x));

            yield return new WaitForSeconds(Random.Range(maneuverTime.x, maneuverTime.y));
            targetManeuver = 0;
            yield return new WaitForSeconds(Random.Range(maneuverWait.x, maneuverWait.y));
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        float newManeuver = Mathf.MoveTowards(rb.velocity.x, targetManeuver, Time.deltaTime * smoothing);

        rb.velocity = new Vector3(newManeuver, 0.0f, currentSpeed);

        rb.position = new Vector3(
            Mathf.Clamp(rb.position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(rb.position.z, boundary.zMin, boundary.zMax)
        );

        rb.rotation = Quaternion.Euler(0.0f, 0.0f, rb.velocity.x * -tilt);

	}
}
