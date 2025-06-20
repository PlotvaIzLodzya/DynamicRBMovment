using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _accelerationTime;
    [SerializeField] private float _stopTime;
    [SerializeField] private ContactFilter2D _contactFilter;
    
    private bool _isOnSteepSlope;
    private float _elapsedTime;
    private ContactPoint2D[] _points;
    private Vector2 _direction;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _points = new ContactPoint2D[5];
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _direction = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            _direction += Vector2.left;
        if (Input.GetKey(KeyCode.D))
            _direction += Vector2.right;

        if (Input.GetKeyDown(KeyCode.D))
        {
            _elapsedTime = 0;
        }
    }

    private void FixedUpdate()
    {
        var exponent = Mathf.Exp(-_rb.linearDamping/_rb.mass * _accelerationTime);
        var denominator = 1f - exponent;
        var forceMagnitude = _maxSpeed * (_rb.mass * _rb.linearDamping) / denominator;
        
        var normal = GetNormal();
        var direction = GetDirectionAlongSurface(_direction, normal);
        var slopeCounterForce = GetSlopeForce(normal);
        
        var force = direction * forceMagnitude + slopeCounterForce;
        Debug.DrawRay(_rb.position, force, Color.red);
        
        _rb.AddForce(force);
        _rb.linearVelocityX = Mathf.Clamp(_rb.linearVelocityX, -_maxSpeed, _maxSpeed);
        if (force.magnitude > 0 && _elapsedTime > _accelerationTime)
        {
            Debug.Log($"time: {_elapsedTime} speed: {_rb.linearVelocity.magnitude}");
        }
        _elapsedTime += Time.fixedDeltaTime;
    }

    private Vector2 GetDirectionAlongSurface(Vector2 direction, Vector2 normal)
    {
        return Vector2.Perpendicular(normal) * -direction.x;
    }

    private Vector2 GetNormal()
    {
        var contactsCount = _rb.GetContacts(_contactFilter, _points);
        
        var normal = Vector2.zero;

        if (_direction.sqrMagnitude > 0)
        {
            foreach (var contact in _points)
            {
                var dirToPoint = contact.point - _rb.position;

                if (IsSameDirection(_direction, dirToPoint))
                {
                    normal = contact.normal;
                    Debug.DrawRay(_rb.position, contact.normal, Color.blue);
                    break;
                }
            }
        }

        if(normal == Vector2.zero)
            normal = _points[0].normal;
        
        Debug.DrawRay(_rb.position, normal.normalized, Color.yellow);
        
        Array.Clear(_points, 0, _points.Length);
        
        return normal.normalized;
    }

    private Vector2 GetSlopeForce(Vector2 normal)
    {
        var tangent = Vector2.Perpendicular(normal); 
        var gravityAlongSlope = -Physics2D.gravity.y * _rb.gravityScale * normal.x;
        var force = tangent * (_rb.mass * gravityAlongSlope);
        
        return force;
    }
    
    private bool IsSameDirection(Vector3 vectorA, Vector3 vectorB)
    {
        return Vector3.Angle(vectorA, vectorB) < 90;
    }
}
