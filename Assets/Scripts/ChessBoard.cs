using System;
using UnityEngine;

public class ChessBoard : MonoBehaviour {
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Prefabs & Material")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    //Logic
    private Pieces[,] pieces;
    private const int TILE_COUNT_X = 8, TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private void Awake() {
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
    int layerMask = LayerMask.GetMask("Tile", "Hover");

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
            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
            currentHover = hitPosition;
            tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
        }
    } else {
        // If not hovering over any tile
        if (currentHover != -Vector2Int.one) {
            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
            currentHover = -Vector2Int.one;
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
        pieces[x, y].transform.position = GetTileCenter(x, y);
    }

    private Vector3 GetTileCenter(int x, int y) {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2);
    }

    //Operations
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
