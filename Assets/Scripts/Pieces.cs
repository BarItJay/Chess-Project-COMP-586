using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pieces: MonoBehaviour
{
    public GameObject controller;
    public GameObject movePlate;
    private int xPos = -1;
    private int yPos = -1;
    private const float pieceSpacing = 0.96f;
    private const float xOffSet = -3.4f;
    private const float yOffSet = -3.5f;
    private string player;
    public Sprite B_King, B_Queen, B_Knight, B_Bishop, B_Rook, B_Pawn;
    public Sprite W_King, W_Queen, W_Knight, W_Bishop, W_Rook, W_Pawn;
    private Dictionary<string, Sprite> pieceSprites;

    public void PieceSprites() {
        pieceSprites = new Dictionary<string, Sprite> {
            {"B_King", B_King}, {"B_Queen", B_Queen}, {"B_Bishop", B_Bishop}, {"B_Knight", B_Knight}, {"B_Rook", B_Rook}, {"B_Pawn",B_Pawn},
            {"W_King", W_King}, {"W_Queen", W_Queen}, {"W_Bishop", W_Bishop}, {"W_Knight", W_Knight}, {"W_Rook", W_Rook}, {"W_Pawn",W_Pawn}
        };
    }

    public void Activate() {
        controller = GameObject.FindGameObjectWithTag("GameController");
        SetCoords();

        if(pieceSprites.ContainsKey(this.name)) {
            this.GetComponent<SpriteRenderer>().sprite = pieceSprites[this.name];
        }
    }

    public void SetCoords() {
        float x = xPos * pieceSpacing + xOffSet;
        float y = yPos * pieceSpacing + yOffSet;
        this.transform.position = new Vector3(x,y,-1.0f);
    }

    public int GetXPos() {
        return xPos;
    }

    public int GetYPos() {
        return yPos;
    }

    public void SetXPos(int x) {
        xPos = x;
    }

    public void SetYPos(int y) {
        yPos = y;
    }
}
