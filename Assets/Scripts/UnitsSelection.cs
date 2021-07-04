using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsSelection : MonoBehaviour {

    private Unit selectedUnit;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            if (selectedUnit != null) {
                selectedUnit.Unselect();
                selectedUnit = null;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                selectedUnit = hit.collider.GetComponent<Unit>();
                if (selectedUnit != null) {
                    selectedUnit.Select();
                }
            }

        }
    }
}
