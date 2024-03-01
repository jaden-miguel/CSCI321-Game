using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class TorchMovement : MonoBehaviour
{

    public float speed = 5f;
    public float height = 0.5f;
    private bool pickedUp = false;
    public GameObject parentObj;

    void Update()
    {
        if (!pickedUp)
        {
            Vector3 pos = transform.position;
            float newY = (Mathf.Sin(Time.time * speed) * height) + 0.5f;
            this.transform.position = new Vector3(pos.x, newY, pos.z);
        }
        else
        {
            //this.transform.parent = parentObj.transform;
            //Vector3 pos = transform.position;
            //this.transform.position = new Vector3(pos.x, pos.y + 0.01f, pos.z);

            Vector3 pos = parentObj.transform.position;
            this.transform.position = new Vector3(pos.x, pos.y + 0.077f, pos.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pickedUp = true;
            this.transform.eulerAngles = new Vector3(0, 0, 0);
            //this.transform.parent = parentObj.transform;
            //Vector3 pos = transform.position;
            //this.transform.position = new Vector3(pos.x + 0.2f, pos.y + 1.04f, pos.z + 0.2f);
        }
    }
}
