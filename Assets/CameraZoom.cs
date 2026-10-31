using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private Camera c;
    private float zoomDistance;

    [SerializeField]
    private float multiplier = 2f, minZoom = 1f, maxZoom = 10f, smoothTime = .1f;
    private float velocity = 0f;

    void Start()
    {
        c = GetComponent<Camera>();
        zoomDistance = c.orthographicSize;
    }

    void Update()
    {
        zoomDistance -= Input.GetAxisRaw("Mouse ScrollWheel") * multiplier;
        zoomDistance = Mathf.Clamp(zoomDistance, minZoom, maxZoom);
        c.orthographicSize = Mathf.SmoothDamp(c.orthographicSize, zoomDistance, ref velocity, smoothTime);
    }


}
