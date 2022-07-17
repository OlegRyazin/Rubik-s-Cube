using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubikManager : MonoBehaviour
{
    public GameObject PartOfCube;
    public Transform MainCubeTrans;
    List<GameObject> AllCubeParts = new List<GameObject>();
    GameObject CubeCenterPart;
    bool canRotate = true;
    bool canShuffle = true;

    List<GameObject> UpPart
    {
        get
        {
            return AllCubeParts.FindAll(x => Mathf.Round(x.transform.localPosition.y) == 0);
        }
    }
    List<GameObject> DownPart
    {
        get
        {
            return AllCubeParts.FindAll(x => Mathf.Round(x.transform.localPosition.y) == -2);
        }
    }
    List<GameObject> FrontPart
    {
        get
        {
            return AllCubeParts.FindAll(x => Mathf.Round(x.transform.localPosition.x) == 0);
        }
    }
    List<GameObject> BackPart
    {
        get
        {
            return AllCubeParts.FindAll(x => Mathf.Round(x.transform.localPosition.x) == -2);
        }
    }
    List<GameObject> LeftPart
    {
        get
        {
            return AllCubeParts.FindAll(x => Mathf.Round(x.transform.localPosition.z) == 0);
        }
    }
    List<GameObject> RightPart
    {
        get
        {
            return AllCubeParts.FindAll(x => Mathf.Round(x.transform.localPosition.z) == 2);
        }
    }
    List<GameObject> UpHorizontalParts
    {
        get
        {
            return AllCubeParts.FindAll(x => Mathf.Round(x.transform.localPosition.x) == -1);
        }
    }
    List<GameObject> UpVerticalParts
    {
        get
        {
            return AllCubeParts.FindAll(x => Mathf.Round(x.transform.localPosition.z) == 1);
        }
    }
    List<GameObject> FrontHorizontalParts
    {
        get
        {
            return AllCubeParts.FindAll(x => Mathf.Round(x.transform.localPosition.y) == -1);
        }
    }

    Vector3[] RotationVectors =
    {
        new Vector3(0, 1, 0), new Vector3(0, -1, 0), new Vector3(0, 0, -1), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(-1, 0, 0)
    };

    void Start()
    {
        CreateCube();
        StartCoroutine(Shuffle());
    }

    void CreateCube()
    {
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                for (int z = 0; z < 3; z++)
                {
                    GameObject part = Instantiate(PartOfCube, MainCubeTrans, false);
                    part.transform.SetParent(MainCubeTrans);
                    part.transform.localPosition = new Vector3(-x, -y, z);
                    part.GetComponent<RubikPref>().SetColor(-x, -y, z);
                    AllCubeParts.Add(part);
                }

        CubeCenterPart = AllCubeParts[13];
    }

    IEnumerator Shuffle()
    {
        yield return new WaitForSeconds(1.5f);
        canShuffle = false;
        for (int moveCount = 3; moveCount >= 0; moveCount--)
        {
            int edge = Random.Range(0, 6);
            List<GameObject> edgeParts = new List<GameObject>();

            switch(edge)
            {
                case 0: edgeParts = UpPart; break;
                case 1: edgeParts = DownPart; break;
                case 2: edgeParts = LeftPart; break;
                case 3: edgeParts = RightPart; break;
                case 4: edgeParts = FrontPart; break;
                case 5: edgeParts = BackPart; break;
            }

            StartCoroutine(Rotate(edgeParts, RotationVectors[edge], 5));
            yield return new WaitForSeconds(0.4f);
        }
        canShuffle = true;
    }

    IEnumerator Rotate(List<GameObject> parts, Vector3 rotationVec, int speed = 3)
    {
        canRotate = false;
        int angle = 0;

        while(angle < 90)
        {
            foreach (GameObject part in parts)
            {
                part.transform.RotateAround(CubeCenterPart.transform.position, rotationVec, speed);             
            }
            angle += speed;
            yield return null;
        }

        CheckComplete();
        canRotate = true;
    }    

    public void DetectRotate(List<GameObject> parts, List<GameObject> planes)
    {
        if(!canRotate || !canShuffle) return;

        if (UpVerticalParts.Exists(x => x == parts[0]) &&
            UpVerticalParts.Exists(x => x == parts[1]))
            StartCoroutine(Rotate(UpVerticalParts, new Vector3(0, 0, 1 * DetectLeftMiddleRightSing(parts))));

        else if (UpHorizontalParts.Exists(x => x == parts[0]) &&
            UpHorizontalParts.Exists(x => x == parts[1]))
            StartCoroutine(Rotate(UpHorizontalParts, new Vector3(1 * DetectFrontMiddleBackSing(parts), 0, 0)));

        else if (FrontHorizontalParts.Exists(x => x == parts[0]) &&
            FrontHorizontalParts.Exists(x => x == parts[1]))
            StartCoroutine(Rotate(FrontHorizontalParts, new Vector3(0, 1 * DetectUpMiddleDownSing(parts), 0)));

        else if (DetectSide(planes, new Vector3(1, 0, 0), new Vector3(0, 0, 1), UpPart)) StartCoroutine(Rotate(UpPart, new Vector3(0, 1 * DetectUpMiddleDownSing(parts), 0)));
        else if (DetectSide(planes, new Vector3(1, 0, 0), new Vector3(0, 0, 1), DownPart)) StartCoroutine(Rotate(DownPart, new Vector3(0, 1 * DetectUpMiddleDownSing(parts), 0)));
        else if (DetectSide(planes, new Vector3(0, 0, 1), new Vector3(0, 1, 0), FrontPart)) StartCoroutine(Rotate(FrontPart, new Vector3(1 * DetectFrontMiddleBackSing(parts), 0, 0)));
        else if (DetectSide(planes, new Vector3(0, 0, 1), new Vector3(0, 1, 0), BackPart)) StartCoroutine(Rotate(BackPart, new Vector3(1 * DetectFrontMiddleBackSing(parts), 0, 0)));
        else if (DetectSide(planes, new Vector3(1, 0, 0), new Vector3(0, 1, 0), LeftPart)) StartCoroutine(Rotate(LeftPart, new Vector3(0, 0, 1 * DetectLeftMiddleRightSing(parts))));
        else if (DetectSide(planes, new Vector3(1, 0, 0), new Vector3(0, 1, 0), RightPart)) StartCoroutine(Rotate(RightPart, new Vector3(0, 0, 1 * DetectLeftMiddleRightSing(parts))));
    }

    bool DetectSide(List<GameObject> planes, Vector3 fDirection, Vector3 sDirection, List<GameObject> side)
    {
        GameObject centerPart = side.Find(x => x.GetComponent<RubikPref>().Planes.FindAll(y => y.activeInHierarchy).Count == 1);
        List<RaycastHit> hit1 = new List<RaycastHit>(Physics.RaycastAll(planes[1].transform.position, fDirection)),
                         hit2 = new List<RaycastHit>(Physics.RaycastAll(planes[0].transform.position, fDirection)),
                         hit1_m = new List<RaycastHit>(Physics.RaycastAll(planes[1].transform.position, -fDirection)),
                         hit2_m = new List<RaycastHit>(Physics.RaycastAll(planes[0].transform.position, -fDirection)),

                         hit3 = new List<RaycastHit>(Physics.RaycastAll(planes[1].transform.position, sDirection)),
                         hit4 = new List<RaycastHit>(Physics.RaycastAll(planes[0].transform.position, sDirection)),
                         hit3_m = new List<RaycastHit>(Physics.RaycastAll(planes[1].transform.position, -sDirection)),
                         hit4_m = new List<RaycastHit>(Physics.RaycastAll(planes[0].transform.position, -sDirection));

        return hit1.Exists(x => x.collider.gameObject == centerPart) ||
               hit2.Exists(x => x.collider.gameObject == centerPart) ||
               hit1_m.Exists(x => x.collider.gameObject == centerPart) ||
               hit2_m.Exists(x => x.collider.gameObject == centerPart) ||

               hit3.Exists(x => x.collider.gameObject == centerPart) ||
               hit4.Exists(x => x.collider.gameObject == centerPart) ||
               hit3_m.Exists(x => x.collider.gameObject == centerPart) ||
               hit4_m.Exists(x => x.collider.gameObject == centerPart);
    }

    float DetectLeftMiddleRightSing(List<GameObject> parts)
    {
        float sing = 0;

        if(Mathf.Round(parts[1].transform.position.y) != Mathf.Round(parts[0].transform.position.y))
        {
            if (Mathf.Round(parts[0].transform.position.x) == -2)
                sing = Mathf.Round(parts[0].transform.position.y) - Mathf.Round(parts[1].transform.position.y);
            else
                sing = Mathf.Round(parts[1].transform.position.y) - Mathf.Round(parts[0].transform.position.y);
        }
        else
        {
            if (Mathf.Round(parts[0].transform.position.y) == -2)
                sing = Mathf.Round(parts[1].transform.position.x) - Mathf.Round(parts[0].transform.position.x);
            else
                sing = Mathf.Round(parts[0].transform.position.x) - Mathf.Round(parts[1].transform.position.x);
        }
        return sing;
    }

    float DetectFrontMiddleBackSing(List<GameObject> parts)
    {
        float sing = 0;

        if (Mathf.Round(parts[1].transform.position.z) != Mathf.Round(parts[0].transform.position.z))
        {
            if (Mathf.Round(parts[0].transform.position.y) == 0)
                sing = Mathf.Round(parts[1].transform.position.z) - Mathf.Round(parts[0].transform.position.z);
            else
                sing = Mathf.Round(parts[0].transform.position.z) - Mathf.Round(parts[1].transform.position.z);
        }
        else
        {
            if (Mathf.Round(parts[0].transform.position.z) == 0)
                sing = Mathf.Round(parts[1].transform.position.y) - Mathf.Round(parts[0].transform.position.y);
            else
                sing = Mathf.Round(parts[0].transform.position.y) - Mathf.Round(parts[1].transform.position.y);
        }
        return sing;
    }

    float DetectUpMiddleDownSing(List<GameObject> parts)
    {
        float sing = 0;

        if (Mathf.Round(parts[1].transform.position.z) != Mathf.Round(parts[0].transform.position.z))
        {
            if (Mathf.Round(parts[0].transform.position.x) == -2)
                sing = Mathf.Round(parts[1].transform.position.z) - Mathf.Round(parts[0].transform.position.z);
            else
                sing = Mathf.Round(parts[0].transform.position.z) - Mathf.Round(parts[1].transform.position.z);
        }
        else
        {
            if (Mathf.Round(parts[0].transform.position.z) == 0)
                sing = Mathf.Round(parts[0].transform.position.x) - Mathf.Round(parts[1].transform.position.x);
            else
                sing = Mathf.Round(parts[1].transform.position.x) - Mathf.Round(parts[0].transform.position.x);
        }
        return sing;
    }

    void CheckComplete()
    {
        if (IsSideComplete(UpPart) && IsSideComplete(DownPart) && IsSideComplete(FrontPart) && IsSideComplete(BackPart) && IsSideComplete(LeftPart) && IsSideComplete(RightPart)) Debug.Log("Done");
    }

    bool IsSideComplete(List<GameObject> parts)
    {
        int mainPlaneIndex = parts[4].GetComponent<RubikPref>().Planes.FindIndex(x => x.activeInHierarchy);

        for (int i = 0; i < parts.Count; i++)
        {
            if (!parts[i].GetComponent<RubikPref>().Planes[mainPlaneIndex].activeInHierarchy ||
                parts[i].GetComponent<RubikPref>().Planes[mainPlaneIndex].GetComponent<Renderer>().material.color !=
                parts[4].GetComponent<RubikPref>().Planes[mainPlaneIndex].GetComponent<Renderer>().material.color) return false;
        }
        return true;
    }
}
