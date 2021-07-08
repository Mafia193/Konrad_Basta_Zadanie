using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitsController : MonoBehaviour {

    [SerializeField] UnitsSelection unitsSelection;
    [SerializeField] LayerMask walkableMask;
    [SerializeField] LayerMask obstaclesMask;
    [SerializeField] [Range(0f, 0.1f)] [Tooltip("The interval between updating rings (units) positions.")]
    float refreshTime = 0.05f;
    [SerializeField] Color defaultColor;    // Used when all units' positions are correct.
    [SerializeField] Color incorrectColor;  // Used when not all units' positions are correct.
    [SerializeField] Color collisionColor;  // Used for units which positions are not correct.
    [SerializeField] [Tooltip("Offset of the ray from the target position for collision detection. The offset has to be a positive number.")]
    float collisionRayDistance = 100f;
    [SerializeField] [Tooltip("Ring offset from surface.")]
    float ringOffset = 0.01f;

    List<Unit> selectedUnits;
    bool destinationSettingStarted; // Is set after choosing units destination.
    bool rotationSettingStarted;    // Is set after choosing units rotation in relation to mid-point.
    bool allRingsAreSetCorrectly;   // Is set after all units target positions are not colliding with an obsticle and are not located outside of the scene. 

    Vector3 startMousePostion;
    Vector3 destination;
    Quaternion turningDirection;   // Turn direction of units after reaching destination.

    Coroutine UpdateEndPosition;
    Coroutine UpdateEndRotation;

    void Awake() {
        Assert.IsNotNull(unitsSelection);
        Assert.AreNotEqual(0, walkableMask, "No layer selected for walkable objects.");
        Assert.AreNotEqual(0, obstaclesMask, "No layer selected for obstacles.");
        Assert.IsTrue(collisionRayDistance >= 0, "The offset has to be a positive number."); 
    }

    void Start() {
        selectedUnits = GameManager.Instance.SelectedUnits;
    }

    void Update() {
        if (Input.GetKey(KeyCode.Escape)) { // Canceling the selection of units target.
            if (UpdateEndRotation != null)
                StopCoroutine(UpdateEndRotation);
            if (UpdateEndPosition != null)
                StopCoroutine(UpdateEndPosition);
            diactivateRings();
            diactivateDirectionRings();
            destinationSettingStarted = false;
            rotationSettingStarted = false;
            return;
        }

        if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if ((obstaclesMask & (1 << hit.collider.gameObject.layer)) == 0) {  // Clicking on obstacle disables from making a move.
                    if ((walkableMask & (1 << hit.collider.gameObject.layer)) == 0) {   // If failed to hit the walkableMask, check whether selected place is a part of walkableMask.
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, walkableMask)) {
                            setDestination(hit);
                        }
                    } else {
                        setDestination(hit);
                    }
                }
            }
        }

        if (destinationSettingStarted && !rotationSettingStarted) {
            if (Input.GetMouseButtonDown(0) && allRingsAreSetCorrectly) {   // Start setting the viewing direction of the units.
                rotationSettingStarted = true;
                StopCoroutine(UpdateEndPosition);
                setDirectionRingsPosition();
                diactivateRings();
                activateDirectionRings();
                UpdateEndRotation = StartCoroutine(updateEndRotation());
            } else if (Input.GetMouseButtonUp(1)) { // Setting units rotation in relation to mid-point.
                StopCoroutine(UpdateEndPosition);
                if (allRingsAreSetCorrectly)
                    setUnitsDestination();
                diactivateRings();
                destinationSettingStarted = false;
            }
        }

        if (rotationSettingStarted && Input.GetMouseButtonUp(0)) {  // The end of the target choosing procedure.
            StopCoroutine(UpdateEndRotation);
            if (allRingsAreSetCorrectly)
                setUnitsDestinationAndRotation();
            diactivateRings();
            diactivateDirectionRings();
            destinationSettingStarted = false;
            rotationSettingStarted = false;
        }
    }

    void setDestination(RaycastHit hit) {
        destinationSettingStarted = true;
        destination = hit.point;
        startMousePostion = Input.mousePosition;
        activateRings();
        UpdateEndPosition = StartCoroutine(updateEndPositions());
    }

    IEnumerator updateEndPositions() {
        while (Input.GetMouseButton(1)) {
            Vector3 movement = Input.mousePosition - startMousePostion;
            float angle = Vector3.Angle(Vector3.up, movement);
            Vector3 Angle = new Vector3(0, movement.x > 0 ? angle : -angle, 0);

            for (int i = selectedUnits.Count; i > 0;) {
                Unit unit = selectedUnits[--i];

                unit.Data.endingPosition = destination + unit.Data.distanceFromCenter;
                rotatePointAroundPivot(ref unit.Data.endingPosition, destination, Angle);
                unit.Data.SetRingPosition(destination.y + ringOffset);

            }
            AllRingsAreSetCorrectly();
            yield return new WaitForSeconds(refreshTime);
        }
    }

    void rotatePointAroundPivot(ref Vector3 point, Vector3 pivot, Vector3 angles) {
        point = Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    IEnumerator updateEndRotation() {
        while (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                Vector3 rotationVector = (hit.point - destination).normalized;
                turningDirection = Quaternion.LookRotation(rotationVector);
                setDirectionRingsQuaternion(new Vector3(hit.point.x, 0, hit.point.z));
            }
            yield return new WaitForSeconds(refreshTime);
        }
    }

    void AllRingsAreSetCorrectly() {
        bool ringsAreSetCorrectly = true;
        foreach (Unit unit in selectedUnits) {
            Ray ray = new Ray(unit.Data.ring.transform.position + new Vector3(0, collisionRayDistance, 0), Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if ((obstaclesMask & (1 << hit.collider.gameObject.layer)) != 0) {
                    ringsAreSetCorrectly = false;
                    unit.Data.SetColor(collisionColor);
                } else if (allRingsAreSetCorrectly) {
                    unit.Data.SetColor(defaultColor);
                } else {
                    unit.Data.SetColor(incorrectColor);
                }
            } else {
                ringsAreSetCorrectly = false;
                unit.Data.SetColor(collisionColor);
            }
        }
        allRingsAreSetCorrectly = ringsAreSetCorrectly;
        return;
    }

    void activateRings() {
        foreach (Unit unit in selectedUnits)
            unit.Data.ring.SetActive(true);
    }

    void diactivateRings() {
        foreach (Unit unit in selectedUnits)
            unit.Data.ring.SetActive(false);
    }

    void activateDirectionRings() {
        foreach (Unit unit in selectedUnits)
            unit.Data.directionRing.SetActive(true);
    }

    void diactivateDirectionRings() {
        foreach (Unit unit in selectedUnits)
            unit.Data.directionRing.SetActive(false);
    }

    public void setDirectionRingsPosition() {
        foreach (Unit unit in selectedUnits)
            unit.Data.SetDirectionRingPosition();
    }

    public void setDirectionRingsQuaternion(Vector3 point) {
        foreach (Unit unit in selectedUnits)
            unit.Data.SetDirectionRingQuaternion(point - destination);
    }

    void setUnitsDestination() {
        for (int i = selectedUnits.Count; i > 0;) {
            Unit unit = selectedUnits[--i];

            unit.SetDestination(unit.Data.endingPosition);
            unit.Data.distanceFromCenter = unit.Data.endingPosition - destination;
        }
    }

    void setUnitsDestinationAndRotation() {
        for (int i = selectedUnits.Count; i > 0;) {
            Unit unit = selectedUnits[--i];

            unit.TurningDirection = turningDirection;
            unit.SetDestination(unit.Data.endingPosition);
            unit.Data.distanceFromCenter = unit.Data.endingPosition - destination;
        }
    }
}
