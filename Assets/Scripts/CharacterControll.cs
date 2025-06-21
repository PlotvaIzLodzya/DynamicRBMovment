using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    [field: SerializeField] public float MaxSpeed { get; private set; } = 10f;
    [field: SerializeField] public float AccelerationTime { get; private set; } = 0.4f;
    
    [SerializeField] private ContactFilter2D _contactFilter;

    private bool _inTest;
    private ContactPoint2D[] _points;
    private Vector2 _direction;
    private Rigidbody2D _rb;
    
    public float Speed => _rb.linearVelocity.magnitude;
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        _points = new ContactPoint2D[5];
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_inTest)
            return;
        
        _direction = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            _direction += Vector2.left;
        if (Input.GetKey(KeyCode.D))
            _direction += Vector2.right;
    }
    
    private void FixedUpdate()
    {
        var forceMagnitude = CalculateForce(AccelerationTime, MaxSpeed, _rb.mass, _rb.linearDamping);
        var normal = GetNormal();
        var direction = GetDirectionAlongSurface(_direction, normal);
        var slopeCounterForce = GetSlopeForce(normal);
        
        var force = direction * forceMagnitude + slopeCounterForce;
        Debug.DrawRay(_rb.position, force, Color.red);
        
        _rb.AddForce(force);
        _rb.linearVelocityX = Mathf.Clamp(_rb.linearVelocityX, -MaxSpeed, MaxSpeed);
    }

    private Vector2 GetDirectionAlongSurface(Vector2 direction, Vector2 normal)
    {
        return Vector2.Perpendicular(normal) * -direction.x;
    }

    private Vector2 GetNormal()
    {
        var contactsCount = _rb.GetContacts(_contactFilter, _points);
        IsGrounded = contactsCount > 0;
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

    private float CalculateForce(float accelerationTime, float maxSpeed, float mass, float linearDamping)
    {
        var exponent = Mathf.Exp(-linearDamping/mass * accelerationTime);
        var denominator = 1f - exponent;
        var forceMagnitude = maxSpeed * (mass * linearDamping) / denominator;
        
        return forceMagnitude;
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
