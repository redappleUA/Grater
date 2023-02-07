using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Movement : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] float _speed = 5f;
    [SerializeField] float _sensetive = 4000f;
    [SerializeField, Range(0f, 0.01f)] float _extentsToDestroy = 0.003f;

    private Spawner _spawner;
    private Transform _startPoint;
    private Transform _endPoint;
    private Rigidbody _rb;
    private Vector3 _graterNormal;
    private bool _inGrater = false;
    private Vector3 _startPosition;
    private Vector3 _direction;
    private float _moveDirection = 0; 

    private void Start()
    {
        _spawner = FindObjectOfType<Spawner>();
        _rb = GetComponent<Rigidbody>();
        _startPoint = GameObject.FindGameObjectWithTag("Start Point").transform;
        _endPoint = GameObject.FindGameObjectWithTag("End Point").transform;

        //Checking if object too small
        if (GetComponent<MeshFilter>().mesh.bounds.extents.x < _extentsToDestroy) _spawner.Spawn(gameObject);
    }

    void FixedUpdate()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.y /= Screen.width;

        if (Input.GetMouseButtonDown(0))
            _startPosition = mouse;

        //Checking the touch
        if (Input.GetMouseButton(0))
        {
            _moveDirection = mouse.y - _startPosition.y;
            _startPosition = mouse;

            _direction = (_endPoint.position - transform.position).normalized;

        }
        if (Input.GetMouseButtonUp(0))
        {
            _moveDirection = 0;
        }

        //Choosing the direction
        _direction = _moveDirection > 0 ? (_startPoint.position - transform.position).normalized :
            _moveDirection < 0 ? (_endPoint.position - transform.position).normalized : Vector3.zero;
        _direction *= _sensetive;

        //To press to the grater
        if (!_inGrater)
            _rb.AddForce(Vector3.left * 1000 * Time.deltaTime, ForceMode.Acceleration);

        //Moving
        if (_inGrater)
            _rb.AddForce(_direction * _speed * Time.fixedDeltaTime, ForceMode.Acceleration);

        CheckRotation();
    }

    void CheckRotation()
    {
        if (transform.rotation != Quaternion.Euler(_spawner.Rotate))
            transform.rotation = Quaternion.Euler(_spawner.Rotate);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Slicer"))
            _inGrater = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Slicer"))
        {
            // Get the normal vector of the grater's surface
            _graterNormal = collision.contacts[0].normal;

            // Project the vegetable's velocity onto the grater's normal to find the velocity along the grater's surface
            Vector3 velocityAlongGrater = Vector3.Project(_rb.velocity, _graterNormal);

            // Apply the velocity along the grater's surface to the vegetable
            _rb.velocity = velocityAlongGrater * Time.deltaTime;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Slicer"))
            _inGrater = false;
    }
}
