using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movementForce = 500f; // Controls player movement power
    private Rigidbody body; // Enable the player object interact 
                            // with the rest of world, 
                            // as well as obeying to Physics laws


    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Implement movement logic here

        //Detect when the up arrow key is pressed down
        if (Input.GetKeyDown(KeyCode.UpArrow)) 
        {
            // Debug.Log("Up Arrow key was pressed.");

            body.AddForce(Vector3.forward * movementForce * Time.deltaTime);
        }

        //Detect when the down arrow key is pressed down
        if (Input.GetKeyDown(KeyCode.DownArrow)) 
        {
            // Debug.Log("Down Arrow key was pressed.");

            body.AddForce(Vector3.back * movementForce * Time.deltaTime);
        }

        //Detect when the right arrow key is pressed down
        if (Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            // Debug.Log("Right Arrow key was pressed.");

            body.AddForce(Vector3.right * movementForce * Time.deltaTime);
        }

        //Detect when the left arrow key is pressed down
        if (Input.GetKeyDown(KeyCode.LeftArrow)) 
        {
            // Debug.Log("Left Arrow key was pressed.");

            body.AddForce(Vector3.left * movementForce * Time.deltaTime);
        }
    }
}
