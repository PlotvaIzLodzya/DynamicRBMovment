using System.Linq;
using UnityEngine;

namespace Tests
{
    public class AccelerationTestSceneConfiguration : MonoBehaviour
    {
        public const float TestTimeScale = 10f;
        
        private TestObstacle[] _obstacles;

        public void EnableObstacle(AccelerationTestType testType)
        {
            _obstacles = GetComponentsInChildren<TestObstacle>(true);
            foreach (var obstacle in _obstacles)
            {
                obstacle.gameObject.SetActive(false);
            }
            
            _obstacles.First(obstacle => obstacle.TestType == testType).gameObject.SetActive(true);
        }
    }
}