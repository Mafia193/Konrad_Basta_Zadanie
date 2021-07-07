using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitsSelection : MonoBehaviour {

    [SerializeField] RectTransform selectionBox;
    [SerializeField] LayerMask unitLayerMask;
    [SerializeField] [Range(0f, 0.1f)] float refreshTime = 0.05f;

    Unit selectedUnit;
    public List<Unit> selectedUnits { get; private set; } = new List<Unit>();
    bool selectionStarted;

    Vector2 startPosition;
    float width;
    float height;
    public int SelectionID { get; private set; }

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
            ++SelectionID;
            startPosition = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);
            unselectAllUnits();
            selectOneUnit();
            SelectionUpdate = StartCoroutine(selectionUpdate());
        }

        if (selectionStarted && Input.GetMouseButtonUp(0)) {
            StopCoroutine(SelectionUpdate);
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
        Vector2 min = selectionBox.anchoredPosition - selectionBox.sizeDelta / 2;
        Vector2 max = selectionBox.anchoredPosition + selectionBox.sizeDelta / 2;

        foreach (Unit unit in GameManager.Instance.Units) {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y) {
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
