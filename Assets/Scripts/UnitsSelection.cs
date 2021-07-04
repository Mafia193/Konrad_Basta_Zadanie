using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitsSelection : MonoBehaviour {

    [SerializeField] RectTransform selectionBox;

    Unit selectedUnit;

    Vector2 startPosition;
    float width;
    float height;

    void Awake() {
        Assert.IsNotNull(selectionBox);
    }

    // Start is called before the first frame update
    void Start() {
        selectionBox.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            startPosition = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);

            if (selectedUnit != null) {
                selectedUnit.Unselect();
                selectedUnit = null;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                selectedUnit = hit.collider.GetComponent<Unit>();
                if (selectedUnit != null) {
                    selectedUnit.Select();
                }
            }
        }

        if (Input.GetMouseButton(0)) {
            drawSelectionBox();
        }

        if (Input.GetMouseButtonUp(0)) {
            selectionBox.gameObject.SetActive(false);
        }
    }

    void drawSelectionBox() {
        width = Input.mousePosition.x - startPosition.x;
        height = Input.mousePosition.y - startPosition.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBox.anchoredPosition = new Vector2(startPosition.x + width / 2, startPosition.y + height / 2);
    }
}
