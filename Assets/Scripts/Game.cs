using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour {
    public GameObject piece;
    private GameObject[,] positions = new GameObject[8,8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];
    private string currentPlayer = "White";
    private bool gameOver = false;

    void Start() {
        InitializePieces();
        PiecesOnBoard();
    }

    private void InitializePieces() {
        playerWhite = new GameObject[] {
            Create("W_Rook", 0, 0),
            Create("W_Knight", 1, 0),
            Create("W_Bishop", 2, 0),
            Create("W_Queen", 3, 0),
            Create("W_King", 4, 0),
            Create("W_Bishop", 5, 0),
            Create("W_Knight", 6, 0),
            Create("W_Rook", 7, 0),
            Create("W_Pawn", 0, 1),
            Create("W_Pawn", 1, 1),
            Create("W_Pawn", 2, 1),
            Create("W_Pawn", 3, 1),
            Create("W_Pawn", 4, 1),
            Create("W_Pawn", 5, 1),
            Create("W_Pawn", 6, 1),
            Create("W_Pawn", 7, 1)
        };

        playerBlack = new GameObject[] {
            Create("B_Rook", 0, 7),
            Create("B_Knight", 1, 7),
            Create("B_Bishop", 2, 7),
            Create("B_Queen", 3, 7),
            Create("B_King", 4, 7),
            Create("B_Bishop", 5, 7),
            Create("B_Knight", 6, 7),
            Create("B_Rook", 7, 7),
            Create("B_Pawn", 0, 6),
            Create("B_Pawn", 1, 6),
            Create("B_Pawn", 2, 6),
            Create("B_Pawn", 3, 6),
            Create("B_Pawn", 4, 6),
            Create("B_Pawn", 5, 6),
            Create("B_Pawn", 6, 6),
            Create("B_Pawn", 7, 6)
        };

        for(int i = 0; i < playerBlack.Length; i++) {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
        
    }

    private void PiecesOnBoard() {
        foreach(GameObject piece in playerBlack) {
            SetPosition(piece);
        }

        foreach(GameObject piece in playerWhite) {
            SetPosition(piece);
        }
    }

    public GameObject Create(string name, int x, int y) {
        GameObject obj = Instantiate(piece, new Vector3(0,0,-1), Quaternion.identity);
        obj.name = name;
        Pieces p = obj.GetComponent<Pieces>();
        p.PieceSprites();
        p.SetXPos(x);
        p.SetYPos(y);
        p.Activate();

        return obj;
    }

    public void SetPosition(GameObject obj) {
        Pieces p = obj.GetComponent<Pieces>();
        int x = p.GetXPos();
        int y = p.GetYPos();
        if(x >= 0 && x < 8 && y >= 0 && y < 8) {
            positions[x, y] = obj;
        } else {
            Debug.LogWarning($"Position out of bounds: {x}, {y}");
        }
    }

    public void SetPositionEmpty(int x, int y) {
        if(PositionOnBoard(x, y)) {
            positions[x, y] = null;
        }
    }

    public GameObject GetPosition(int x, int y) {
        return PositionOnBoard(x, y) ? positions[x, y]: null;
    }

    public bool PositionOnBoard(int x, int y) {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    public string GetCurrentPlayer() {
        return currentPlayer;
    }

    public bool IsGameOver() {
        return gameOver;
    }

    public void NextTurn() {
        currentPlayer = (currentPlayer == "White") ? "Black" : "White";
    }

    public void Update() {
        if(gameOver) {
            if(Input.GetMouseButtonDown(0)) {
                SceneManager.LoadScene("Main Menu");
            }
            return;
        }
        KingStatus();
    }

    public void KingStatus() {
        bool whiteAlive = false;
        bool blackAlive = false;

        foreach(GameObject p in playerWhite) {
            if(p != null && p.name == "W_King") {
                whiteAlive = true;
            }
        }

        foreach(GameObject p in playerBlack) {
            if(p != null && p.name == "B_King") {
                blackAlive = true;
            }
        }

        if(!whiteAlive) {
            Winner("Black");
        } else if(!blackAlive) {
            Winner("White");
        }
    }

    public void Winner(string winner) {
        gameOver = true;
        Debug.Log($"{winner} wins! Game Over!");
    }
}
