using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitsSelection : MonoBehaviour {

    [SerializeField] RectTransform selectionBox;
    [SerializeField] LayerMask unitLayerMask;

    Unit selectedUnit;
    List<Unit> selectedUnits = new List<Unit>();

    Vector2 startPosition;
    float width;
    float height;

    void Awake() {
        Assert.IsNotNull(selectionBox);
        Assert.AreNotEqual(0, unitLayerMask, "No layer selected for selecting units.");
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
            unselectAllUnits();
            selectOneUnit();
        }

        if (Input.GetMouseButton(0)) {
            drawSelectionBox();
            selectUnits();
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

    void selectOneUnit() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitLayerMask)) {
            selectedUnit = hit.collider.GetComponent<Unit>();
            selectedUnit.Select();
        }
    }

    void selectUnits() {
        Vector2 min = selectionBox.anchoredPosition - selectionBox.sizeDelta / 2;
        Vector2 max = selectionBox.anchoredPosition + selectionBox.sizeDelta / 2;

        foreach (Unit unit in GameManager.Instance.Units) {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y) {
                if ((unitLayerMask.value & (1 << unit.gameObject.layer)) != 0) {
                    selectedUnits.Add(unit);
                    unit.Select();
                }
            } else if (unit.IsSelected && unit != selectedUnit) {
                unit.Unselect();
            }
        }
    }

    void unselectAllUnits() {
        if (selectedUnit != null) {
            selectedUnit.Unselect();
            selectedUnit = null;
        }

        foreach (Unit unit in selectedUnits)
            unit.Unselect();

        selectedUnits.Clear();
    }
}
