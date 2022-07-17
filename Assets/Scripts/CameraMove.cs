using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    Vector3 localRotation;
    bool cameraDisabled = false, 
         rotateDisabled = false;
    public RubikManager RubikMan;
    List<GameObject> parts = new List<GameObject>(), 
                     planes = new List<GameObject>();

    private void LateUpdate()
    {
        if(Input.GetMouseButton(0))
        {
            if(!rotateDisabled)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit, 100))
                {
                    cameraDisabled = true;

                    if(parts.Count < 2 && 
                        !parts.Exists(x => x == hit.collider.transform.parent.gameObject) &&
                        hit.transform.parent.gameObject != RubikMan.gameObject)
                    {
                        parts.Add(hit.collider.transform.parent.gameObject);
                        planes.Add(hit.collider.gameObject);
                    }
                    else if(parts.Count == 2)
                    {
                        RubikMan.DetectRotate(parts, planes);
                        rotateDisabled = true;
                    }
                }
            }
            if(!cameraDisabled)
            {
                rotateDisabled = true;
                localRotation.x += Input.GetAxis("Mouse X") * 15;
                localRotation.y += Input.GetAxis("Mouse Y") * -15;
                localRotation.y = Mathf.Clamp(localRotation.y, -90, 90);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            parts.Clear();
            planes.Clear();
            cameraDisabled = rotateDisabled = false;
        }

        Quaternion qt = Quaternion.Euler(localRotation.y, localRotation.x, 0);
        transform.parent.rotation = Quaternion.Lerp(transform.parent.rotation, qt, Time.deltaTime * 15);
    }
}
