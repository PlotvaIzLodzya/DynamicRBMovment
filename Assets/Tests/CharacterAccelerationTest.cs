using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public abstract class  CharacterAccelerationTest
    {
        protected abstract  AccelerationTestType TestType { get; }
        
        [UnityTest]
        public IEnumerator AccelerationTestWithEnumeratorPasses()
        {
            yield return SceneManager.LoadSceneAsync("SampleScene");
            Time.timeScale = AccelerationTestSceneConfiguration.TestTimeScale;
            var character = GameObject.FindAnyObjectByType<CharacterControl>();
            var obstacleConfiguration = GameObject.FindAnyObjectByType<AccelerationTestSceneConfiguration>();
            
            obstacleConfiguration.EnableObstacle(TestType);
            
            var acceleration =  new CharacterAcceleration();
        
            yield return acceleration.Accelerating(character);
            AssertAcceleration(character, acceleration.ElapsedTime);
        }
    
        protected abstract void AssertAcceleration(CharacterControl character, float elapsedTime);
    }
}