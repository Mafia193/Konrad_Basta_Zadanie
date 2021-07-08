using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitsSelection : MonoBehaviour {

    [SerializeField] RectTransform selectionBox;    // It is used to sellect units.
    [SerializeField] LayerMask unitLayerMask;
    
    [SerializeField] [Range(0f, 0.1f)] [Tooltip ("The interval between the selectionBox refreshes.")] 
    float refreshTime = 0.05f;

    Unit selectedUnit;
    public List<Unit> selectedUnits { get; private set; } = new List<Unit>();
    bool selectionStarted;  // Is set when the units selection begins.

    Vector2 startPosition;  // Start point of selectionBox.
    float width;    // Width selectionBox.
    float height;   // Height selectionBox.

    Coroutine SelectionUpdate;

    void Awake() {
        Assert.IsNotNull(selectionBox);
        Assert.AreNotEqual(0, unitLayerMask, "No layer selected for selecting units.");
    }

    void Start() {
        selectionBox.gameObject.SetActive(false);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1)) {
            selectionStarted = true;
            startPosition = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);
            unselectAllUnits();
            selectOneUnit();    // Allows to choose the unit you click on.
            SelectionUpdate = StartCoroutine(selectionUpdate());
        }

        if (selectionStarted && Input.GetMouseButtonUp(0)) {
            StopCoroutine(SelectionUpdate);
            GameManager.Instance.UpdateUnitsMovementData();
            selectionBox.gameObject.SetActive(false);
            selectionStarted = false;
        }
    }

    void unselectAllUnits() {
        foreach (Unit unit in selectedUnits)
            unit.Unselect();

        selectedUnit = null;
        selectedUnits.Clear();
    }
    void selectOneUnit() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitLayerMask)) {
            selectedUnit = hit.collider.GetComponent<Unit>();
            selectedUnit.Select();
            selectedUnits.Add(selectedUnit);
        }
    }
    IEnumerator selectionUpdate() {
        while (Input.GetMouseButton(0)) {
            drawSelectionBox();
            selectUnits();
            yield return new WaitForSeconds(refreshTime);
        }
    }

    void drawSelectionBox() {
        width = Input.mousePosition.x - startPosition.x;
        height = Input.mousePosition.y - startPosition.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBox.anchoredPosition = new Vector2(startPosition.x + width / 2, startPosition.y + height / 2);
    }

    void selectUnits() {
        Vector2 min = selectionBox.anchoredPosition - selectionBox.sizeDelta / 2;   // Borderline points of selectionBox.
        Vector2 max = selectionBox.anchoredPosition + selectionBox.sizeDelta / 2;

        foreach (Unit unit in GameManager.Instance.Units) {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y) { // Unit is located inside selectionBox. 
                if ((unitLayerMask.value & (1 << unit.gameObject.layer)) != 0) {
                    if (!selectedUnits.Contains(unit)) {
                        selectedUnits.Add(unit);
                        unit.Select();
                    }
                }
            } else if (unit.IsSelected && unit != selectedUnit) {
                unit.Unselect();
                selectedUnits.Remove(unit);
            }
        }
    }
}
