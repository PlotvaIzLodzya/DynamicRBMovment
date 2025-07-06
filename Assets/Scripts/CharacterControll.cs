using System;
using System.Collections;
using UnityEngine;

using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    [field: SerializeField] public float MaxSpeed { get; private set; } = 10f;
    [field: SerializeField] public float AccelerationTime { get; private set; } = 0.4f;
    [field: SerializeField] public float StopTime { get; private set; } = 0.2f;
    
    [SerializeField] private ContactFilter2D _contactFilter;

    private bool _isOnSlope;
    private bool _inTest;
    private float _deccelerationPerFrame;
    private float _accelerationPerFrame;
    private ContactPoint2D[] _points;
    private Vector2 _direction;
    private Rigidbody2D _rb;

    public float Speed { get; private set; }
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        _points = new ContactPoint2D[5];
        _rb = GetComponent<Rigidbody2D>();
        _rb.linearDamping = 0f;
        _deccelerationPerFrame = (MaxSpeed/StopTime) * Time.fixedDeltaTime;
        _accelerationPerFrame = (MaxSpeed/AccelerationTime) * Time.fixedDeltaTime;
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
        var forceMagnitude = CalculateForce(AccelerationTime, MaxSpeed, _rb.mass);
        var normal = GetNormal();
        var direction = GetDirectionAlongSurface(_direction, normal);
        var slopeCounterForce = GetSlopeForce(normal);
        var force = HandleSlope(direction * forceMagnitude, slopeCounterForce, normal);
        force = ApplyBreak(force, slopeCounterForce);
        
        _rb.AddForce(force);
        
        _rb.linearVelocityX = Mathf.Clamp(_rb.linearVelocityX, -MaxSpeed, MaxSpeed);
        Speed = _rb.linearVelocity.magnitude;
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
                var dirToPoint = contact.point - _rb.position;
                var dir = _rb.linearVelocity.GetHorizontal();
                
                if (dir.IsSameDirection(dirToPoint))
                    normal = contact.normal;
            }
        }
        
        return normal;
    }

    private float CalculateForce(float accelerationTime, float targetSpeed, float mass, float currentSpeed = 0f)
    {
        var forceMagnitude = mass * (targetSpeed - currentSpeed) / accelerationTime ;
        
        // Compensation for damping = 1, i don't if it will be needed
        // var dampingCompensation = Speed * mass * (1-Time.fixedDeltaTime);
        // forceMagnitude += dampingCompensation;
        
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
            _rb.linearVelocity = force.normalized * (Speed + _accelerationPerFrame);
        }
        
        return force + slopeCounterForce;
    }

    private Vector2 ApplyBreak(Vector2 force, Vector2 slopeCounterForce)
    {
        if (_direction.sqrMagnitude > 0.01f || IsGrounded == false)
            return force;
        
        if (_rb.linearVelocity.sqrMagnitude <= _deccelerationPerFrame)
            _rb.linearVelocity = Vector2.zero;
        
        if(_rb.linearVelocity.sqrMagnitude > _deccelerationPerFrame)
            force = _rb.linearVelocity.normalized * CalculateForce(StopTime,0,_rb.mass, MaxSpeed) + slopeCounterForce;
        
        return force;
    }

    private Vector2 GetSlopeForce(Vector2 normal)
    {
        var tangent = Vector2.Perpendicular(normal); 
        var gravityAlongSlope = -Physics2D.gravity.y * _rb.gravityScale * normal.x;
        var force = tangent * (_rb.mass * gravityAlongSlope);
        
        return force;
    }
}
