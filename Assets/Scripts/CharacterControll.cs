using System;
using System.Collections;
using UnityEngine;

public class CharacterControll : MonoBehaviour
{
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _accelerationTime;
    [SerializeField] private float _stopTime;
    [SerializeField] private ContactFilter2D _contactFilter;
    
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
    }

    private void FixedUpdate()
    {
        var forceMagnitude = _rb.mass * (_maxSpeed / _accelerationTime + _rb.linearDamping * _maxSpeed);
        
        var normal = GetNormal();
        var direction = GetDirectionAlongSurface(_direction, normal);
        var slopeCounterForce = GetSlopeForce(normal);
        var force = direction * forceMagnitude + slopeCounterForce;
        Debug.DrawRay(_rb.position, force, Color.red);
        _rb.AddForce(force);
        
        _rb.linearVelocityX = Mathf.Clamp(_rb.linearVelocityX, -_maxSpeed, _maxSpeed);
    }

    private Vector2 GetDirectionAlongSurface(Vector2 direction, Vector2 normal)
    {
        return Vector2.Perpendicular(normal) * -direction.x;
    }

    private Vector2 GetNormal()
    {
        _rb.GetContacts(_contactFilter, _points);
        var points = new RaycastHit2D[5];
        var direction = _direction.sqrMagnitude > 0.01f ? _direction : Vector2.down;
        direction = _rb.linearVelocity.normalized;
        _rb.Cast(direction, _contactFilter, points, 0.2f);
        
        var normal = Vector2.zero;
        foreach (var contact in points)
        {
            normal += contact.normal;
        }

        normal = normal == Vector2.zero ? Vector2.up : normal;
        Array.Clear(_points, 0, _points.Length);
        
        return normal.normalized;
    }

    private Vector2 GetSlopeForce(Vector2 normal)
    {
        var tangent = new Vector2(-normal.y, normal.x); 
        var gravityAlongSlope = -Physics2D.gravity.y * _rb.gravityScale * normal.x;
        var force = tangent * (_rb.mass * gravityAlongSlope);
        
        return force;
    }
}
