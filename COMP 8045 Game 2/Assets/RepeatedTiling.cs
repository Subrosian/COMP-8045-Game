using UnityEngine;
using System.Collections;

public class RepeatedTiling : MonoBehaviour {

    public Transform grasstiles_2x2;
    public Transform borderboundarytiles_3x2; //repeat surrounding 3x2 (rowxcol) tree tiles
    private int NumCols = 40; //number of cols that would be 2 tiles long each //stage area being 22 cols, though with extending the grass beyond that
    private int NumRows = 40; //# rows 2 tiles each //stage area being 22 rows, though with extending the grass beyond that

    private int numNonBorderCols = 22;
    private int numNonBorderRows = 22;

    //the following assignments are placeholder values
    public static float StageLeft = 10;
    public static float StageRight = 10;
    public static float StageTop = 10;
    public static float StageBottom = 10;

    // Use this for initialization
    void Start () {
        //make tiled grass tiles
        //noting also of eg. shuffling and\or rearranging the grass tiles ...
        float TileWidth = grasstiles_2x2.GetComponent<Renderer>().bounds.size.x;
        float TileHeight = grasstiles_2x2.GetComponent<Renderer>().bounds.size.y;

        Vector3 offsetFromCenter = new Vector3(-TileWidth * NumCols / 2, -TileHeight * NumRows / 2, 0);
        int colMargin = (NumCols - numNonBorderCols) / 2; //border columns' width surrounding the inner grass area
        int rowMargin = (NumRows - numNonBorderRows) / 2; //border rows' width surrounding the inner grass area
        Debug.Log("'right margin': " + (colMargin + numNonBorderCols) + "; bottom margin: " + (rowMargin + numNonBorderRows));
        for (float i = 0; i < NumCols; i++) //i,j each corresponding to a 2x2 region
        {
            for(float j = 0; j < NumRows; j++)
            {
                bool makeBoundary = false;
                if (i <= colMargin-1 || i >= colMargin + numNonBorderCols || j <= (rowMargin-1) - 0.5f || j >= rowMargin + numNonBorderRows)
                {
                    makeBoundary = true; //error - noting of how this would make makeBoundary true for successive 'nonBorder' rows after such would have been made true for the border rows
                }
                if (makeBoundary)
                {
                    Instantiate(borderboundarytiles_3x2, new Vector3(TileWidth * i, TileHeight * (j + 0.25f), 0) + offsetFromCenter, Quaternion.identity);
                    j += 0.5f; //adding for the corresponding tree tile, hard-coded here as the additional increment in addition to the bounds size of grass
                }
                else
                {
                    Instantiate(grasstiles_2x2, new Vector3(TileWidth * i, TileHeight * j, 0) + offsetFromCenter, Quaternion.identity);
                }
            }
        }

        //Noting of accuracy of this ... noting of this and being with including the border unless edited to not be
        Vector3 nonBorderOffsetFromCenter = new Vector3(-TileWidth * numNonBorderCols / 2, -TileHeight * numNonBorderRows / 2, 0);
        StageLeft = nonBorderOffsetFromCenter.x - TileWidth / 2; //left of the leftmost tile
        StageRight = nonBorderOffsetFromCenter.x + TileWidth * (numNonBorderCols-1) + TileWidth / 2; //right of the rightmost tile
        StageBottom = nonBorderOffsetFromCenter.y + - TileHeight / 2; //bottom of the bottommost tile
        StageTop = nonBorderOffsetFromCenter.y + TileHeight * (numNonBorderRows-1) + TileHeight / 2; //top of the topmost tile
        Debug.Log("StageLeft: " + StageLeft + "; StageRight: " + StageRight);
        Debug.Log("StageBottom: " + StageBottom + "; StageTop: " + StageTop);
        Debug.Log("Position of center offset: " + offsetFromCenter);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
