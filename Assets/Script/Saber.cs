using UnityEngine;

public class Saber : MonoBehaviour
{
    public LayerMask layer;
    Vector3 prevPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
    if (Physics.Raycast(transform.position, transform.forward,
    out hit, 1, layer))

    {
        if (Vector3.Angle(transform.position - prevPos, hit.transform.up) > 130)
        {
            Destroy(hit.transform.gameObject);
        }
    }
    prevPos = transform.position;
        
    }
}
