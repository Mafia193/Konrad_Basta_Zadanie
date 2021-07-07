using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitsController : MonoBehaviour {

    [SerializeField] UnitsSelection unitsSelection;
    [SerializeField] LayerMask layerMask;
    [SerializeField] GameObject Ring;
    [SerializeField] GameObject DirectionRing;

    List<UnitMovementData> unitMoveDatas = new List<UnitMovementData>();
    bool destinationSettingStarted;
    bool rotationSettingStarted;

    Vector3 startMousePostion;
    Vector3 destination;
    Vector3 center = Vector3.zero;
    Quaternion finalTurn;

    int selectionId;

    void Awake() {
        Assert.IsNotNull(unitsSelection);
        Assert.IsNotNull(Ring);
        Assert.IsNotNull(DirectionRing);
        Assert.AreNotEqual(0, layerMask, "No layer selected for walkable objects.");
    }

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if ((layerMask & (1 << hit.collider.gameObject.layer)) != 0) {
                    destinationSettingStarted = true;
                    destination = hit.point;
                    startMousePostion = Input.mousePosition;
                    activateRings();

                    if (selectionId != unitsSelection.SelectionID) {
                        selectionId = unitsSelection.SelectionID;

                        setCenter();

                        clearUnitsMovesDatas();
                        foreach (Unit unit in unitsSelection.selectedUnits) {
                            unitMoveDatas.Add(new UnitMovementData(unit, unit.transform.position - center, Instantiate(Ring) as GameObject, Instantiate(DirectionRing) as GameObject));
                        }
                    }
                }
            }
        }

        if (destinationSettingStarted && !rotationSettingStarted) {
            if (Input.GetMouseButtonDown(0)) {
                rotationSettingStarted = true;
                setDirectionRingsPosition();
                diactivateRings();
                activateDirectionRings();
            } else {
                if (Input.GetMouseButton(1)) {
                    drawPositions();
                } else {
                    diactivateRings();
                }

                if (Input.GetMouseButtonUp(1)) {
                    setUnitsDestination();
                    destinationSettingStarted = false;
                }
            }
        }

        if (rotationSettingStarted) {
            if (Input.GetMouseButton(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    Vector3 rotationVector = (hit.point - destination).normalized;
                    finalTurn = Quaternion.LookRotation(rotationVector);
                    setDirectionRingsQuaternion(new Vector3(hit.point.x, 0, hit.point.z));
                }
            }

            if (Input.GetMouseButtonUp(0)) {
                setUnitsDestinationAndRotation();
                diactivateRings();
                diactivateDirectionRings();
                destinationSettingStarted = false;
                rotationSettingStarted = false;
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

    void activateDirectionRings() {
        foreach (UnitMovementData unitData in unitMoveDatas)
            unitData.directionRing.SetActive(true);
    }

    void diactivateDirectionRings() {
        foreach (UnitMovementData unitData in unitMoveDatas)
            unitData.directionRing.SetActive(false);
    }

    public void setDirectionRingsPosition() {
        foreach (UnitMovementData unitData in unitMoveDatas)
            unitData.setDirectionRing();
    }

    public void setDirectionRingsQuaternion(Vector3 point) {
        foreach (UnitMovementData unitData in unitMoveDatas)
            unitData.setDirectionRingQuaternion(point - destination);
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
        foreach (UnitMovementData unitData in unitMoveDatas) {
            Destroy(unitData.ring.gameObject);
            Destroy(unitData.directionRing.gameObject);
        }
        unitMoveDatas.Clear();
    }

    void setUnitsDestination() {
        for (int i = unitMoveDatas.Count; i > 0;) {
            UnitMovementData unitData = unitMoveDatas[--i];
            unitData.unit.SetDestination(unitData.endPosition);
            unitData.distanceFromCenter = unitData.endPosition - destination;
        }
    }

    void setUnitsDestinationAndRotation() {
        for (int i = unitMoveDatas.Count; i > 0;) {
            UnitMovementData unitData = unitMoveDatas[--i];
            unitData.unit.SetDestination(unitData.endPosition);
            unitData.distanceFromCenter = unitData.endPosition - destination;
            unitData.unit.FinalTurn = finalTurn;
        }
    }

    class UnitMovementData {
        public readonly Unit unit;
        public Vector3 distanceFromCenter;
        public Vector3 endPosition;
        public GameObject ring;
        public GameObject directionRing;

        public UnitMovementData(Unit Unit, Vector3 DistanceFromCenter, GameObject Ring, GameObject DirectionRing) : this(Unit, DistanceFromCenter, DistanceFromCenter, Ring, DirectionRing) { }
        public UnitMovementData(Unit Unit, Vector3 DistanceFromCenter, Vector3 EndPosition, GameObject Ring, GameObject DirectionRing) {
            unit = Unit;
            distanceFromCenter = DistanceFromCenter;
            endPosition = EndPosition;
            ring = Ring;
            directionRing = DirectionRing;
            directionRing.SetActive(false);
        }

        public void setRing(float hight) {
            ring.transform.position = new Vector3(endPosition.x, hight, endPosition.z);
        }

        public void setDirectionRing() {
            directionRing.transform.position = ring.transform.position;
        }

        public void setDirectionRingQuaternion(Vector3 destinationPoint) {
            directionRing.transform.rotation = Quaternion.LookRotation(destinationPoint);
        }
    }
}
