using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitsController : MonoBehaviour {

    [SerializeField] UnitsSelection unitsSelection;
    [SerializeField] LayerMask layerMask;
    [SerializeField] GameObject Ring;

    List<UnitMovementData> unitMoveDatas = new List<UnitMovementData>();

    Vector3 startMousePostion;
    Vector3 destination;
    Vector3 center = Vector3.zero;

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
            startMousePostion = Input.mousePosition;
            if (selectionId != unitsSelection.SelectionID) {
                selectionId = unitsSelection.SelectionID;

                setCenter();

                clearUnitsMovesDatas();
                foreach (Unit unit in unitsSelection.selectedUnits) {
                    unitMoveDatas.Add(new UnitMovementData(unit, unit.transform.position - center, Instantiate(Ring) as GameObject));
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
                destination = hit.point;
            }

            activateRings();
        }

        if (Input.GetMouseButton(1) && !Input.GetMouseButton(0)) {
            drawPositions();
        } else {
            diactivateRings();
        }

        if (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0)) {
            for (int i = unitMoveDatas.Count; i > 0;) {
                UnitMovementData unitData = unitMoveDatas[--i];
                unitData.unit.SetDestination(unitData.endPosition);
                unitData.distanceFromCenter = unitData.endPosition - destination;
            }
        }
    }

    void setCenter() {
        center = Vector3.zero;
        foreach (Unit unit in unitsSelection.selectedUnits) {
            center += unit.transform.position;
        }
        center /= unitsSelection.selectedUnits.Count;
    }
    void activateRings() {
        foreach (UnitMovementData unitData in unitMoveDatas)
            unitData.ring.SetActive(true);
    }

    void diactivateRings() {
        foreach (UnitMovementData unitData in unitMoveDatas)
            unitData.ring.SetActive(false);
    }

    void drawPositions() {
        Vector3 movement = Input.mousePosition - startMousePostion;
        float angle = Vector3.Angle(Vector3.up, movement);
        Vector3 Angle = new Vector3(0, movement.x > 0 ? angle : -angle, 0);

        for (int i = unitMoveDatas.Count; i > 0;) {
            UnitMovementData unitData = unitMoveDatas[--i];

            unitData.endPosition = destination + unitData.distanceFromCenter;
            rotatePointAroundPivot(ref unitData.endPosition, destination, Angle);
            unitData.setRing(destination.y + 0.01f);
        }
    }

    void rotatePointAroundPivot(ref Vector3 point, Vector3 pivot, Vector3 angles) {
        point = Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    void clearUnitsMovesDatas() {
        foreach (UnitMovementData unitData in unitMoveDatas)
            Destroy(unitData.ring.gameObject);
        unitMoveDatas.Clear();
    }

    class UnitMovementData {
        public readonly Unit unit;
        public Vector3 distanceFromCenter;
        public Vector3 endPosition;
        public GameObject ring;

        public UnitMovementData(Unit Unit, Vector3 DistanceFromCenter, GameObject Ring) : this(Unit, DistanceFromCenter, DistanceFromCenter, Ring) { }
        public UnitMovementData(Unit Unit, Vector3 DistanceFromCenter, Vector3 EndPosition, GameObject Ring) {
            unit = Unit;
            distanceFromCenter = DistanceFromCenter;
            endPosition = EndPosition;
            ring = Ring;
        }

        public void setRing(float hight) {
            ring.transform.position = new Vector3(endPosition.x, hight, endPosition.z); ;
        }
    }
}
