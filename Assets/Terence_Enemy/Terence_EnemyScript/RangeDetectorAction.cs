using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RangeDetector", story: "Update Range [Detector] and Assign [Target]", category: "Action", id: "538427234b85429754dda957c7c6afa7")]
public partial class RangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyRangeDetector> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

  

    protected override Status OnUpdate()
    {
        return Detector.Value.UpdateDetector() == null ? Status.Failure:Status.Success ;
    }

 
}

