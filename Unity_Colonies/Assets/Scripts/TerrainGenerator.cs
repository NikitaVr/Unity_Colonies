using UnityEngine;
using System.Collections;
// allows for use of lists:
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using GameSaveData;

public class TerrainGenerator : MonoBehaviour
{

    // Setting up all the required variables
    public Sprite sprite;
    public Sprite sprite2;
    public Sprite sprite3;
    public Sprite mushroom;
    public int[,] blocks;
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
    private int stone;
    private int dirt;



    // Function runs when object is first created (start of game)
    void Start()
    {
        // Run the functions required for terrain
        GenSuperBlocks();
        //GenTerrain();
        loadTerrain();
        RenderTerrain();
        lastPlayerSuperX = Mathf.FloorToInt(player.transform.position.x / superBlockSize);
        lastPlayerSuperY = Mathf.FloorToInt(player.transform.position.y / superBlockSize);
        //saveGame();

    }

    //loads the terrain data for the blocks 2D array from a save file created with Protobuf-net
    void loadTerrain()
    {

        blocks = new int[xblocks, yblocks];

        using (FileStream f = new FileStream("GameSave", FileMode.OpenOrCreate))
        {
            terrain terrainObj = Serializer.Deserialize<terrain>(f);
            int[] blocksSingle = terrainObj.blocks;
            for (int px = 0; px < xblocks; px++)
            {
                for (int py = 0; py < yblocks; py++)
                {
                    blocks[px, py] = blocksSingle[py + px * yblocks];
                }

            }
        }
    }


    // serializes and saves the blocks 2D array into a save file using Protobuf-net
    void saveGame()
    {
        using (FileStream f = new FileStream("GameSave", FileMode.OpenOrCreate))
        {
            int[] blocksSingle = new int[xblocks*yblocks];
            for (int px = 0; px < xblocks; px++)
            {
                for (int py = 0; py < yblocks; py++)
                {
                    blocksSingle[py + px * yblocks] = blocks[px, py];
                }

            }
            terrain saveTerrain = new terrain(blocksSingle);
            ProtoBuf.Serializer.Serialize<terrain>(f, saveTerrain);
        }
    }


    // function for creating terrain noise for varying terrain, returns an int
    // x and y are the coordinates of the block
    // scale determines how 'large' the perlin noise is, or how far the theoretical points are apart from each other
    // this determines whether transition from on point to the next is smooth or jagged, larger scale means smoother.
    // mag = magnitude which determines the intensity of the returned noise at each point
    // exp still needs to be figured out!
    int Noise(int x, int y, float scale, float mag, float exp)
    {
        return (int)(Mathf.Pow(Mathf.PerlinNoise(x / scale, y / scale) * mag, (exp)));
    }


    // function for creating the larger super blocks, each on of which will contain a certain number of blocks
    // splits up the blocks 2D array into many smaller 2D array of size superBlockSize, and set these 2D arrays
    // into another 2D array
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

        // create a new blocks variable that will contain the map data
        blocks = new int[xblocks,yblocks];

        // cycle through all the x values of the map
        for (int px = 0; px < xblocks; px++)
        {

            // generate a 1D height map using only the x values for the Noise function, and y being a constant 
            int stone = Noise(px, 0, 80, 15, 1);
            stone += Noise(px,0,50,30,1);
            stone += Noise(px,0,10,10,1);
            stone += 150;

            int dirt = Noise(px, 0, 100f, 35, 1);
            dirt += Noise(px,100,50,30,1);
            dirt += 150;

            // for each x value, cycle through the y values
            for (int py = 0; py < yblocks; py++)
            {

                // generate stone and dirt based on the previosuly made heightmap
                if (py < stone)
                {
                    blocks[px, py] = 1;

                    //some green patches underground
                    if (Noise(px,py,12,16,1)>10)
                    {
                        blocks[px, py] = 2;
                    }

                    //caves
                    if (Noise(px,py*2,16,14,1)>10)
                    {
                        blocks[px, py] = 0;
                        //Mushrooms
                        if (py > 1)
                        {
                            if (blocks[px, py - 1] == 1 && Noise(px, py * 2, 2, 14, 1) > 7)
                            {
                                blocks[px, py] = 4;
                            }
                        }
                    }
                }

                else if (py<dirt)
                {
                    blocks[px, py] = 2;
                }
                // check if this block is a surface block above both dirt and stone
                else if ((py == dirt && py>stone) || (py == stone && py > dirt))
                {
                    GenTree(px, py);
                }






            }
        }
    }

    // function for generating a tree that requires a value for the x and y coordinates of the block that will be the base of the tree
    void GenTree(int posX,int posY)
    {
        int treeNoise = Noise(posX, posY * 2, 2, 14, 1);

        // if the perlin noise at this point is high enough, create the tree
        if ( treeNoise > 7)
        {
            // create a tree the size of which depends on the perlin noise at the base
            for (int py = 0; py < (treeNoise-5); py++)
            {
                int yCoord = posY + py;
                if (py > (treeNoise - 8))
                {
                    for (int px = -1; px < 2; px++)
                    {
                        int xCoord = px + posX;
                        //add leaves only near the top
                        if (xCoord > 1 && xCoord < xblocks - 1)
                        {
                            blocks[xCoord, yCoord] = 2;
                        }
                    }
                }
                // creates the body of the tree
                blocks[posX, yCoord] = 3;

            }
            // adds a leaf ontop
            blocks[posX, posY + treeNoise - 5] = 2;
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

                                if (blocks[blocksX,blocksY] == 1)
                                {


                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite;
                                    go.AddComponent<BoxCollider2D>();

                                }
                                if (blocks[blocksX,blocksY] == 2)
                                {


                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite2;
                                    go.AddComponent<BoxCollider2D>();

                                }

                            if (blocks[blocksX, blocksY] == 3)
                            {


                                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                renderer.sprite = sprite3;
                                go.AddComponent<BoxCollider2D>();

                            }
                            if (blocks[blocksX, blocksY] == 4)
                            {


                                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                renderer.sprite = mushroom;
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

                                if (blocks[blocksX, blocksY] == 1)
                                {

                                    
                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite;
                                    go.AddComponent<BoxCollider2D>();

                                }
                                if (blocks[blocksX, blocksY] == 2)
                                {

                                    
                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite2;
                                    go.AddComponent<BoxCollider2D>();

                                }

                                if (blocks[blocksX,blocksY] == 3)
                                {


                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite3;
                                    go.AddComponent<BoxCollider2D>();

                                }
                                if (blocks[blocksX, blocksY] == 4)
                                {


                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = mushroom;
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
        blocks[xPos,yPos] = 0;

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
        blocks[xPos,yPos] = blockType;

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

    void OnApplicationQuit()
    {
        saveGame();
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
