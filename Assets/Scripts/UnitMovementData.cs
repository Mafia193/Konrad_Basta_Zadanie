using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovementData {
    public Vector3 distanceFromCenter;
    public Vector3 endingPosition;
    public GameObject ring;
    public GameObject directionRing;
    public SpriteRenderer ringRenderer;

    public UnitMovementData(Vector3 DistanceFromCenter, GameObject Ring, GameObject DirectionRing) : this(DistanceFromCenter, DistanceFromCenter, Ring, DirectionRing) { }
    public UnitMovementData(Vector3 DistanceFromCenter, Vector3 EndPosition, GameObject Ring, GameObject DirectionRing) {
        distanceFromCenter = DistanceFromCenter;
        endingPosition = EndPosition;
        ring = Ring;
        directionRing = DirectionRing;
        directionRing.SetActive(false);
        ringRenderer = ring.GetComponentInChildren<SpriteRenderer>();
    }

    public void SetRingPosition(float hight) {
        ring.transform.position = new Vector3(endingPosition.x, hight, endingPosition.z);
    }

    public void SetDirectionRingPosition() {
        directionRing.transform.position = ring.transform.position;
    }

    public void SetDirectionRingQuaternion(Vector3 destinationPoint) {
        directionRing.transform.rotation = Quaternion.LookRotation(destinationPoint);
    }

    public void SetColor(Color color) {
        ringRenderer.color = color;
    }
}
