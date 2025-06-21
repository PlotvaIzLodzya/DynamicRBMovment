using System.Collections;
using System.Reflection;
using UnityEngine;

public class CharacterAcceleration
{
    public float ElapsedTime { get; private set; }
    
    public IEnumerator Accelerating(CharacterControl character)
    {        
        SetPrivateField(character, "_inTest", true);
        ElapsedTime = 0f;
        yield return new WaitUntil(() => character.IsGrounded);
        yield return new WaitUntil(() => character.Speed < 0.01f);
        
        SetPrivateField(character, "_direction", Vector2.right);
        
        while (ElapsedTime <= character.AccelerationTime && character.Speed <= character.MaxSpeed)
        {
            yield return new WaitForFixedUpdate();
            ElapsedTime += Time.fixedDeltaTime;
        }
    }
    
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(obj, value);
    }
}