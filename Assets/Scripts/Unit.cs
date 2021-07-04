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

    // Start is called before the first frame update
    void Start() {
        GameManager.Instance.RegisterUnit(this);
        renderer = GetComponent<Renderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        Unselect();
    }

    // Update is called once per frame
    void Update() {

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
