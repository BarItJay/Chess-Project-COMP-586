using UnityEngine;

public enum PieceType {
    None,
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

public class Pieces : MonoBehaviour {
    public int team, currentX, currentY;
    public PieceType type;

    private Vector3 desiredPos, desiredScale;
}
