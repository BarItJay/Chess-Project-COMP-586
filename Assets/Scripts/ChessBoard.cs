using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public enum SpecialMove {
    None,
    EnPassant,
    Castling,
    Promotion
}

public class ChessBoard : MonoBehaviour {
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f, deathSpacing = 0.3f, deathHeight = -0.82f, dragOffset = 1.5f;
    [SerializeField] private GameObject winnerScreen;
    [SerializeField] private GameObject promotionScreen;

    [Header("Prefabs & Material")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private Animator menuAnimator;

    //Logic
    private Pieces[,] pieces;
    private Pieces currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<Pieces> deadWhites = new List<Pieces>();
    private List<Pieces> deadBlacks = new List<Pieces>();
    private const int TILE_COUNT_X = 8, TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isWhite;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();


    private void Awake() {
        isWhite = true;
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        GenerateAllPieces();
        PositionAllPieces();
    }

    private void Update() {
    if (!currentCamera) {
        currentCamera = Camera.main;
        return;
    }

    RaycastHit info;
    // Raycast against both "Tile" and "Hover" layers
    Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
    int layerMask = LayerMask.GetMask("Tile", "Hover", "Highlight", "Capture");

    if (Physics.Raycast(ray, out info, 100, layerMask)) {
        // Get index of the tile hit
        Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

        // If hovering over a new tile
        if (currentHover == -Vector2Int.one) {
            currentHover = hitPosition;
            tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
        }
        // If hovering over a different tile
        else if (currentHover != hitPosition) {
            tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
            currentHover = hitPosition;
            tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");

        } 
        // press mouse button
        if(Input.GetMouseButtonDown(0)) {
            if(pieces[hitPosition.x, hitPosition.y]) {
                //Check player turn
                if((pieces[hitPosition.x, hitPosition.y].team == 0 && isWhite) || (pieces[hitPosition.x, hitPosition.y].team == 1 && !isWhite)) {
                    currentlyDragging = pieces[hitPosition.x, hitPosition.y];
                    
                    //Get list of where piece can move with highlight
                    availableMoves = currentlyDragging.GetAvailableMoves(ref pieces, TILE_COUNT_X, TILE_COUNT_Y);
                    //Get list of special moves
                    specialMove = currentlyDragging.GetSpecialMoves(ref pieces, ref moveList, ref availableMoves);

                    PreventCheck();
                    HighlightTiles();
                }
            }
        } 
 
        // release mouse button
        if(currentlyDragging != null && Input.GetMouseButtonUp(0)) {
            Vector2Int previousPos = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

            bool validMove = MoveToPos(currentlyDragging, hitPosition.x, hitPosition.y);
            if(!validMove) {
                currentlyDragging.SetPos(GetTileCenter(previousPos.x, previousPos.y));
            } 
            currentlyDragging = null;
            RemoveHighlight();
        }
    } else {
        // If not hovering over any tile
        if (currentHover != -Vector2Int.one) {
            tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
            currentHover = -Vector2Int.one;
        }

        if(currentlyDragging && Input.GetMouseButtonUp(0)) {
            currentlyDragging.SetPos(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
            currentlyDragging = null;
            RemoveHighlight();
        }
    }

    //if dragging piece
    if(currentlyDragging) {
        Plane horPlane = new Plane(Vector3.up, Vector3.up * yOffset);
        float distance = 0.0f;
        if(horPlane.Raycast(ray, out distance)) {
            currentlyDragging.SetPos(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }

}

    //Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY) {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX/2) * tileSize, 0, (tileCountX/2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for(int x = 0; x < tileCountX; x++) {
            for(int y = 0; y < tileCountY; y++) {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y) {
        GameObject tileObj = new GameObject($"X:{x}, Y:{y}");
        tileObj.transform.parent = transform;

        Mesh mesh  = new Mesh();
        tileObj.AddComponent<MeshFilter>().mesh = mesh;
        tileObj.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - bounds;

        int[] tris = new int[] {0,1,2,1,3,2};

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObj.layer = LayerMask.NameToLayer("Tile");
        tileObj.AddComponent<BoxCollider>();

        return tileObj;
    }

    //Generate Pieces
    private void GenerateAllPieces() {
        pieces = new Pieces[TILE_COUNT_X, TILE_COUNT_Y];
        int whiteTeam = 0, blackTeam = 1;

        //White Team
        pieces[0, 0] = GenerateSinglePiece(PieceType.Rook, whiteTeam);
        pieces[1, 0] = GenerateSinglePiece(PieceType.Knight, whiteTeam);
        pieces[2, 0] = GenerateSinglePiece(PieceType.Bishop, whiteTeam);
        pieces[3, 0] = GenerateSinglePiece(PieceType.Queen, whiteTeam);
        pieces[4, 0] = GenerateSinglePiece(PieceType.King, whiteTeam);
        pieces[5, 0] = GenerateSinglePiece(PieceType.Bishop, whiteTeam);
        pieces[6, 0] = GenerateSinglePiece(PieceType.Knight, whiteTeam);
        pieces[7, 0] = GenerateSinglePiece(PieceType.Rook, whiteTeam);

        for(int i = 0; i < TILE_COUNT_X; i++) {
            pieces[i, 1] = GenerateSinglePiece(PieceType.Pawn, whiteTeam);
        }

        //Black Team
        pieces[0, 7] = GenerateSinglePiece(PieceType.Rook, blackTeam);
        pieces[1, 7] = GenerateSinglePiece(PieceType.Knight, blackTeam);
        pieces[2, 7] = GenerateSinglePiece(PieceType.Bishop, blackTeam);
        pieces[3, 7] = GenerateSinglePiece(PieceType.Queen, blackTeam);
        pieces[4, 7] = GenerateSinglePiece(PieceType.King, blackTeam);
        pieces[5, 7] = GenerateSinglePiece(PieceType.Bishop, blackTeam);
        pieces[6, 7] = GenerateSinglePiece(PieceType.Knight, blackTeam);
        pieces[7, 7] = GenerateSinglePiece(PieceType.Rook, blackTeam);

        for(int i = 0; i < TILE_COUNT_X; i++) {
            pieces[i, 6] = GenerateSinglePiece(PieceType.Pawn, blackTeam);
        }
    }

    private Pieces GenerateSinglePiece(PieceType type, int team) {
        Pieces p = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Pieces>();
        p.type = type;
        p.team = team;
        p.GetComponent<MeshRenderer>().material = teamMaterials[team];
        return p;
    }

    //Positioning
    private void PositionAllPieces() {
        for(int i = 0; i < TILE_COUNT_X; i++) {
            for(int j = 0; j < TILE_COUNT_Y; j++) {
                if(pieces[i, j] != null) {
                    PositionSinglePiece(i, j, true);
                }
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false) {
        pieces[x, y].currentX = x;
        pieces[x, y].currentY = y;
        pieces[x, y].SetPos(GetTileCenter(x, y), force);//remove force for unintended animation
    }

    private Vector3 GetTileCenter(int x, int y) {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2);
    }

    //Highlight Tiles
    private void HighlightTiles() {
        for(int i = 0; i < availableMoves.Count; i++) {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    private void RemoveHighlight() {
        for(int i = 0; i < availableMoves.Count; i++) {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }

        availableMoves.Clear();
    }

    //CheckMate
    private void CheckMate(int team) {
        DisplayWinner(team);
    }

    private void DisplayWinner(int winner) {
        for(int i = 0; i < winnerScreen.transform.childCount; i++) {
            winnerScreen.transform.GetChild(i).gameObject.SetActive(false);
        }
        winnerScreen.SetActive(true);
        winnerScreen.transform.GetChild(winner).gameObject.SetActive(true);
        winnerScreen.transform.GetChild(2).gameObject.SetActive(true);
        winnerScreen.transform.GetChild(3).gameObject.SetActive(true);
    }

    public void OnResetButton() {
        //UI
        winnerScreen.transform.GetChild(0).gameObject.SetActive(false);
        winnerScreen.transform.GetChild(1).gameObject.SetActive(false);
        winnerScreen.SetActive(false);

        //Reset fields
        currentlyDragging = null;
        availableMoves.Clear();
        moveList.Clear();

        //Clean up
        for(int x = 0; x < TILE_COUNT_X; x++) {
            for(int y = 0; y < TILE_COUNT_Y; y++) {
                if(pieces[x, y] != null) {
                    Destroy(pieces[x, y].gameObject);
                }
                pieces[x, y] = null;
            }
        }

        for(int i = 0; i < deadWhites.Count; i++) {
            Destroy(deadWhites[i].gameObject);
        }
        for(int i = 0; i < deadBlacks.Count; i++) {
            Destroy(deadBlacks[i].gameObject);
        }

        deadWhites.Clear();
        deadBlacks.Clear();

        GenerateAllPieces();
        PositionAllPieces();
        isWhite = true;
    }

    public void OnMenuExitButton() {
        menuAnimator.SetTrigger("StartMenu");
        OnResetButton();
    }

    public void OnReturnToStartButton() {
        SceneManager.LoadScene("MainMenu");
    }
    //Special Moves
    private void ProcessSpecialMove() {
        if(specialMove == SpecialMove.Promotion) {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            Pieces targetPawn = pieces[lastMove[1].x, lastMove[1].y];

            if(targetPawn.type == PieceType.Pawn) {
                if((targetPawn.team == 0 && lastMove[1].y == 7) || (targetPawn.team == 1 && lastMove[1].y == 0)) {
                    DisplayPromotion(targetPawn.team);
                }
            }
        }

        if(specialMove == SpecialMove.EnPassant) {
            var newMove = moveList[moveList.Count - 1];
            Pieces myPawn = pieces[newMove[1].x, newMove[1].y];
            var targetPawnPos = moveList[moveList.Count - 2];
            Pieces enemyPawn = pieces[targetPawnPos[1].x, targetPawnPos[1].y];

            if(myPawn.currentX == enemyPawn.currentX) {
                if(myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1) {
                    if(enemyPawn.team == 0) {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPos(new Vector3(8 * tileSize, deathHeight, -1 * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2) + (Vector3.forward * deathSpacing) * deadWhites.Count);
                    }
                    else {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPos(new Vector3(8 * tileSize, deathHeight, -1 * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2) + (Vector3.forward * deathSpacing) * deadBlacks.Count);
                    }
                    pieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }

        if(specialMove == SpecialMove.Castling) {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            //Left Rook
            if(lastMove[1].x == 2) {
                 if(lastMove[1].y == 0) {//White
                    Pieces rook = pieces[0, 0];
                    pieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    pieces[0, 0] = null;
                 } else if(lastMove[1].y == 7) {//Black
                    Pieces rook = pieces[0, 7];
                    pieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    pieces[0, 7] = null;
                 }
            }//Right Rook
            else if(lastMove[1].x == 6) {
                 if(lastMove[1].y == 0) {//White
                    Pieces rook = pieces[7, 0];
                    pieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    pieces[7, 0] = null;
                 } else if(lastMove[1].y == 7) {//Black
                    Pieces rook = pieces[7, 7];
                    pieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    pieces[7, 7] = null;
                 }
            }

        }
    }

    private void PreventCheck() {
        Pieces targetKing = null;
        for(int x = 0; x < TILE_COUNT_X; x++) {
            for(int y = 0; y < TILE_COUNT_Y; y++) {
                if(pieces[x, y] != null) {
                    if(pieces[x, y].type == PieceType.King) {
                        if(pieces[x, y].team == currentlyDragging.team) {
                            targetKing = pieces[x, y];
                        }
                    }
                }
            }
        }

        //Deduce moves that lead to check and remove them from possible moves
        SimulateMovesIndividual(currentlyDragging, ref availableMoves, targetKing);
    }

    private void SimulateMovesIndividual(Pieces p, ref List<Vector2Int> moves, Pieces targetKing) {
        //Save current values to return to afterwards
        int actualX = p.currentX;
        int actualY = p.currentY;
        List<Vector2Int> invalidMoves = new List<Vector2Int>();

        //Simulate all moves and check for check
        for(int i = 0; i < moves.Count; i++) {
            int simX = moves[i].x;
            int simY = moves[i].y;

            //Simulated King Position
            Vector2Int kingPosSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            //Was the King's move simulated
            if(p.type == PieceType.King) {
                kingPosSim = new Vector2Int(simX, simY);
            }

            //Copy the [,] and not a ref
            Pieces[,] sim = new Pieces[TILE_COUNT_X, TILE_COUNT_Y];
            List<Pieces> simAttacking = new List<Pieces>();
            for(int x = 0; x < TILE_COUNT_X; x++) {
                for(int y = 0; y < TILE_COUNT_Y; y++) {
                    if(pieces[x, y] != null) {
                        sim[x, y] = pieces[x, y];
                        if(sim[x, y].team != p.team) {
                            simAttacking.Add(sim[x, y]);
                        }
                    }
                }
            }

            //Simulate move
            sim[actualX, actualY] = null;
            p.currentX = simX;
            p.currentY = simY;
            sim[simX, simY] = p;

            //Was a piece captured in simulation
            var deadPiece = simAttacking.Find(c => c.currentX == simX && c.currentY == simY);
            if(deadPiece != null) {
                simAttacking.Remove(deadPiece);
            }

            //Get all simulated attacking moves
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for(int j = 0; j < simAttacking.Count; j++) {
                var pieceMoves = simAttacking[j].GetAvailableMoves(ref sim, TILE_COUNT_X, TILE_COUNT_Y);
                for(int k = 0; k < pieceMoves.Count; k++) {
                    simMoves.Add(pieceMoves[k]);
                }
            }

            //Would the move lead to check
            if(ContainsValidMove(ref simMoves, kingPosSim)) {
                invalidMoves.Add(moves[i]);
            }

            //Restore p data
            p.currentX = actualX;
            p.currentY = actualY;
        }

        //Remove from move list
        for(int i = 0; i < invalidMoves.Count; i++) {
            moves.Remove(invalidMoves[i]);
        }
    }

    private bool CheckForCheckMate() {
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (pieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;

        List<Pieces> attacking = new List<Pieces>();
        List<Pieces> defending = new List<Pieces>();
        Pieces targetKing = null;
        for(int x = 0; x < TILE_COUNT_X; x++) {
            for(int y = 0; y < TILE_COUNT_Y; y++) {
                if(pieces[x, y] != null) {
                    if(pieces[x, y].team == targetTeam) {
                        defending.Add(pieces[x, y]);
                        if(pieces[x, y].type == PieceType.King) {
                            targetKing = pieces[x, y];
                        }
                    } else {
                        attacking.Add(pieces[x, y]);
                    }
                }
            }
        }
        //Is the king in danger
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for(int i = 0; i < attacking.Count; i++) {
            var pieceMoves = attacking[i].GetAvailableMoves(ref pieces, TILE_COUNT_X, TILE_COUNT_Y);
            for(int j = 0; j < pieceMoves.Count; j++) {
                currentAvailableMoves.Add(pieceMoves[j]);
            }
        }
        //Is it check
        if(ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY))) {
            //Is there a way out of check
            for(int i = 0; i < defending.Count; i++) {
                List<Vector2Int> defendingMoves = defending[i].GetAvailableMoves(ref pieces, TILE_COUNT_X, TILE_COUNT_Y);
                SimulateMovesIndividual(defending[i], ref defendingMoves, targetKing);

                if(defendingMoves.Count != 0) {
                    return false;
                }
            }
            return true;
        }


        return false;
    }

    //Promotions
    private void DisplayPromotion(int team) {
        promotionScreen.SetActive(true);        
        winnerScreen.transform.GetChild(team).gameObject.SetActive(true);
    }

    public void PromoteQueen() {
        PromotePawn(PieceType.Queen);
    }

    public void PromoteKnight() {
        PromotePawn(PieceType.Knight);
    }

    public void PromoteBishop() {
        PromotePawn(PieceType.Bishop);
    }

    public void PromoteRook() {
        PromotePawn(PieceType.Rook);
    }

    private void PromotePawn(PieceType newType) {
        Vector2Int[] lastMove = moveList[moveList.Count - 1];
        Pieces targetPawn = pieces[lastMove[1].x, lastMove[1].y];
        Pieces newPiece = GenerateSinglePiece(newType, targetPawn.team);
        newPiece.transform.position = targetPawn.transform.position;

        Destroy(pieces[lastMove[1].x, lastMove[1].y].gameObject);
        pieces[lastMove[1].x, lastMove[1].y] = newPiece;

        PositionSinglePiece(lastMove[1].x, lastMove[1].y);

        
        promotionScreen.SetActive(false);

        specialMove = SpecialMove.None;

    }
    
    //Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos) {
        for(int i = 0; i < moves.Count; i++) {
            if(moves[i].x == pos.x && moves[i].y == pos.y) {
                return true;
            }
        }

        return false;
    }
    private bool MoveToPos(Pieces p, int x, int y) {
        if(!ContainsValidMove(ref availableMoves, new Vector2Int(x,y))) {
            return false;
        }
        Vector2Int previousPos = new Vector2Int(p.currentX, p.currentY);

        //There is another piece on target position
        if(pieces[x, y] != null) {
            Pieces otherPiece = pieces[x, y];
            if(p.team == otherPiece.team) {
                return false;
            }

            //If its enemy team
            if(otherPiece.team == 0) {
                if(otherPiece.type == PieceType.King) {
                    CheckMate(1);
                }
                deadWhites.Add(otherPiece);
                otherPiece.SetScale(Vector3.one * deathSize);
                otherPiece.SetPos(new Vector3(8 * tileSize, deathHeight, -1 * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2) + (Vector3.forward * deathSpacing) * deadWhites.Count);
            } else {
                if(otherPiece.type == PieceType.King) {
                    CheckMate(0);
                }
                deadBlacks.Add(otherPiece);
                otherPiece.SetScale(Vector3.one * deathSize);
                otherPiece.SetPos(new Vector3(-1 * tileSize, deathHeight, 8 * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2) + (Vector3.back * deathSpacing) * deadBlacks.Count);
            }
        }

        pieces[x, y] = p;
        pieces[previousPos.x, previousPos.y] = null;

        PositionSinglePiece(x, y);

        isWhite = !isWhite;
        moveList.Add(new Vector2Int[] { previousPos, new Vector2Int(x, y)});

        ProcessSpecialMove();
        if(CheckForCheckMate()) {
            CheckMate(p.team);
        }

        return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo) {
        for(int x = 0; x < TILE_COUNT_X; x++) {
            for(int y = 0; y < TILE_COUNT_Y; y++) {
                if(tiles[x, y] == hitInfo) {
                    return new Vector2Int(x, y);
                }
            }
        }

        return -Vector2Int.one;//Invalid
    }
}
