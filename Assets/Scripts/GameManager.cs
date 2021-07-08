using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance;

    public List<Unit> Units { get; private set; } = new List<Unit>();
    public List<Unit> SelectedUnits { get; private set; } = new List<Unit>();
    private Vector3 center; // The mid-point of selected units.

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    public void RegisterUnit(Unit unit) {
        Units.Add(unit);
    }

    public void UpdateUnitsMovementData() {
        setCenter();
        foreach (Unit unit in SelectedUnits) {
            unit.UpdateMovementData(center);
        }
    }

    void setCenter() {
        center = Vector3.zero;
        foreach (Unit unit in SelectedUnits) {
            center += unit.transform.position;
        }
        center /= SelectedUnits.Count;
    }
}
