using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static int FIELD_SIZE = 30; // Width and height of the game field
    public static float COLLISION_THRESHOLD = 1.5f; // Collision distance between food and player 
    
    public GameObject playerObject; // Reference to the Player GameObject
    private float score; // Count the player collections of food

    private GameObject food; // Represents the food in the game

    public GameObject cameraObject; // Ref to the camera
    // Private variable to store the offset distance between the player and camera
    private Vector3 offset;       

    // Start is called before the first frame update
    void Start()
    {
        food = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        score = 0f;

        // Calculate and store the offset value by getting the distance between 
        // the player's position and camera's position.
        offset = cameraObject.transform.position - playerObject.transform.position;
    }

    // Positions the food at a random location inside the field
    void SpawnFood()
    {
        food.transform.position = new Vector3(Random.Range(-14.5f, 14.5f), 
                                    Random.Range(0.0f, 1.5f), Random.Range(-14.5f, 14.5f));
    }

    // Update is called once per frame
    void Update()
    {
        if (playerObject)
        {
            float dist = Vector3.Distance(playerObject.transform.position, food.transform.position);
            if (COLLISION_THRESHOLD > dist)
            {
                score++;
                Debug.Log("Player has collected food.");

                SpawnFood();
            }
        }
    }

    // LateUpdate is called after Update each frame
    void LateUpdate () 
    {
        // Set the position of the camera's transform to be the same as the player's, 
        // but offset by the calculated offset distance.
        cameraObject.transform.position = playerObject.transform.position + offset;
    }
}
