using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw : MonoBehaviour
{
    Camera cam;
    [SerializeField]
    LayerMask layerMask;
    [SerializeField]
    CollectRange collectRange;
    [SerializeField]
    GameObject trail, colliderHolder;
    int count;
    int numPoints;
    float pointDelayTimer;
    float lineWidth;
    bool check;

    GameObject center;
    GameObject lastPoint;
    List<Collider> objectsInRange;
    List<Vector3> points;
    List<GameObject> pointsObjects;
    struct LinesData
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
    }

    // Use this for initialization
    void Start()
    {
        lineWidth = .2f;
        points = new List<Vector3>();
        pointsObjects = new List<GameObject>();
        cam = Camera.main;
        count = 0;
    }


    // Update is called once per frame
    void Update()
    {
        //draw lines and set points
        if (Input.GetButton("Fire1"))
        {
            //move on the drawable locations
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask))
            {
                transform.position = new Vector3(hit.point.x, hit.point.y + .5f, hit.point.z);
            }
            //delay the points drawn
            pointDelayTimer -= Time.deltaTime;
            if (pointDelayTimer <= 0)
            {
                GameObject point = new GameObject();
                if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask))
                {
                    point.transform.position = new Vector3(hit.point.x, 0, hit.point.z);
                }
                if (numPoints >= 2)
                {
                    //make collider
                    CapsuleCollider collider = point.AddComponent<CapsuleCollider>();
                    collider.radius = lineWidth;
                    collider.center = Vector3.zero;
                    collider.direction = 2;
                    collider.transform.position = point.transform.position + (lastPoint.transform.position - point.transform.position) / 2;
                    point.transform.LookAt(lastPoint.transform.position);
                    collider.height = (lastPoint.transform.position - point.transform.position).magnitude;
                    collider.transform.parent = colliderHolder.transform;
                }
                numPoints++;
                points.Add(point.transform.position);
                point.tag = "point";
                point.layer = 9;
                point.transform.parent = colliderHolder.transform;
                lastPoint = point;
                pointsObjects.Add(point);
                pointDelayTimer = .1f;
                trail.SetActive(true);
            }
        }
        else
        {
            trail.SetActive(false);
        }

        //check if anything is within the drawn area
        if (Input.GetButtonUp("Fire1"))
        {
            objectsInRange = collectRange.GetList();

            //set middle point of the circel
            List<float> x = new List<float>();
            List<float> y = new List<float>();
            for (int i = 0; i < points.Count; i++)
            {
                x.Add(points[i].x);
                y.Add(points[i].z);
            }

            float minX = CheckMin(x);
            float maxX = CheckMax(x);
            float minY = CheckMin(y);
            float maxY = CheckMax(y);
            float centerX = (minX + maxX) / 2;
            float centerY = (minY + maxY) / 2;
            center = new GameObject();
            center.transform.position = new Vector3(centerX, 0, centerY);
            center.name = "center";
            x.Clear();
            y.Clear();
            //check if inside or ouside
            check = true;
        }

        //check the number of hits even outside oneven inside
        if (check == true)
        {
            List<Collider> colliders = new List<Collider>();
            for (int i = 0; i < objectsInRange.Count; i++)
            {
                if (objectsInRange[i] == null)
                {
                    objectsInRange.Remove(objectsInRange[i]);
                    return;
                }
                List<RaycastHit> hits = new List<RaycastHit>();
                foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(objectsInRange[i].transform.position.x, center.transform.position.y, objectsInRange[i].transform.position.z), center.transform.position, float.PositiveInfinity))
                {
                    if (hit.transform.tag == "point")
                    {
                        if (hits.Count <= 1 || Vector3.Distance(hits[i].point, hits[i - 1].point) >= .1f)
                        {
                            hits.Add(hit);
                        }
                    }
                }
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Debug.Log(hits.Count);
                }
                if (hits.Count % 2 != 0)
                {
                    colliders.Add(objectsInRange[i]);
                }
            }

            //remove from list
            if (colliders.Count > 0)
            {
                for (int i = 0; i < colliders.Count; i++)
                {
                    objectsInRange.Remove(colliders[i]);
                    collectRange.RemoveFromList(colliders[i]);
                    colliders[i].GetComponent<Collectable>().Collect();
                }
            }

            //when done clear
            check = false;
            ClearAll();
        }
    }

    float CheckMin(List<float> min)
    {
        float lowest = 0;

        for (int i = 0; i < min.Count; i++)
        {
            lowest = min[i];
        }

        for (int i = 0; i < min.Count; i++)
        {
            if (min[i] < lowest)
            {
                lowest = min[i];
            }
        }

        return lowest;
    }

    float CheckMax(List<float> max)
    {
        float highest = 0;

        for (int i = 0; i < max.Count; i++)
        {
            if (highest == 0)
            {
                highest = max[i];
            }

            if (max[i] > highest)
            {
                highest = max[i];
            }
        }

        return highest;
    }

    void ClearAll()
    {
        points = new List<Vector3>();
        for (int i = 0; i < numPoints; i++)
        {
            Destroy(pointsObjects[i]);
        }
        pointsObjects = new List<GameObject>();
        numPoints = 0;
        collectRange.ClearList();
        Destroy(center);
    }
}