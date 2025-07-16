using UnityEngine;
using Unity.Behavior;

public class ExclaimationMarkTrigger : MonoBehaviour
{
    [SerializeField] private GameObject agent;
    //[SerializeField] private ExclamationMark exclamtionMark;

    private BlackboardVariable m_targetDetected; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BehaviorGraphAgent agentBehavior = agent.GetComponent<BehaviorGraphAgent>();
        if (agentBehavior == null)
        {
            return;
        }
        agentBehavior.BlackboardReference.GetVariable("TargetDetected", out m_targetDetected);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_targetDetected ==null)
        {
            return;
        }
        bool targetDetected = (bool)m_targetDetected.ObjectValue;
       
    }
}
