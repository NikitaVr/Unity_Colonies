using UnityEngine;
using System.Collections;

public class BlockModification : MonoBehaviour {

    public GameObject playerCameraObject;
    public Camera playerCamera;
    public GameObject player;
    public BoxCollider2D playerCollider;
    public LayerMask mask;
    public GameObject terrainGen;
    public TerrainGenerator terrainGenScript;

    // Use this for initialization
    void Start () {
        //BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>();
        //Physics2D.IgnoreCollision()
        //LayerMask mask = 8; 
        playerCamera = playerCameraObject.GetComponent<Camera>();
        terrainGenScript = terrainGen.GetComponent<TerrainGenerator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseWorld = playerCamera.ScreenToWorldPoint(mousePos);
            //mouseWorld.z = 0;

            Vector2 playerPosition = player.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(playerPosition, mouseWorld - playerPosition,2,mask);


            if(hit != false && hit.collider != null)
            {
                int xPos = Mathf.FloorToInt(hit.transform.position.x);
                int yPos = Mathf.FloorToInt(hit.transform.position.y);
                terrainGenScript.removeBlock(xPos, yPos);

                
            }

           
        }
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseWorld = playerCamera.ScreenToWorldPoint(mousePos);
            //mouseWorld.z = 0;
            Vector2 playerPosition = player.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(playerPosition, mouseWorld - playerPosition, 2, mask);


            if (hit != false && hit.collider != null)
            {

                int xPos = Mathf.FloorToInt(hit.transform.position.x);
                int yPos = Mathf.FloorToInt(hit.transform.position.y);
                //int xPos = Mathf.RoundToInt(hit.point.x);
                //int yPos = Mathf.RoundToInt(hit.point.y);
                int blockType = 1;
                Debug.Log(hit.point);
                if (hit.point.x - hit.transform.position.x == -0.5)
                {
                    xPos = (int)((float)hit.point.x - 0.5);
                }
                if (hit.point.x - hit.transform.position.x == 0.5)
                {
                    xPos = (int)((float)hit.point.x + 0.5);
                }

                if (hit.point.y - hit.transform.position.y == -0.5)
                {
                    yPos = (int)((float)hit.point.y - 0.5);
                }
                if (hit.point.y - hit.transform.position.y == 0.5)
                {
                    yPos = (int)((float)hit.point.y + 0.5);
                }
                terrainGenScript.createBlock(xPos, yPos,blockType);


            }


        }
    }
}
