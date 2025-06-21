using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class AccelerationTest
{
    private readonly float _testTimeScale = 10f;
    
    [UnityTest]
    public IEnumerator AccelerationTestWithEnumeratorPasses()
    {
        yield return SceneManager.LoadSceneAsync("SampleScene");
        Time.timeScale = _testTimeScale;
        var character = GameObject.FindAnyObjectByType<CharacterControl>();
        SetPrivateField(character, "_inTest", true);
        
        yield return FixedUpdating(character);
    }
    
    private IEnumerator FixedUpdating(CharacterControl character)
    {
        var elapsedTime = 0f;
        yield return new WaitUntil(() => character.IsGrounded);
        yield return new WaitUntil(() => character.Speed < 0.01f);
        
        SetPrivateField(character, "_direction", Vector2.right);
        
        while (elapsedTime <= character.AccelerationTime && character.Speed <= character.MaxSpeed)
        {
            yield return new WaitForFixedUpdate();
            elapsedTime += Time.fixedDeltaTime;
        }
        
        Debug.Log($"Time: {elapsedTime}, Speed: {character.Speed}");
        
        if (elapsedTime < character.AccelerationTime)
        {
            Assert.AreEqual(
                elapsedTime,
                character.AccelerationTime,
                0.05f);
        }
        
        Assert.AreEqual(
            character.MaxSpeed,
            Mathf.Abs(character.Speed),
            0.5f);
    }
    
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(obj, value);
    }
}
