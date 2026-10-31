using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    [SerializeField]
    private float DragSpeed, LeftBoundary, RightBoundary, TopBoundary, BottomBoundry;
    void Start()
    {

    }
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            var np = new Vector3();
            np.x = Input.GetAxis("Mouse X") * DragSpeed * Time.deltaTime;
            np.y = Input.GetAxis("Mouse Y") * DragSpeed * Time.deltaTime;
            if (gameObject.transform.position.x > -LeftBoundary && gameObject.transform.position.x < RightBoundary && gameObject.transform.position.y > -BottomBoundry && gameObject.transform.position.y < TopBoundary)
            {
                transform.Translate(np);
            }
            if (gameObject.transform.position.x > -LeftBoundary || gameObject.transform.position.x < RightBoundary || gameObject.transform.position.y > -BottomBoundry || gameObject.transform.position.y < TopBoundary)
            {
                transform.Translate(-np);
            }
        }
    }

}
