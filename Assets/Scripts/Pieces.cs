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

    private bool IsKingInCheck() {
        return controller.GetComponent<Game>().IsCheck(player);
    }

    private void OnMouseUp() {
        // Check if it's the current player's turn
        if(controller.GetComponent<Game>().GetCurrentPlayer() != player) {
            return;
        }

        // If the piece is already selected, deselect it
        if(selected == this) {
            Debug.Log($"Deselecting {this.name}");
            DestroyMovePlates();
            selected = null;    
            return;
        }

        // If another piece is selected, deselect it
        if(selected != null) {
            selected.DestroyMovePlates();
        }

        // Select the current piece and show its move options
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
        Game sc = controller.GetComponent<Game>();
        bool inCheck = sc.IsCheck(player);
        List<(int, int)> validMoves = inCheck ? GetValidMovePositions() : GetMovePositions();

        string pieceName = this.name;
        if(!inCheck) {
            switch (pieceName) {
                case "B_King":
                case "W_King":
                    SurroundMovePlate();
                    break;

                case "B_Queen":
                case "W_Queen":
                    foreach (var dir in new (int, int)[] { (1, 0), (0, 1), (1, 1), (-1, 0), (0, -1), (-1, -1), (-1, 1), (1, -1) }) {
                        LineMovePlate(dir.Item1, dir.Item2);
                    }
                    break;

                case "B_Bishop":
                case "W_Bishop":
                    foreach (var dir in new (int, int)[] { (1, 1), (-1, -1), (-1, 1), (1, -1) }) {
                        LineMovePlate(dir.Item1, dir.Item2);
                    }
                    break;

                case "B_Knight":
                case "W_Knight":
                    LMovePlate();
                    break;

                case "B_Rook":
                case "W_Rook":
                    foreach (var dir in new (int, int)[] { (1, 0), (0, 1), (-1, 0), (0, -1) }) {
                        LineMovePlate(dir.Item1, dir.Item2);
                    }
                    break;

                case "B_Pawn":
                case "W_Pawn":
                    PawnMovePlate(xPos, yPos + direction);
                    break;
            }
        } else {
            DestroyMovePlates();

            foreach (var move in validMoves) {
                int x = move.Item1;
                int y = move.Item2;

                GameObject positionPiece = sc.GetPosition(x, y);
                if (positionPiece == null) {
                    MovePlateSpawn(x, y);
                } else if (positionPiece.GetComponent<Pieces>().player != player) {
                    MovePlateAttackSpawn(x, y);
                }
            }
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
        Game sc = controller.GetComponent<Game>();
        foreach(var offset in new (int,int)[] {(0,1), (0,-1), (-1,-1), (-1,0), (-1,1), (1,-1), (1,0), (1,1)}) {
            int newX = xPos + offset.Item1;
            int newY = yPos + offset.Item2;

            if(sc.PositionOnBoard(newX,newY)) {
                PointMovePlate(newX, newY);
            }
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

        // Regular move forward
        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null) {
            Debug.Log($"Regular move to ({x}, {y})");
            MovePlateSpawn(x, y);

            // First move, allow moving 2 steps forward
            if (isFirstMove && sc.PositionOnBoard(x, y + direction) && sc.GetPosition(x, y + direction) == null) {
                Debug.Log($"First move to ({x}, {y + direction})");
                MovePlateSpawn(x, y + direction);
            }
        }

        // Diagonal captures
        if (sc.PositionOnBoard(x + 1, y) && sc.GetPosition(x + 1, y) != null &&
            sc.GetPosition(x + 1, y).GetComponent<Pieces>().player != player) {
            Debug.Log($"Capture move to ({x + 1}, {y})");
            MovePlateAttackSpawn(x + 1, y);
        }

        if (sc.PositionOnBoard(x - 1, y) && sc.GetPosition(x - 1, y) != null &&
            sc.GetPosition(x - 1, y).GetComponent<Pieces>().player != player) {
            Debug.Log($"Capture move to ({x - 1}, {y})");
            MovePlateAttackSpawn(x - 1, y);
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

    public List<(int, int)> GetMovePositions() {
        List<(int, int)> movePositions = new List<(int, int)>();

        switch (this.name) {
            case "B_King":
            case "W_King":
                foreach (var offset in new (int, int)[] {(0,1), (0,-1), (-1,-1), (-1,0), (-1,1), (1,-1), (1,0), (1,1)}) {
                    movePositions.Add((xPos + offset.Item1, yPos + offset.Item2));
                }
                break;

            case "B_Queen":
            case "W_Queen":
                foreach (var dir in new (int, int)[] {(1,0), (0,1), (1,1), (-1,0), (0,-1), (-1,-1), (-1,1), (1,-1)}) {
                    AddLineMovePositions(movePositions, dir.Item1, dir.Item2);
                }
                break;

            case "B_Bishop":
            case "W_Bishop":
                foreach (var dir in new (int, int)[] {(1,1), (-1,-1), (-1,1), (1,-1)}) {
                    AddLineMovePositions(movePositions, dir.Item1, dir.Item2);
                }
                break;

            case "B_Knight":
            case "W_Knight":
                foreach (var offset in new (int, int)[] {(1,2), (-1,2), (2,1), (2,-1), (1,-2), (-1,-2), (-2,1), (-2,-1)}) {
                    movePositions.Add((xPos + offset.Item1, yPos + offset.Item2));
                }
                break;

            case "B_Rook":
            case "W_Rook":
                foreach (var dir in new (int, int)[] {(1,0), (0,1), (-1,0), (0,-1)}) {
                    AddLineMovePositions(movePositions, dir.Item1, dir.Item2);
                }
                break;

            case "B_Pawn":
            case "W_Pawn":
                movePositions.Add((xPos, yPos + direction));
                if (isFirstMove) {
                    movePositions.Add((xPos, yPos + 2 * direction));
                }
                break;
        }

        return movePositions;
    }

    private void AddLineMovePositions(List<(int, int)> movePositions, int xDir, int yDir) {
        Game sc = controller.GetComponent<Game>();
        int x = xPos + xDir;
        int y = yPos + yDir;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null) {
            movePositions.Add((x, y));
            x += xDir;
            y += yDir;
        }

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Pieces>().player != player) {
            movePositions.Add((x, y));
        }
    }

    private List<(int, int)> cachedMovePositions;

    public List<(int, int)> GetValidMovePositions() {
        Game sc = controller.GetComponent<Game>();
        List<(int, int)> validMoves = new List<(int, int)>();
        List<(int, int)> allMoves = GetCachedMovePositions();

        foreach (var move in allMoves) {
            if (sc.PositionOnBoard(move.Item1, move.Item2)) {
                GameObject targetPosition = sc.GetPosition(move.Item1, move.Item2);
                if (targetPosition == null || targetPosition.GetComponent<Pieces>().player != player) {
                    if (sc.SimulateMove(gameObject, move)) {
                        validMoves.Add(move);
                    }
                } 
            }
        }

        return validMoves;
    }

    private List<(int, int)> GetCachedMovePositions() {
        if (cachedMovePositions == null) {
            cachedMovePositions = GetMovePositions();
        }
        return cachedMovePositions;
    }

    public void ClearCachedMovePositions() {
        cachedMovePositions = null;
    }
    public List<(int, int)> GetAttackPositions() {
        List<(int, int)> positions = new List<(int, int)>();

        if (this.name.StartsWith("W_Pawn")) {
            positions.Add((GetXPos() - 1, GetYPos() + 1)); // Diagonal attack (left)
            positions.Add((GetXPos() + 1, GetYPos() + 1)); // Diagonal attack (right)
        } else if (this.name.StartsWith("B_Pawn")) {
            positions.Add((GetXPos() - 1, GetYPos() - 1)); // Diagonal attack (left)
            positions.Add((GetXPos() + 1, GetYPos() - 1)); // Diagonal attack (right)
        } else {
            // Other piece types: use standard move generation
            return GetMovePositions();
        }

        return positions;
    }

}
