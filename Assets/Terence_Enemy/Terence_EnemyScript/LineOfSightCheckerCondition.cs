using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line of Sight Checker", story: "Check [Target] with Line Of Sight [Detector]", category: "Conditions", id: "2d894293787f33e8e2c9022503251d1d")]
public partial class LineOfSightCheckerCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<EnemyLineOfSightDetector> Detector;

    public override bool IsTrue()
    {
        return Detector.Value.PerformDetection(Target.Value)!= null;
    }

}
