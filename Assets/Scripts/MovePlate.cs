using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;
    GameObject reference = null;
    int xPos;
    int yPos;
    public bool attack = false;

    public void Start() {
        if(attack) {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f,0f,0f,1.0f);
        }
    }

    public void OnMouseUp() {
        controller = GameObject.FindGameObjectWithTag("GameController");

        Pieces p = reference.GetComponent<Pieces>();

        if(attack) {
            GameObject piece = controller.GetComponent<Game>().GetPosition(xPos, yPos);
            Destroy(piece);
        }

        controller.GetComponent<Game>().SetPositionEmpty(p.GetXPos(), p.GetYPos());
        p.SetXPos(xPos);
        p.SetYPos(yPos);
        p.SetCoords();

        if(reference.name.EndsWith("Pawn")) {
            p.isFirstMove = false;
        }

        controller.GetComponent<Game>().SetPosition(reference);
        p.DestroyMovePlates();

        controller.GetComponent<Game>().NextTurn();
    }

    public void SetCoords(int x, int y) {
        xPos = x;
        yPos = y;
    }

    public void SetReference(GameObject obj) {
        reference = obj;
    }

    public GameObject GetReference() {
        return reference;
    }
}
