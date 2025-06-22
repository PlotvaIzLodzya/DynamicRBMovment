using System.Linq;
using UnityEngine;

namespace Tests
{
    public class AccelerationTestSceneConfiguration : MonoBehaviour
    {
        public const float TestTimeScale = 10f;
        
        [SerializeField] private TestObstacle[] _obstacles;

        public void EnableObstacle(AccelerationTestType testType)
        {
            foreach (var obstacle in _obstacles)
            {
                obstacle.gameObject.SetActive(false);
            }
            
            _obstacles.First(obstacle => obstacle.TestType == testType).gameObject.SetActive(true);
        }
    }
}