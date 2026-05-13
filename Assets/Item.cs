using UnityEngine;

public class Item : MonoBehaviour
{

    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    public void PickUp(Transform parent, Vector3 position)
    {
        rb.isKinematic = true;
        transform.SetParent(parent);
        transform.position = position;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        //weight
        //parent
    }

    public void Throw(float force, Vector3 direction)
    {
        rb.isKinematic = false;
        transform.SetParent(null);
        
        rb.AddForce(direction * force, ForceMode.Impulse);
    }
}
