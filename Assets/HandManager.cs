using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    // Start is called before the first frame update
    public SkinnedMeshRenderer HandMesh;

    public GameObject  DefaultPrefab;

    SphereCollider[] FingerPointArray;

    public void BuildHand(GameObject _NewPrefab)
    {
        foreach(SphereCollider FingerPoint in FingerPointArray)
        {
            //Transform[] _GOArray = FingerPoint.gameObject.GetComponentsInChildren<Transform>();

            foreach(Transform _GO in FingerPoint.transform) Destroy(_GO.gameObject);

            GameObject _TGO = Instantiate(_NewPrefab, FingerPoint.transform);
            float _TScale = Random.Range(.001f, .004f);
            FingerPointSizer _FPS = _TGO.GetComponent<FingerPointSizer>();
            if (_FPS!= null)
            {
                _TScale = Random.Range(_FPS.Min, _FPS.Max);
            }

            _TGO.transform.localPosition = new Vector3(0, 0, 0);
            _TGO.transform.localScale = new Vector3(_TScale, _TScale, _TScale);

        }
    }

    private void Start()
    {
        HandMesh.enabled = true;
        FingerPointArray = GetComponentsInChildren<SphereCollider>();
        BuildHand(DefaultPrefab);
    }

}
