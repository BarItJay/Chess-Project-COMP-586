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

        if(attack) {
            GameObject piece = controller.GetComponent<Game>().GetPosition(xPos, yPos);
            Destroy(piece);
        }
        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Pieces>().GetXPos(), reference.GetComponent<Pieces>().GetYPos());
        reference.GetComponent<Pieces>().SetXPos(xPos);
        reference.GetComponent<Pieces>().SetYPos(yPos);
        reference.GetComponent<Pieces>().SetCoords();

        controller.GetComponent<Game>().SetPosition(reference);
        reference.GetComponent<Pieces>().DestroyMovePlates();
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
