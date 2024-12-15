using System.Collections.Generic;
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

    public override SpecialMove GetSpecialMoves(ref Pieces[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int dir = (team == 0) ? 1 : -1;

        //Promotion
        if((team == 0 && currentY == 6) || (team == 1 && currentY == 1)) {
            return SpecialMove.Promotion;
        }

        //En Passant
        if(moveList.Count > 0) {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if(board[lastMove[1].x, lastMove[1].y].type == PieceType.Pawn) {//If last piece moved was a pawn
                if(Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2) {//If the last moved was 2 forward
                    if(board[lastMove[1].x, lastMove[1].y].team != team) {//If the move was made by opponent
                        if(lastMove[1].y == currentY) {//If both pawns have the same y position
                            if(lastMove[1].x == currentX - 1) {//Left side
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + dir));
                                return SpecialMove.EnPassant;
                            }
                            if(lastMove[1].x == currentX + 1) {//Right side
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + dir));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }
               
        return SpecialMove.None;
    }

}
