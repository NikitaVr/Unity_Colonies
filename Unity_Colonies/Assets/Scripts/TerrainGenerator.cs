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

    public Sprite cloud;

    public Sprite iceSoft;
    public Sprite iceHard;
    public Sprite tundraSoft;
    public Sprite tundraHard;
    public Sprite desertSoft;
    public Sprite desertHard;
    public Sprite jungleSoft;
    public Sprite jungleHard;

    private int attributesPerBlock = 4;

    
    public int height;
    public int cloudHeight;
    public float heat;
    public float moisture;
    public int cloudiness;

    public int[] topSoil;


    public Sprite[,,] spriteCollection;
    public Sprite mushroom;
    public int[,,] blocks;
    public int[,] heatMap;
    public int[,] moistureMap;
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

    public List<List<int>> seeds;



    // Function runs when object is first created (start of game)
    void Start()
    {
        fillSpriteCollection();

        // Run the functions required for terrain
        GenSuperBlocks();
        //genHeatMap();
        //genMoistureMap();
        GenTerrain();
        //loadTerrain();
        RenderTerrain();
        genSeeds();
        lastPlayerSuperX = Mathf.FloorToInt(player.transform.position.x / superBlockSize);
        lastPlayerSuperY = Mathf.FloorToInt(player.transform.position.y / superBlockSize);
        //saveGame();

    }

    void fillSpriteCollection()
    {
        //[heat,moisture,hardness]
        spriteCollection = new Sprite[2, 2, 3]; 
        spriteCollection[0, 0, 1] = tundraSoft;
        spriteCollection[0, 0, 2] = tundraHard;
        spriteCollection[0, 1, 1] = iceSoft;
        spriteCollection[0, 1, 2] = iceHard;
        spriteCollection[1, 0, 1] = desertSoft;
        spriteCollection[1, 0, 2] = desertHard;
        spriteCollection[1, 1, 1] = jungleSoft;
        spriteCollection[1, 1, 2] = jungleHard;
}

    //loads the terrain data for the blocks 2D array from a save file created with Protobuf-net
    void loadTerrain()
    {

        blocks = new int[xblocks, yblocks,4];

        using (FileStream f = new FileStream("GameSave", FileMode.OpenOrCreate))
        {
            terrain terrainObj = Serializer.Deserialize<terrain>(f);
            int[] blocksSingle = terrainObj.blocks;
            for (int px = 0; px < xblocks; px++)
            {
                for (int py = 0; py < yblocks; py++)
                {
                    for (int elementNumber = 0; elementNumber < attributesPerBlock; elementNumber++)
                    {


                        // FIX THIS!!!
                        blocks[px, py, elementNumber] = blocksSingle[py * attributesPerBlock + elementNumber + px * yblocks* attributesPerBlock];
                        //if (py == 500)
                        //{
                        //    Debug.Log(terrainObj.blocks[py + 2 + px * yblocks]);
                        //}

                    }
                }

            }
            
        }
    }


    // serializes and saves the blocks 2D array into a save file using Protobuf-net
    void saveGame()
    {
        using (FileStream f = new FileStream("GameSave", FileMode.OpenOrCreate))
        {
            int[] blocksSingle = new int[xblocks*yblocks* attributesPerBlock];
            for (int px = 0; px < xblocks; px++)
            {
                for (int py = 0; py < yblocks; py++)
                {
                    for (int elementNumber = 0; elementNumber < attributesPerBlock; elementNumber++)
                    {


                        // FIX THIS!!!
                        blocksSingle[py * attributesPerBlock + elementNumber + px * yblocks* attributesPerBlock] = blocks[px, py, elementNumber];
                        //if (py == 500)
                        //{
                        //    Debug.Log(blocksSingle[py + 2 + px * yblocks]);
                        //}
                    }
                }

            }
            //for (int px = 0; px < xblocks; px++)
            //{
            //    for (int py = 0; py < yblocks; py++)
            //    {
            //        for (int elementNumber = 0; elementNumber < 4; elementNumber++)
            //        {
            //            if (py == 500)
            //            {
            //                Debug.Log(blocksSingle[py * 4 + 2 + px * yblocks]);
            //            }
            //        }
            //    }
            //}
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

        //for (int px = 0; px < xblocks / superBlockSize; px++)
        //{
        //    superColumn = new List<List<List<GameObject>>>(yblocks / superBlockSize);
        //    superColumn = new GameObject[yblocks / superBlockSize][][];
        //    superBlocks.Add(superColumn);
        //    superBlocks[px] = superColumn;
        //    for (int py = 0; py < yblocks / superBlockSize; py++)
        //    {

        //        superBlock = new List<List<GameObject>>(superBlockSize);
        //        superBlock = new GameObject[superBlockSize][];
        //        superColumn.Add(superBlock);
        //        superColumn[py] = superBlock;
        //    }
        //}

    }

    //Creates lists of purely numbers that contain the block info
    void GenTerrain()
    {

        // create a new blocks variable that will contain the map data
        blocks = new int[xblocks,yblocks,4];
        topSoil = new int[xblocks];

        // cycle through all the x values of the map
        for (int px = 0; px < xblocks; px++)
        {

            // generate a 1D height map using only the x values for the Noise function, and y being a constant 
            int stone = Noise(px, 0, 80, 15, 1);
            stone += Noise(px,0,50,30,1);
            stone += Noise(px,0,10,10,1);
            stone += height;

            int dirt = Noise(px, 0, 100f, 35, 1);
            dirt += Noise(px,100,50,30,1);
            dirt += height;

            if (stone > dirt)
            {
                topSoil[px] = stone;
            }

            else
            {
                topSoil[px] = dirt;
            }

            // for each x value, cycle through the y values
            for (int py = 0; py < yblocks; py++)
            {

                int amountHeat = Mathf.FloorToInt(Noise(px, 0, 1000, 10, 1) / ((float) 50.0 / heat));
                if (amountHeat > 2)
                {
                    amountHeat = 2;
                }
                //heatMap[px, py] = amountHeat;
                // [blockHeat,blockMoisture,blockHardness,blockHealth]
                blocks[px, py, 0] = amountHeat;

                int amountMoisture = Mathf.FloorToInt(Noise(px + 100, 0, 1000, 10, 1) / ((float) 50.0 / moisture));
                if (amountMoisture > 2)
                {
                    amountMoisture = 2;
                }
                blocks[px, py,1] = amountMoisture;

                // generate stone and dirt based on the previosuly made heightmap
                if (py < stone)
                {
                    blocks[px, py,2] = 1;

                    //some green patches underground
                    if (Noise(px,py,12,16,1)>10)
                    {
                        blocks[px, py, 2] = 2;
                    }

                    //caves - also use this method, combined with moisture maps, to create  clouds
                    if (Noise(px,py*2,16,14,1)>10)
                    {
                        blocks[px, py, 2] = 0;
                        //Mushrooms
                        if (py > 1)
                        {
                            if (blocks[px, py - 1, 2] == 1 && Noise(px, py * 2, 2, 14, 1) > 7)
                            {
                                blocks[px, py, 2] = 4;
                            }
                        }
                    }
                }

                else if (py<dirt)
                {
                    blocks[px, py, 2] = 1;
                }
                // check if this block is a surface block above both dirt and stone
                else if ((py == dirt && py>stone) || (py == stone && py > dirt))
                {
                    GenTree(px, py);
                }

                if (Noise(px + 100, py + 100, 10, 10, 1) > cloudiness && py > height + cloudHeight)
                {
                    blocks[px, py, 2] = 5;
                }

                    //basic health for now
                    blocks[px, py, 3] = blocks[px, py, 2];





            }
        }
    }

    void genSeeds()
    {
        // sees for plants will have a moisture and heat requirement, and their height/size will be determined by how much they are above (or possibly below for some)
        // the required moisture + heat
        // each list for a seed represent: [posX,posY,heatAboveOrBelow, heatAmount, moistureAboveOrBelow, moistureAmount, seedType]
        // -1 = Below , 1 = Above
        // the heat and moisture and above or below values can later be gotten from a database,  the actual elements are replaced by just one element
        // pointing to the database
        seeds = new List<List<int>>();
    }


    void addSeed(int posX, int posY, int heatAboveOrBelow, int heatAmount, int moistureAboveOrBelow, int moistureAmount, int seedType)
    {
        List<int> seed = new List<int>();
        seed.Add(posX);
        seed.Add(posY);
        seed.Add(heatAboveOrBelow);
        seed.Add(heatAmount);
        seed.Add(moistureAboveOrBelow);
        seed.Add(moistureAmount);
        seed.Add(seedType);
        seeds.Add(seed);
    }
    void genHeatMap()
    {
        heatMap = new int[xblocks,yblocks];
        //int largest = 0;
        for (int px = 0; px < xblocks; px++)
        {
            for (int py = 0; py < yblocks; py++)
            {
                int amountHeat = Mathf.CeilToInt(Noise(px, 0, 1000, 10, 1) / (50 / heat));
                if (amountHeat > 2)
                {
                    amountHeat = 2;
                }
                heatMap[px, py] = amountHeat;
                //if (heatMap[px,py] > largest)
                //{
                //    largest = heatMap[px, py];
                //}
            }
            //Debug.Log("heatmap " + heatMap[px,0]);

        }
        
    }

    void genMoistureMap()
    {
        moistureMap = new int[xblocks, yblocks];
        //int largest = 0;
        for (int px = 0; px < xblocks; px++)
        {
            for (int py = 0; py < yblocks; py++)
            {
                int amount = Mathf.CeilToInt(Noise(px + 100, 0, 1000, 10, 1) / (50 / moisture));
                if (amount > 2)
                {
                    amount = 2;
                }
                moistureMap[px, py] = amount;
                //if (heatMap[px,py] > largest)
                //{
                //    largest = heatMap[px, py];
                //}
            }
            //Debug.Log("heatmap " + moistureMap[px, 0]);

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
                            blocks[xCoord, yCoord, 2] = 1;
                        }
                    }
                }
                // creates the body of the tree
                blocks[posX, yCoord, 2] = 3;

            }
            // adds a leaf ontop
            blocks[posX, posY + treeNoise - 5, 2] = 1;
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

                            if (blocks[blocksX, blocksY, 2] == 0)
                            {
                                //add a check to make sure clouds are above ground only
                                // same noise as for moisture but smaller scale....
                                //if (Noise(blocksX + 100, blocksY + 100, 10, 10, 1) > cloudiness)
                                //{
                                //    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                //    renderer.sprite = cloud;
                                //    //go.AddComponent<BoxCollider2D>();
                                //}
                            }

                            if (blocks[blocksX, blocksY, 2] == 1)
                            {


                                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                try
                                {
                                    renderer.sprite = spriteCollection[blocks[blocksX, blocksY, 0], blocks[blocksX, blocksY, 1], blocks[blocksX, blocksY, 2]];
                                }
                                catch
                                {
                                    //Debug.Log("heat:" + heatMap[blocksX, blocksY]);
                                    //Debug.Log(moistureMap[blocksX, blocksY]);
                                    //Debug.Log("Hardness:" + blocks[blocksX, blocksY, 2]);
                                    Debug.Log("Error in sprite renderer part");
                                }
                                go.AddComponent<BoxCollider2D>();

                            }
                            if (blocks[blocksX, blocksY, 2] == 2)
                            {

                                //Debug.Log(heatMap[px, py]);
                                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                //Debug.Log("heat:" + heatMap[blocksX, blocksY]);
                                //Debug.Log(moistureMap[blocksX, blocksY]);
                                //Debug.Log("Hardness:" + blocks[blocksX, blocksY]);
                                renderer.sprite = spriteCollection[blocks[blocksX, blocksY, 0], blocks[blocksX, blocksY, 1], blocks[blocksX, blocksY, 2]];
                                go.AddComponent<BoxCollider2D>();

                            }

                            if (blocks[blocksX, blocksY, 2] == 3)
                            {


                                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                renderer.sprite = sprite3;
                                go.AddComponent<BoxCollider2D>();

                            }
                            if (blocks[blocksX, blocksY, 2] == 4)
                            {


                                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                renderer.sprite = mushroom;
                                go.AddComponent<BoxCollider2D>();

                            }

                            if (blocks[blocksX, blocksY, 2] == 5)
                            {
                                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                renderer.sprite = cloud;
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


    // !!!!!!!!!!! FINISH THIS CODE! make sure that trees now work of moisture and heat not just one perlin noise
    void changeOverTime()
    {
        foreach(List<int> seed in seeds)
        {
            if (seed[6] == 1)
            {
                GenTree(seed[0],seed[1]);
            }
        }
    }


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

                                if (blocks[blocksX, blocksY, 2] == 0)
                                {
                                    //add a check to make sure clouds are above ground only
                                    // same noise as for moisture but smaller scale....
                                    //if (Noise(blocksX + 100, blocksY+ 100, 10, 10, 1) > cloudiness)
                                    //{
                                    //    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    //    renderer.sprite = cloud;
                                    //    //go.AddComponent<BoxCollider2D>();
                                    //}
                                }

                                if (blocks[blocksX, blocksY, 2] == 1)
                                {

                                    
                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    //Debug.Log("heat:" + heatMap[px, py]);
                                    //Debug.Log("moisture:" + moistureMap[px, py]);
                                    //Debug.Log("Hardness:" + blocks[px, py]);
                                    renderer.sprite = spriteCollection[blocks[blocksX, blocksY, 0], blocks[blocksX, blocksY, 1], blocks[blocksX, blocksY, 2]];
                                    go.AddComponent<BoxCollider2D>();

                                }
                                if (blocks[blocksX, blocksY, 2] == 2)
                                {

                                    // !!!! may cause blocks placed by player turn into region specific blocks, make sure to fix!
                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();

                                    renderer.sprite = spriteCollection[blocks[blocksX, blocksY, 0], blocks[blocksX, blocksY, 1], blocks[blocksX, blocksY, 2]];
                                    go.AddComponent<BoxCollider2D>();

                                }

                                if (blocks[blocksX,blocksY, 2] == 3)
                                {


                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = sprite3;
                                    go.AddComponent<BoxCollider2D>();

                                }
                                if (blocks[blocksX, blocksY, 2] == 4)
                                {


                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = mushroom;
                                    go.AddComponent<BoxCollider2D>();

                                }

                                if (blocks[blocksX, blocksY, 2] == 5)
                                {
                                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                                    renderer.sprite = cloud;
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
        blocks[xPos,yPos, 2] = 0;

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
        blocks[xPos,yPos, 2] = blockType;

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
