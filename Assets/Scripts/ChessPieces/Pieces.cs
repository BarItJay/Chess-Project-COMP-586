using Unity.VisualScripting;
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

    private Vector3 desiredPos, desiredScale = Vector3.one;

    private void Update() {
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual void SetPos(Vector3 pos, bool force = false) {
        desiredPos = pos;
        if(force) {
            transform.position = desiredPos;
        }
    }

    public virtual void SetScale(Vector3 scale, bool force = false) {
        desiredScale = scale;
        if(force) {
            transform.localScale = desiredScale;
        }
    }



}
