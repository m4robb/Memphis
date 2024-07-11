using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[ExecuteInEditMode]
public class ScenarioStructure
{ 
    public GameObject ScenarioContainer;
    public string ScenarioName;
}
public class ScenarioSwitcher : MonoBehaviour
{
    ProbeReferenceVolume ProbeRefVolume;
    public ScenarioStructure[] Scenarios;
    public int ScenarioIndex = 0;

    int CurrentIndex;

    void Start()
    {
        CurrentIndex = ScenarioIndex;
        ProbeRefVolume =ProbeReferenceVolume.instance;
        Invoke("SetScenario", .1f);
    }

    void SetScenario()
    {
        ProbeRefVolume.lightingScenario = Scenarios[ScenarioIndex].ScenarioName;
        Debug.Log(Scenarios[ScenarioIndex].ScenarioName);
    }

    public void DoSwitch(int _Index)
    {
        ScenarioIndex = _Index;
    }
 
    void Update()
    {
        if(CurrentIndex != ScenarioIndex)
        {
            Scenarios[CurrentIndex].ScenarioContainer.SetActive(false);
            Scenarios[ScenarioIndex].ScenarioContainer.SetActive(true);
            ProbeRefVolume.lightingScenario = Scenarios[ScenarioIndex].ScenarioName;
            CurrentIndex = ScenarioIndex;

        }
    }
}
