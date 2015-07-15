using UnityEngine;
using System.Collections;
// allows for use of lists:
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{

    // Setting up all the required variables
    public Sprite sprite;
    public Sprite sprite2;
    public Sprite sprite3;
    public List<List<int>> blocks;
    //public List<List<List<List<GameObject>>>> superBlocks;
    public GameObject[,,,] superBlocks;
    private List<int> column;
    private GameObject[] blockColumn;
    //private List<List<List<GameObject>>> superColumn;
    private GameObject[,,] superColumn;
    //public List<List<GameObject>> superBlock;
    public GameObject[,] superBlock;
    //public byte[,] superblocks;
    public int xblocks;
    public int yblocks;
    public int superBlockSize;
    public GameObject player;
    public int playerSuperX;
    public int playerSuperY;
    public int lastPlayerSuperX;
    public int lastPlayerSuperY;



    // Function runs when object is first created (start of game)
    void Start()
    {
        // Run the functions required for terrain
        GenSuperBlocks();
        GenTerrain();
        RenderTerrain();
        lastPlayerSuperX = Mathf.FloorToInt(player.transform.position.x / superBlockSize);
        lastPlayerSuperY = Mathf.FloorToInt(player.transform.position.y / superBlockSize);

    }


    // function for creating terrain noise for varying terrain, returns an int
    int Noise(int x, int y, float scale, float mag, float exp)
    {
        return (int)(Mathf.Pow(Mathf.PerlinNoise(x / scale, y / scale) * mag, (exp)));
    }


    // function for creating the larger super blocks, each on of which will contain a certain number of blocks
    void GenSuperBlocks()
    {
        //superBlocks = new List<List<List<List<GameObject>>>>(xblocks / superBlockSize);
        superBlocks = new GameObject[xblocks/superBlockSize,yblocks/superBlockSize,superBlockSize,superBlockSize];
        //Debug.Log(superBlocks.Count);

        for (int px = 0; px < xblocks / superBlockSize; px++)
        {
            //superColumn = new List<List<List<GameObject>>>(yblocks / superBlockSize);
            //superColumn = new GameObject[yblocks/superBlockSize][][];
            //superBlocks.Add(superColumn);
            //superBlocks[px] = superColumn;
            for (int py = 0; py < yblocks / superBlockSize; py++)
            {

                //superBlock = new List<List<GameObject>>(superBlockSize);
                //superBlock = new GameObject[superBlockSize][];
                //superColumn.Add(superBlock);
                //superColumn[py] = superBlock;
            }
        }

    }

    //Creates lists of purely numbers that contain the block info
    void GenTerrain()
    {

        //chunks = new byte[10, 10];

        //blocks = new byte[xblocks, yblocks];
        blocks = new List<List<int>>();



        //GameObject chunk = null;

        for (int px = 0; px < xblocks; px++)
        {

            column = new List<int>();
            blocks.Add(column);
            for (int py = 0; py < yblocks; py++)
            {



                // using perlin noise for block info, still working on this
                column.Add(Noise(px, py, 2, 15, 1));






            }
        }
    }


    // generates the gameobjects for the terrain sprites, splits them into superblocks, and desides which ones are turned on or off
    void RenderTerrain()
    {

        playerSuperX = Mathf.FloorToInt(player.transform.position.x / superBlockSize);
        playerSuperY = Mathf.FloorToInt(player.transform.position.y / superBlockSize);

       

            for (int superX = playerSuperX - 1; superX < playerSuperX + 2; superX++)
            {


                for (int superY = playerSuperY - 1; superY < playerSuperY + 2; superY++)
                {
                    Debug.Log(superY);

                    if (superBlocks[superX, superY, 0, 0] == null)
                    {

                        for (int px = 0; px < superBlockSize; px++)
                        {
                            for (int py = 0; py < superBlockSize; py++)
                            {

                                GameObject go = new GameObject("New Sprite");
                                go.layer = 9;
                                //int health = go.AddComponent<int>();

                                //go.SetActive(false);
                                superBlocks[superX, superY, px, py] = go;

                                int blocksX = superX * superBlockSize + px;
                                int blocksY = superY * superBlockSize + py;
                                go.transform.position = new Vector2(blocksX, blocksY);

                                if (blocks[blocksX][blocksY] == 6)
                                {


                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite;
                                    go.AddComponent<BoxCollider2D>();

                                }
                                if (blocks[blocksX][blocksY] == 7)
                                {


                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite2;
                                    go.AddComponent<BoxCollider2D>();

                                }

                                if (blocks[blocksX][blocksY] == 8)
                                {


                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite3;
                                    go.AddComponent<BoxCollider2D>();

                                }
                            }
                        }
                    }
                }
            }
        
    }


    //int,int GetPlayerSuperBlock()
    //{
    //    int x = Mathf.FloorToInt(player.transform.position.x / superBlockSize);
    //    int y = Mathf.FloorToInt(player.transform.position.y / superBlockSize);
    //    return x,y
    //}


    // Update is called once per frame
    void Update()
    {
        playerSuperX = Mathf.FloorToInt(player.transform.position.x / superBlockSize);
        playerSuperY = Mathf.FloorToInt(player.transform.position.y / superBlockSize);

        if (playerSuperX != lastPlayerSuperX || playerSuperY != lastPlayerSuperY)
        {

            for (int superX = playerSuperX - 1; superX < playerSuperX + 2; superX++)
            {
                

                for (int superY = playerSuperY - 1; superY < playerSuperY + 2; superY++)
                {
                    Debug.Log(superY);

                    if (superBlocks[superX, superY, 0, 0] == null)
                    {

                        for (int px = 0; px < superBlockSize; px++)
                        {
                            for (int py = 0; py < superBlockSize; py++)
                            {

                                GameObject go = new GameObject("New Sprite");
                                go.layer = 9;
                                
                                //go.SetActive(false);
                                superBlocks[superX, superY, px, py] = go;

                                int blocksX = superX * superBlockSize + px;
                                int blocksY = superY * superBlockSize + py;
                                go.transform.position = new Vector2(blocksX, blocksY);

                                if (blocks[blocksX][blocksY] == 6)
                                {

                                    
                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite;
                                    go.AddComponent<BoxCollider2D>();

                                }
                                if (blocks[blocksX][blocksY] == 7)
                                {

                                    
                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite2;
                                    go.AddComponent<BoxCollider2D>();

                                }

                                if (blocks[blocksX][blocksY] == 8)
                                {

                                    
                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite3;
                                    go.AddComponent<BoxCollider2D>();

                                }
                            }
                        }
                    }
                }
            }
        }

        if (playerSuperX != lastPlayerSuperX)
        {
            int unusedX = playerSuperX - ((playerSuperX - lastPlayerSuperX) * 2);
            for (int superY = playerSuperY - 1; superY < playerSuperY + 2; superY++)
            {
                if (superBlocks[unusedX, superY, 0, 0] != null)
                {

                    if (superBlocks[unusedX, superY, 0, 0].activeSelf == true)
                    {
                        
                        for (int px = 0; px < superBlockSize; px++)
                        {
                            for (int py = 0; py < superBlockSize; py++)
                            {
                                superBlocks[unusedX, superY, px, py].SetActive(false);
                            }
                        }
                    }

                    
                    
                }
            }

            int nextX = playerSuperX + ((playerSuperX - lastPlayerSuperX));
            for (int superY = playerSuperY - 1; superY < playerSuperY + 2; superY++)
            {
                if (superBlocks[nextX, superY, 0, 0] != null)
                {

                    if (superBlocks[nextX, superY, 0, 0].activeSelf == false)
                    {

                        for (int px = 0; px < superBlockSize; px++)
                        {
                            for (int py = 0; py < superBlockSize; py++)
                            {
                                superBlocks[nextX, superY, px, py].SetActive(true);
                            }
                        }
                    }



                }
            }
        }

        if (playerSuperY != lastPlayerSuperY)
        {
            int unusedY = playerSuperY - ((playerSuperY - lastPlayerSuperY) * 2);
            for (int superX = playerSuperX - 1; superX < playerSuperX + 2; superX++)
            {

                if (superBlocks[superX, unusedY, 0, 0] != null)
                {

                    if (superBlocks[superX, unusedY, 0, 0].activeSelf == true)
                    {
                        
                        for (int px = 0; px < superBlockSize; px++)
                        {
                            for (int py = 0; py < superBlockSize; py++)
                            {
                                superBlocks[superX, unusedY, px, py].SetActive(false);
                            }
                        }
                    }

                    
                    
                }

            }

            int nextY = playerSuperY + ((playerSuperY - lastPlayerSuperY));
            for (int superX = playerSuperX - 1; superX < playerSuperX + 2; superX++)
            {

                if (superBlocks[superX, nextY, 0, 0] != null)
                {

                    if (superBlocks[superX, nextY, 0, 0].activeSelf == false)
                    {

                        for (int px = 0; px < superBlockSize; px++)
                        {
                            for (int py = 0; py < superBlockSize; py++)
                            {
                                superBlocks[superX, nextY, px, py].SetActive(true);
                            }
                        }
                    }



                }

            }
        }

        lastPlayerSuperX = playerSuperX;
        lastPlayerSuperY = playerSuperY;

    }

    public void removeBlock(int xPos, int yPos)
    {
        blocks[xPos][yPos] = 0;

        int superX = Mathf.FloorToInt(xPos / superBlockSize);
        int superY = Mathf.FloorToInt(yPos / superBlockSize);
        int localX = xPos - (superX * superBlockSize);
        int localY = yPos - (superY * superBlockSize);

        Destroy(superBlocks[superX, superY, localX, localY]);
        // Try to clean the object without having to replace it with a newly instantiated one
        GameObject go = new GameObject("New Sprite");
        go.layer = 9;
        superBlocks[superX, superY, localX, localY] = go;
        go.transform.position = new Vector2(xPos, yPos);
    }

    public void createBlock(int xPos, int yPos, int blockType)
    {
        blocks[xPos][yPos] = blockType;

        int superX = Mathf.FloorToInt(xPos / superBlockSize);
        int superY = Mathf.FloorToInt(yPos / superBlockSize);
        int localX = xPos - (superX * superBlockSize);
        int localY = yPos - (superY * superBlockSize);

        Destroy(superBlocks[superX, superY, localX, localY]);
        // Try to clean the object without having to replace it with a newly instantiated one
        GameObject go = new GameObject("New Sprite");
        go.layer = 9;

        // This only creates the same type of block, add diffrent types
        superBlocks[superX, superY, localX, localY] = go;
        go.transform.position = new Vector2(xPos, yPos);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        go.AddComponent<BoxCollider2D>();
    }

}



        //    playerSuperX = Mathf.FloorToInt(player.transform.position.x / superBlockSize);
        //    playerSuperY = Mathf.FloorToInt(player.transform.position.y / superBlockSize);

        //    if (playerSuperX != lastPlayerSuperX || playerSuperY != lastPlayerSuperY)
        //    {

        //        for (int px = playerSuperX-1; px < playerSuperX + 2; px++)
        //        {

        //            for (int py = playerSuperY - 1; py < playerSuperY + 2; py++)

        //                if (superBlocks[px][py][0][0].activeSelf == false)
        //                {
        //                    //for (int px2 = px*superBlockSize; px2 < xblocks; px2++)
        //                    //{

        //                    //    blockColumn = new List<GameObject>();


        //                    //    for (int py2 = py*superBlockSize; py2 < yblocks; py2++)
        //                    //    {

        //                    //        GameObject go = new GameObject("New Sprite");
        //                    //        //go.SetActive(false);
        //                    //        blockColumn.Add(go);
        //                    //        int superX = Mathf.FloorToInt(px2 / superBlockSize);
        //                    //        int superY = Mathf.FloorToInt(py2 / superBlockSize);

        //                    //        if (py2 % superBlockSize == 0)
        //                    //        {
        //                    //            blockColumn = new List<GameObject>();
        //                    //            superBlocks[superX][superY].Add(blockColumn);

        //                    //        }


        //                    //        if (blocks[px2][py2] == 6)
        //                    //        {

        //                    //            go.transform.position = new Vector2(px2, py2);
        //                    //            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        //                    //            renderer.sprite = sprite;
        //                    //            go.AddComponent<BoxCollider2D>();

        //                    //        }
        //                    //        if (blocks[px2][py2] == 7)
        //                    //        {

        //                    //            go.transform.position = new Vector2(px2, py2);
        //                    //            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        //                    //            renderer.sprite = sprite2;
        //                    //            go.AddComponent<BoxCollider2D>();

        //                    //        }

        //                    //        if (blocks[px2][py2] == 8)
        //                    //        {

        //                    //            go.transform.position = new Vector2(px2, py2);
        //                    //            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        //                    //            renderer.sprite = sprite3;
        //                    //            go.AddComponent<BoxCollider2D>();

        //                    //        }


        //                    //    }


        //                    //}

        //                    //Debug.Log(superBlocks[px][py][0][0].transform.position);
        //                    foreach (List<GameObject> blockColumn in superBlocks[px][py])
        //                    {
        //                        foreach (GameObject go in blockColumn)
        //                        {
        //                            go.SetActive(true);
        //                        }
        //                    }
        //                }
        //        }

        //    }

        //    if (playerSuperX != lastPlayerSuperX)
        //    {
        //        int unusedX = playerSuperX - ((playerSuperX - lastPlayerSuperX)*2);
        //        for (int py = playerSuperY-1; py < playerSuperY + 2; py++)
        //        {
        //            if (superBlocks[unusedX][py][0][0].activeSelf == true)
        //            {
        //                foreach (List<GameObject> blockColumn in superBlocks[unusedX][py])
        //                {
        //                    foreach (GameObject go in blockColumn)
        //                    {
        //                        go.SetActive(false);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (playerSuperY != lastPlayerSuperY)
        //    {
        //        int unusedY = playerSuperY - ((playerSuperY - lastPlayerSuperY) * 2);
        //        for (int px = playerSuperX - 1; px < playerSuperX + 2; px++)
        //        {
        //            if (superBlocks[px][unusedY][0][0].activeSelf == true)
        //            {
        //                foreach (List<GameObject> blockColumn in superBlocks[px][unusedY])
        //                {
        //                    foreach (GameObject go in blockColumn)
        //                    {
        //                        go.SetActive(false);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    lastPlayerSuperX = playerSuperX;
        //    lastPlayerSuperY = playerSuperY;

        //}
