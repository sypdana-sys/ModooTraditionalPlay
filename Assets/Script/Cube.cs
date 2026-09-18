using UnityEngine;

public class Cube : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = (transform.forward - transform.up).normalized;
        transform.position += Time.deltaTime * dir * 2;
    }
}
