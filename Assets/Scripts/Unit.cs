using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Unit : MonoBehaviour {

    [SerializeField] Color selectedUnitColor;
    [SerializeField] Color unselectedUnitColor;
    [SerializeField] GameObject Ring;
    [SerializeField] GameObject DirectionRing;

    Renderer renderer;
    NavMeshAgent navMeshAgent;

    public UnitMovementData Data { get; set; }

    Coroutine RotateUnit;
    public bool IsSelected { get; private set; }
    bool rotateAtTheEnd;    // After making a move the unit turns towards turningDirection.
    Quaternion turningDirecion;
    public Quaternion TurningDirection {
        get => turningDirecion;
        set {
            turningDirecion = value;
            rotateAtTheEnd = true;
        }
    }

    void Awake() {
        Assert.IsNotNull(Ring);
        Assert.IsNotNull(DirectionRing);
    }

    void Start() {
        GameManager.Instance.RegisterUnit(this);
        renderer = GetComponent<Renderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        loadMovementData();
        Unselect();
        TurningDirection = transform.rotation;
    }

    void loadMovementData() {
        Data = new UnitMovementData(Vector3.zero, Instantiate(Ring) as GameObject, Instantiate(DirectionRing) as GameObject);
        Data.ring.SetActive(false);
        Data.directionRing.SetActive(false);
    }

    public void UpdateMovementData(Vector3 center) {
        Data.distanceFromCenter = transform.position - center;
    }

    public void Select() {
        IsSelected = true;
        if (!GameManager.Instance.SelectedUnits.Contains(this))
            GameManager.Instance.SelectedUnits.Add(this);
        renderer.material.color = selectedUnitColor;
    }

    public void Unselect() {
        IsSelected = false;
        if (GameManager.Instance.SelectedUnits.Contains(this))
            GameManager.Instance.SelectedUnits.Remove(this);
        renderer.material.color = unselectedUnitColor;
    }

    public void SetDestination(Vector3 destination) {
        navMeshAgent.destination = destination;
        if (RotateUnit != null)
            StopCoroutine(RotateUnit);
        RotateUnit = StartCoroutine(rotateUnit());
    }

    IEnumerator rotateUnit() {
        while (true) {
            if (navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon) {
                if (!navMeshAgent.updateRotation)
                    navMeshAgent.updateRotation = true;
            } else if (rotateAtTheEnd) {
                // Taking control over the unit rotation.
                if (navMeshAgent.updateRotation)
                    navMeshAgent.updateRotation = false;
                float angle = navMeshAgent.angularSpeed / 36 * Time.deltaTime;  // Adjusting the angular speed to the angular speed of NavMeshAgent.
                if (Quaternion.Angle(transform.rotation, turningDirecion) < angle) {
                    transform.rotation = turningDirecion;
                    rotateAtTheEnd = false;
                    navMeshAgent.updateRotation = true;
                    break;
                } else {
                    transform.rotation = Quaternion.Lerp(transform.rotation, turningDirecion, angle);
                }
            }
            yield return null;
        }
    }
}
