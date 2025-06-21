using UnityEngine;

namespace Tests
{
    public enum AccelerationTestType
    {
        Straight,
        EnteringSlope,
        ExitingSlope,
        SlideDown,
        SlideUp,
    }
    
    public class TestObstacle : MonoBehaviour
    {
        [field: SerializeField] public AccelerationTestType TestType { get; private set; }
    }
}