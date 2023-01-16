using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static bool academyStepped;
    
    public List<Environment> environments;
    private List<Environment> unrequestedEnvironments = new List<Environment>();
    private List<Environment> uncompletedEnvironments = new List<Environment>();

    private void Awake()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;

        foreach (Environment environment in environments)
        {
            environment.OnEnvironmentRequestedDecision += HandleEnvironmentRequestedDecision;
            environment.OnEnvironmentGameCompleted += HandleEnvironmentGameCompleted;
        }
    }

    private void OnDestroy()
    {
        foreach (Environment environment in environments.Where(environment => environment != null))
        {
            environment.OnEnvironmentRequestedDecision -= HandleEnvironmentRequestedDecision;
            environment.OnEnvironmentGameCompleted -= HandleEnvironmentGameCompleted;
        }
    }

    private void Start()
    {
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            Reset();
            
            foreach (Environment environment in environments)
            {
                StartCoroutine(environment.PlayAGame());
            }

            yield return new WaitUntil(() => uncompletedEnvironments.Count < 1);
            yield return null;
        }
    }
    
    private void HandleEnvironmentRequestedDecision(Environment environment)
    {
        unrequestedEnvironments.Remove(environment);

        if (unrequestedEnvironments.Count < 1)
        {
            Academy.Instance.EnvironmentStep();
            academyStepped = true;
        }
    }
    
    private void HandleEnvironmentGameCompleted(Environment environment)
    {
        uncompletedEnvironments.Remove(environment);
    }

    private void Reset()
    {
        academyStepped = false;
        unrequestedEnvironments = new List<Environment>(environments);
        uncompletedEnvironments = new List<Environment>(environments);
    }
}