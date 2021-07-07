using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour {

    [SerializeField] Color selectedColor;
    [SerializeField] Color unselectedColor;

    Renderer renderer;
    NavMeshAgent navMeshAgent;
    public bool IsSelected { get; private set; }
    bool newTarget;
    Quaternion finalTurn;
    public Quaternion FinalTurn {
        get => finalTurn;
        set {
            finalTurn = value;
            newTarget = true;
        }
    }

    // Start is called before the first frame update
    void Start() {
        GameManager.Instance.RegisterUnit(this);
        renderer = GetComponent<Renderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        Unselect();
        FinalTurn = transform.rotation;
    }

    // Update is called once per frame
    void Update() {

    }

    void LateUpdate() {
        if (navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon) {
            if (!navMeshAgent.updateRotation)
                navMeshAgent.updateRotation = true;
        } else if (newTarget) {
            if (navMeshAgent.updateRotation)
                navMeshAgent.updateRotation = false;

            float angle = navMeshAgent.angularSpeed / 36 * Time.deltaTime;
            if (Quaternion.Angle(transform.rotation, finalTurn) < angle) {
                transform.rotation = finalTurn;
                newTarget = false;
                navMeshAgent.updateRotation = true;
            } else {
                transform.rotation = Quaternion.Lerp(transform.rotation, finalTurn, angle);
            }
        }
    }

    public void Select() {
        IsSelected = true;
        renderer.material.color = selectedColor;
    }

    public void Unselect() {
        IsSelected = false;
        renderer.material.color = unselectedColor;
    }

    public void SetDestination(Vector3 destination) {
        navMeshAgent.destination = destination;
    }
}
