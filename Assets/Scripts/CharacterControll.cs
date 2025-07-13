using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private float _gravityScale;
    private float _defaultGravityScale;
    private float _rbGravityScale;
    private float _deccelerationPerFrame;
    private float _accelerationPerFrame;
    private ContactPoint2D[] _points;
    private Vector2 _direction;
    private Vector2 _velocity;
    private Rigidbody2D _rb;

    public float Speed { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsLanded { get; private set; }

    private void Awake()
    {
        Application.targetFrameRate = 144;
        _rb = GetComponent<Rigidbody2D>();
        _rbGravityScale = _rb.gravityScale;
        _defaultGravityScale = 20f;
        _gravityScale = _defaultGravityScale;
        _points = new ContactPoint2D[5];
        _rb.linearDamping = 0f;
        _rb.gravityScale = 0f;
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
        var contactsCount = _rb.GetContacts(_contactFilter, _points);
        var wasGrounded = IsGrounded;
        IsGrounded = contactsCount > 0;
        
        var forceMagnitude = CalculateForce(AccelerationTime, MaxSpeed, _rb.mass);
        var normal = GetNormal(_points, contactsCount);
        var direction = GetDirectionAlongSurface(_direction, normal);
        var slopeCounterForce = GetSlopeForce(normal);
        var force = HandleSlope(direction * forceMagnitude, slopeCounterForce, normal);
        force = ApplyBreak(force, slopeCounterForce);
        force += Vector2.down * (Mathf.Abs(Physics2D.gravity.y) * _gravityScale * _rb.mass);
        var nextStepGrounded = CheckNextStep(force);
        _gravityScale = nextStepGrounded ? _defaultGravityScale : _rbGravityScale;
        
        _rb.AddForce(force);
        
        _rb.linearVelocityX = Mathf.Clamp(_rb.linearVelocityX, -MaxSpeed, MaxSpeed);
        Speed = _rb.linearVelocity.magnitude;
        _velocity = _rb.linearVelocity;
        IsLanded = IsGrounded && wasGrounded == false;
    }

    private Vector2 GetDirectionAlongSurface(Vector2 direction, Vector2 normal)
    {
        return Vector2.Perpendicular(normal) * -direction.x;
    }

    private Vector2 GetNormal(ContactPoint2D[] points, int contactsCount)
    {
        if(contactsCount == 0)
            return Vector2.up;
        
        if (contactsCount == 1)
            return points[0].normal;
        
        var normal = GetNormalFromContacts(points, contactsCount);
        
        Array.Clear(points, 0, points.Length);
        
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
        
        // Compensation for damping = 1, i don't know if it will be needed
        // var dampingCompensation = Speed * mass * (1-Time.fixedDeltaTime);
        // forceMagnitude += dampingCompensation;
        
        return forceMagnitude;
    }

    private bool CheckNextStep(Vector2 force)
    {
        var points = new RaycastHit2D[5];
        var count = _rb.Cast(force.normalized, _contactFilter, points, distance: 0.01f);
        
        return  count > 0;
    }

    private Vector2 HandleSlope(Vector2 force, Vector2 slopeCounterForce, Vector2 surfaceNormal)
    {
        var wasOnSlope = _isOnSlope;
        _isOnSlope = Mathf.Abs(surfaceNormal.x) > 0 && IsGrounded;
        var enteredSlope = _isOnSlope && wasOnSlope == false;
        var exitSlope = _isOnSlope == false && wasOnSlope;
        
        if (IsGrounded && IsLanded == false && (enteredSlope || exitSlope))
        {
            if (_rb.linearVelocity.IsSameDirection(_direction))
            {
                var speedDif = _rb.mass * (Speed - _rb.linearVelocity.magnitude) + _accelerationPerFrame;
                var impulse = force.normalized * speedDif;
                impulse.y -= Physics2D.gravity.y * _gravityScale * _rb.mass * Time.fixedDeltaTime;
                _rb.AddForce(impulse, ForceMode2D.Impulse);
            }
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
        var gravityAlongSlope = -Physics2D.gravity.y * _gravityScale * normal.x;
        var force = tangent * (_rb.mass * gravityAlongSlope);
        
        return force;
    }
}
