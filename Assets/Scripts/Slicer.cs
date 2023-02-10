using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EzySlice;

public class Slicer : MonoBehaviour
{
    [SerializeField] GameObject _rubbedObjectPrefab;
    [SerializeField, Range(0f, 15f)] float _timeToDestroy = 5;
    [SerializeField] Transform _spawningPoint;
    [SerializeField] PhysicMaterial _material;
    [SerializeField] Material _crossSectionMaterial;
    [SerializeField] ParticleSystem _cutEffectsPrefab;
    [SerializeField, Range(0f, 15f)] float _sizeOfCutEffects = 8;
    [SerializeField, Range(0f, 15f)] float _timeForWate = 1;

    private Transform _cutPlane;
    private GameObject _sliceObject;
    private ParticleSystem _cutEffects;
    private static bool _objectInTrigger = false;

    private void Start()
    {
        _cutPlane = gameObject.transform;
        _cutEffects = Instantiate(_cutEffectsPrefab, transform.position, Quaternion.identity, transform);
        _cutEffects.transform.localScale = Vector3.one * _sizeOfCutEffects;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sliceable") && !_objectInTrigger)
        {
            Debug.Log("Wait");
            _objectInTrigger = true;
            _sliceObject = other.gameObject;
            Slice(other.gameObject);
            _cutEffects.Play();
        }
        StartCoroutine(Wait(_timeForWate));
    }

    private void OnTriggerExit(Collider other) => _objectInTrigger = false;

    private void Slice(GameObject cutObject)
    {
        if (cutObject.CompareTag("Sliceable"))
        {
            SlicedHull hull = SliceObject(cutObject, _crossSectionMaterial);
            if (hull != null)
            {
                GameObject bottom = hull.CreateLowerHull(cutObject);
                GameObject top = hull.CreateUpperHull(cutObject);

                top.tag = "Sliceable";
                AddHullComponents(top);
                var rb = top.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
                rb.interpolation = RigidbodyInterpolation.Interpolate;

                Destroy(bottom);
                Destroy(cutObject);

                StartCoroutine(SpawningRubbedObjects());
            }
        }
    }

    /// <summary>
    /// Slice the object per plane
    /// </summary>
    /// <param name="obj">Object to slice</param>
    /// <param name="crossSectionMaterial">Material on cross section</param>
    /// <returns>Sliced object</returns>
    public SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial)
    {
        // slice the provided object using the transforms of this object
        if (obj.GetComponent<MeshFilter>() == null)
            return null;

        return obj.Slice(_cutPlane.position, _cutPlane.up, crossSectionMaterial);
    }

    /// <summary>
    /// Adding the Rigidbody and MeshCollider components
    /// </summary>
    /// <param name="go"></param>
    public void AddHullComponents(GameObject go)
    {
        go.layer = 9;
        go.AddComponent<Movement>();
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.None;
        rb.mass = 100;
        MeshCollider collider = go.AddComponent<MeshCollider>();
        collider.convex = true;
        collider.material = _material;

        rb.AddExplosionForce(100, go.transform.position, 20);
    }

    /// <summary>
    /// Wall spawn rubbed vegetables
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawningRubbedObjects()
    {
        Material material;
        material  = _sliceObject.GetComponent<MeshRenderer>().materials[0];

        for (int i = Random.Range(5, 10); i > 0; i--)
        {
            var go = Instantiate(_rubbedObjectPrefab, new Vector3(_spawningPoint.position.x + Random.value, _spawningPoint.position.y, _spawningPoint.position.z + Random.value),
                Quaternion.Euler(new Vector3(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180))));

            go.GetComponent<MeshRenderer>().material = material;
            StartCoroutine(DestroyAfterTime(go, _timeToDestroy));

            yield return new WaitForSeconds(Random.value / 2);
        }
    }

    IEnumerator DestroyAfterTime(GameObject goToDestroy, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(goToDestroy);
    }

    IEnumerator Wait(float timeForWate)
    {
        yield return new WaitForSeconds(timeForWate);
    }
}