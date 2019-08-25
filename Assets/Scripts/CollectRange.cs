using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectRange : MonoBehaviour
{
    float range;
    List<Collider> collectableObject = new List<Collider>();
    int count;
    void Update()
    {
        //set range
        range = transform.localScale.x / 2;

        //check if an object is within the range
        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "collectable")
            {
                if (!collectableObject.Contains(colliders[i]))
                {
                    collectableObject.Add(colliders[i]);
                }
            }
        }

        //check if the the object goes out the range and if so remove it from the list
        for (int i = 0; i < colliders.Length; i++)
        {
            if (Vector3.Distance(transform.position, colliders[i].transform.position) > range)
            {
                collectableObject.Remove(colliders[i]);
            }
        }
    }

    public List<Collider> GetList()
    {
        return collectableObject;
    }

    public void ClearList()
    {
        collectableObject = new List<Collider>();
    }

    public void RemoveFromList(Collider col)
    {
        collectableObject.Remove(col);
    }
}
