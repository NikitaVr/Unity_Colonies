using UnityEngine;
using System.Collections;

public class Inventory : MonoBehaviour {

    // The item format is [y value = row in inventory][x value = position in row][integer indicating item, amount of this item]
    public int[,,] items;
    public int rows;
    public int rowWidth;

	// Use this for initialization
	void Start () {
        items = new int[rows, rowWidth, 2];
	}

    public int removeItem(int yPos, int xPos, int amount)
    {
        items[yPos, xPos, 1] -= amount;
        return items[yPos, xPos, 0];
    }

    public void addItem(int itemType, int amount)
    {
        for (int row = 0; row < items.Length; row++)
        {
            for (int x = 0; x < rowWidth; x++)
            {
                if (items[row,x,0] == itemType)
                {
                    items[row, x, 1] += amount;
                }
                else if (items[row, x, 0] == 0)
                {
                    items[row, x, 0] = itemType;
                    items[row, x, 1] = amount;
                }
            }
        }
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
