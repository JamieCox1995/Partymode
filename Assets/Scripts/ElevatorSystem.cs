using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorSystem : MonoBehaviour
{
    public Transform[] elevatorFloors;
    public int floorIndex = 0;

    public Vector2 elevatorSpeed;
    public float stoppingDistance = 2f;

    private float distError = 0.001f;

    public GameObject elevator;

	// Use this for initialization
	void Start ()
    {
        // Setting the elevator to the floor position to the one in the inspector.
        elevator.transform.position = elevatorFloors[floorIndex].position;
	}

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartElevator(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartElevator(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartElevator(02);
        }
    }*/

    public void StartElevator(int newFloor)
    {
        floorIndex = newFloor;
        
        StartCoroutine(MoveElevator());
    }

    public void GoUp()
    {
        if (floorIndex >= elevatorFloors.Length - 1) return;

        floorIndex++;

        StartElevator(floorIndex);
    }

    public void GoDown()
    {
        if (floorIndex <= 0) return;

        floorIndex--;

        StartElevator(floorIndex);
    }

    public IEnumerator MoveElevator()
    {
        float distance = float.MaxValue;
        float moveSpeed = elevatorSpeed.y;

        while (distance >=  distError)
        {
            Vector3 dir = elevatorFloors[floorIndex].position - elevator.transform.position;
            distance = dir.magnitude;

            dir = dir.normalized;

            if (distance <= stoppingDistance)
            {
                moveSpeed = elevatorSpeed.y * (distance / stoppingDistance);

                //moveSpeed = Mathf.Clamp(moveSpeed, elevatorSpeed.x, elevatorSpeed.y);
            }

            elevator.transform.position += (dir * moveSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
