using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    [SerializeField] Color selectedColor;
    [SerializeField] Color unselectedColor;

    Renderer renderer;
    public bool IsSelected { get; private set; }

    // Start is called before the first frame update
    void Start() {
        renderer = GetComponent<Renderer>();
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
}
