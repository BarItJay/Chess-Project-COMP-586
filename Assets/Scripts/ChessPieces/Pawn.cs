using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Pawn : Pieces {
    public override List<Vector2Int> GetAvailableMoves(ref Pieces[,] board, int tileCountX, int tileCountY) {
        List<Vector2Int> r = new List<Vector2Int>();

        int dir = (team == 0) ? 1 : -1;

        //One in front
        if(board[currentX, currentY + dir] == null) {
            r.Add(new Vector2Int(currentX, currentY + dir));
        } 

        //Two in front
        if(board[currentX, currentY + dir] == null) {
            if(team == 0 && currentY == 1 && board[currentX, currentY + (dir*2)] == null) {
                r.Add(new Vector2Int(currentX, currentY + (dir*2)));
            }
            if(team == 1 && currentY == 6 && board[currentX, currentY + (dir*2)] == null) {
                r.Add(new Vector2Int(currentX, currentY + (dir*2)));
            }
        }

        //Capture
        if(currentX != tileCountX - 1) {
            if(board[currentX + 1, currentY + dir] != null && board[currentX + 1, currentY + dir].team != team) {
                r.Add(new Vector2Int(currentX + 1, currentY + dir));
            } 
        }
        
        if(currentX != 0) {
            if(board[currentX - 1, currentY + dir] != null && board[currentX - 1, currentY + dir].team != team) {
                r.Add(new Vector2Int(currentX - 1, currentY + dir));
            } 
        }

        return r;
    }

}
