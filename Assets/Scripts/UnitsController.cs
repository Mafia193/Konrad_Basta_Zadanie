using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitsController : MonoBehaviour {

    [SerializeField] UnitsSelection unitsSelection;
    Dictionary<Unit, Vector3> unitsPositions = new Dictionary<Unit, Vector3>();

    Vector3 destination;
    Vector3 center = Vector3.zero;
    int selectionId;

    void Awake() {
        Assert.IsNotNull(unitsSelection);
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0)) {
            if (selectionId != unitsSelection.SelectionID) {
                selectionId = unitsSelection.SelectionID;

                center = Vector3.zero;
                foreach (Unit unit in unitsSelection.selectedUnits) {
                    center += unit.transform.position;
                }
                center /= unitsSelection.selectedUnits.Count;

                unitsPositions.Clear();
                foreach (Unit unit in unitsSelection.selectedUnits) {
                    unitsPositions.Add(unit, unit.transform.position - center);
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                destination = hit.point;
                foreach (var unit in unitsPositions) {
                    unit.Key.SetDestination(destination + unit.Value);
                }
                
            }
        }

    }
}
