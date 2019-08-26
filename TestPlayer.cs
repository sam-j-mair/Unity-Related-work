using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    Rigidbody m_rigidBody;
    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody>();
        m_rigidBody.isKinematic = true;
    }

    public void MoveTo(Vector3 position)
    {
        m_rigidBody.MovePosition(position);
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
}
