using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
    public string player;
    public bool isFirstMove = true;
    public int direction;
    public Sprite B_King, B_Queen, B_Knight, B_Bishop, B_Rook, B_Pawn;
    public Sprite W_King, W_Queen, W_Knight, W_Bishop, W_Rook, W_Pawn;
    private Dictionary<string, Sprite> pieceSprites;
    private static Pieces selected;

    public void PieceSprites() {
        pieceSprites = new Dictionary<string, Sprite> {
            {"B_King", B_King}, {"B_Queen", B_Queen}, {"B_Bishop", B_Bishop}, {"B_Knight", B_Knight}, {"B_Rook", B_Rook}, {"B_Pawn",B_Pawn},
            {"W_King", W_King}, {"W_Queen", W_Queen}, {"W_Bishop", W_Bishop}, {"W_Knight", W_Knight}, {"W_Rook", W_Rook}, {"W_Pawn",W_Pawn}
        };
    }

    public void Activate() {
        PieceSprites();
        controller = GameObject.FindGameObjectWithTag("GameController");
        SetCoords();

        if(pieceSprites.ContainsKey(this.name)) {
            this.GetComponent<SpriteRenderer>().sprite = pieceSprites[this.name];
            player = this.name.StartsWith("B_") ? "Black":"White";

            if(this.name.Contains("Pawn")) {
                direction = (player == "White") ? 1:-1;
            }
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

    private void OnMouseUp() {

        if(controller.GetComponent<Game>().GetCurrentPlayer() != player) {
            return;
        }

        if(selected == this) {
            Debug.Log($"Deselecting {this.name}");
            DestroyMovePlates();
            selected = null;    
            return;
        }

        if(selected != null) {
            selected.DestroyMovePlates();
        }

        selected = this;
        DestroyMovePlates();
        InitiateMovePlates();
    }

    public void DestroyMovePlates() {
        foreach(GameObject movePlate in GameObject.FindGameObjectsWithTag("MovePlate")) {
            Destroy(movePlate);
        }
    }

    public void InitiateMovePlates() {
        switch(this.name) {
            case "B_King":
            case "W_King":
                SurroundMovePlate();
                break;

            case "B_Queen":
            case "W_Queen":
                foreach(var dir in new (int,int)[] {(1,0), (0,1), (1,1), (-1,0), (0,-1), (-1,-1), (-1,1), (1,-1)}) {
                    LineMovePlate(dir.Item1, dir.Item2);
                }
                break;

            case "B_Bishop":
            case "W_Bishop":
                foreach(var dir in new (int,int)[] {(1,1), (-1,-1), (-1,1), (1,-1)}) {
                    LineMovePlate(dir.Item1, dir.Item2);
                }
                break;

            case "B_Knight":
            case "W_Knight":
                LMovePlate();
                break;

            case "B_Rook":
            case "W_Rook":
                foreach(var dir in new (int,int)[] {(1,0), (0,1), (-1,0), (0,-1)}) {
                    LineMovePlate(dir.Item1, dir.Item2);
                }
                break;

            case "B_Pawn":
            case "W_Pawn":
                PawnMovePlate(xPos, yPos+direction);
                break;

        }
    }

    public void LineMovePlate(int xDir, int yDir) {
        Game sc = controller.GetComponent<Game>();

        int x = xPos+xDir;
        int y = yPos+yDir;

        while(sc.PositionOnBoard(x,y) && sc.GetPosition(x,y) == null) {
            MovePlateSpawn(x,y);
            x += xDir;
            y += yDir;
        }

        if(sc.PositionOnBoard(x,y) && sc.GetPosition(x,y).GetComponent<Pieces>().player != player) {
            MovePlateAttackSpawn(x,y);
        }
    }

    public void LMovePlate() {
        foreach(var offset in new (int,int)[] {(1,2), (-1,2), (2,1), (2,-1), (1,-2), (-1,-2), (-2,1), (-2,-1)}) {
            PointMovePlate(xPos+offset.Item1, yPos+offset.Item2);
        }
    }

    public void SurroundMovePlate() {
        foreach(var offset in new (int,int)[] {(0,1), (0,-1), (-1,-1), (-1,0), (-1,1), (1,-1), (1,0), (1,1)}) {
            PointMovePlate(xPos+offset.Item1, yPos+offset.Item2);
        }
    }

    public void PointMovePlate(int x, int y) {
        Game sc = controller.GetComponent<Game>();
        if(sc.PositionOnBoard(x,y)) {
            GameObject piece = sc.GetPosition(x,y);

            if(piece == null) {
                MovePlateSpawn(x,y);
            } else if(piece.GetComponent<Pieces>().player != player) {
                MovePlateAttackSpawn(x,y);
            }
        }
    }

    public void PawnMovePlate(int x, int y) {
        Game sc = controller.GetComponent<Game>();
        if(sc.PositionOnBoard(x,y)) {
            if(sc.GetPosition(x,y) == null) {
                MovePlateSpawn(x,y);

                if(isFirstMove && sc.PositionOnBoard(x, y+direction) && sc.GetPosition(x, y+direction) == null) {
                    MovePlateSpawn(x, y+direction);
                    isFirstMove = false;
                }
            }

            if(sc.PositionOnBoard(x+1,y) && sc.GetPosition(x+1,y) != null && sc.GetPosition(x+1,y).GetComponent<Pieces>().player != player) {
                MovePlateAttackSpawn(x+1,y);
            }

            if(sc.PositionOnBoard(x-1,y) && sc.GetPosition(x-1,y) != null && sc.GetPosition(x-1,y).GetComponent<Pieces>().player != player) {
                MovePlateAttackSpawn(x-1,y);
            }
        }
    }

    public void MovePlateSpawn(int xPos, int yPos) {
        float x = xPos * pieceSpacing + xOffSet;
        float y = yPos * pieceSpacing + yOffSet + 0.15f;
    
        GameObject mp = Instantiate(movePlate, new Vector3(x,y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(xPos, yPos);
    }

    public void MovePlateAttackSpawn(int xPos, int yPos) {
        float x = xPos * pieceSpacing + xOffSet;
        float y = yPos * pieceSpacing + yOffSet + 0.15f;
    
        GameObject mp = Instantiate(movePlate, new Vector3(x,y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(xPos, yPos);
    }
}
