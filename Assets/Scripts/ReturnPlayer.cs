using UnityEngine;
using System.Collections;

public class ReturnPlayer : MonoBehaviour
{

	public Transform ReturnPoint;

	public void OnCollisionEnter(Collision other)
	{
		other.transform.position = ReturnPoint.position;

	}
	public void OnTriggerEnter(Collider other)
	{
		other.transform.position = ReturnPoint.position;

	}
}