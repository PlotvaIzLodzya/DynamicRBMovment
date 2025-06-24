using System;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    [field: SerializeField] public float MaxSpeed { get; private set; } = 10f;
    [field: SerializeField] public float AccelerationTime { get; private set; } = 0.4f;
    
    [SerializeField] private ContactFilter2D _contactFilter;

    private bool _isOnSlope;
    private bool _inTest;
    private ContactPoint2D[] _points;
    private Vector2 _direction;
    private Rigidbody2D _rb;

    public float Speed { get; private set; }
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
        var force = HandleSlope(direction * forceMagnitude, slopeCounterForce, normal);
        
        _rb.AddForce(force);
        Speed = _rb.linearVelocity.magnitude;
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
        
        if(contactsCount == 0)
            return Vector2.up;
        
        if (contactsCount == 1)
            return _points[0].normal;
        
        var normal = GetNormalFromContacts(_points, contactsCount);
        
        Array.Clear(_points, 0, _points.Length);
        
        return normal.normalized;
    }

    private Vector2 GetNormalFromContacts(ContactPoint2D[] contacts, int contactCount)
    {
        var normal = Vector2.up;
        
        if (_rb.linearVelocity.sqrMagnitude > 0.01f || _direction.sqrMagnitude > 0.01f)
        {
            for (int i = 0; i < contactCount; i++)
            {
                var contact = contacts[i];
                normal = contact.normal;
                var dirToPoint = contact.point - _rb.position;
                var dir = _rb.linearVelocity.GetHorizontal();
                
                if (dir.IsSameDirection(dirToPoint))
                    break;
            }
        }
        
        return normal;
    }

    private float CalculateForce(float accelerationTime, float targetSpeed, float mass, float linearDamping, float currentSpeed = 0f)
    {
        var exponent = Mathf.Exp(- linearDamping/mass * accelerationTime);
        var denominator = 1f - exponent;
        var forceMagnitude = (targetSpeed - currentSpeed * exponent) * (mass * linearDamping) / denominator;
        
        return forceMagnitude;
    }

    private Vector2 HandleSlope(Vector2 force, Vector2 slopeCounterForce, Vector2 surfaceNormal)
    {
        var wasOnSlope = _isOnSlope;
        _isOnSlope = Mathf.Abs(surfaceNormal.x) > 0 && IsGrounded;
        var enteredSlope = _isOnSlope && wasOnSlope == false;
        var exitSlope = _isOnSlope == false && wasOnSlope;
        
        if (enteredSlope || exitSlope)
        {
            return force.normalized * CalculateForce(Time.fixedDeltaTime, Speed, _rb.mass, _rb.linearDamping, _rb.linearVelocity.magnitude) + slopeCounterForce;
        }
        
        return force + slopeCounterForce;
    }

    private Vector2 GetSlopeForce(Vector2 normal)
    {
        var tangent = Vector2.Perpendicular(normal); 
        var gravityAlongSlope = -Physics2D.gravity.y * _rb.gravityScale * normal.x;
        var force = tangent * (_rb.mass * gravityAlongSlope);
        
        return force;
    }
}
