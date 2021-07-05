using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitsController : MonoBehaviour {

    [SerializeField] UnitsSelection unitsSelection;
    [SerializeField] LayerMask layerMask;
    [SerializeField] GameObject Ring;

    List<GameObject> Rings = new List<GameObject>();
    Dictionary<Unit, Vector3> unitsPositions = new Dictionary<Unit, Vector3>();

    Vector3 destination;
    Vector3 center = Vector3.zero;
    Vector3 startPosition;
    int selectionId;

    void Awake() {
        Assert.IsNotNull(unitsSelection);
        Assert.IsNotNull(Ring);
        Assert.AreNotEqual(0, layerMask);
    }

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0)) {
            startPosition = Input.mousePosition;

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
            createRings();
        }

        if (Input.GetMouseButton(1) && !Input.GetMouseButton(0)) {
            drawPositions();
        }

        if (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0)) {
            destroyRings();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
                destination = hit.point;
                foreach (var unit in unitsPositions) {
                    unit.Key.SetDestination(destination + unit.Value);
                }

            }
        }
    }

    void createRings() {
        foreach (Unit unit in unitsSelection.selectedUnits) {
            GameObject ring = Instantiate(Ring) as GameObject;
            Rings.Add(ring);
        }
    }

    void drawPositions() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
            destination = hit.point;
            int i = unitsPositions.Count;
            foreach (var unit in unitsPositions) {
                Vector3 position = destination + unit.Value;
                Rings[--i].transform.position = new Vector3(position.x, destination.y + 0.01f, position.z);
            }
        }
    }

    void destroyRings() {
        foreach (GameObject ring in Rings) {
            Destroy(ring.gameObject);
        }
        Rings.Clear();
    }
}
