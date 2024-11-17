using System;
using UnityEngine;

public class ChessBoard : MonoBehaviour {
    //Logic
    private const int TILE_COUNT_X = 8, TILE_COUNT_Y = 8;
    private GameObject[,] tiles;

    private void Awake() {
        GenerateAllTiles(1, TILE_COUNT_X, TILE_COUNT_Y);

    }

    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY) {
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

        return tileObj;
    }
}
